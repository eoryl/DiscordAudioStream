/*
 * 
This class is a port to C# of the this C++ version that can be found here:
https://github.com/tcarpent/PeakLimiter

The original work this file is derived from implies the following for this file :

Copyright (c) 2016, UMR STMS 9912 - Ircam-Centre Pompidou / CNRS / UPMC
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
    * Redistributions of source code must retain the above copyright
      notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright
      notice, this list of conditions and the following disclaimer in the
      documentation and/or other materials provided with the distribution.
    * Neither the name of the <organization> nor the
      names of its contributors may be used to endorse or promote products
      derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace AudioProcessing
{
    unsafe public class PeakLimiter
    {

        public int m_attack;
        public float m_attackConst, m_releaseConst;
        public float m_attackMs, m_releaseMs, m_maxAttackMs;
        public float m_threshold;
        public int m_channels, m_maxChannels;
        public int m_sampleRate, m_maxSampleRate;
        public float m_fadedGain;
        public float* m_pMaxBuffer;
        public float* m_pMaxBufferSlow;
        public float* m_pDelayBuffer;
        public int m_maxBufferIndex, m_maxBufferSlowIndex, m_delayBufferIndex;
        public int m_sectionLen, m_nbrMaxBufferSection;
        public int m_maxBufferSectionIndex, m_maxBufferSectionCounter;
        public float m_smoothState;
        public float m_maxMaxBufferSlow, m_maxCurrentSection;
        public int m_indexMaxBufferSlow;
        public int* m_pIndexMaxInSection;

        const int LIMITER_OK = 0;
        const int LIMITER_INVALID_PARAMETER = 1;

        int min(int a, int b)
        {
            return Math.Min(a, b);
        }

        int max(int a, int b)
        {
            return Math.Max(a, b);
        }

        float min(float a, float b)
        {
            return Math.Min(a, b);
        }
        float max(float a, float b)
        {
            return Math.Max(a, b);
        }
        
        float pow(float a, float b)
        {
            return (float) Math.Pow(a,b);
        }

        double pow(double a, double b)
        {
            return Math.Pow(a, b);
        }

        float sqrt(float a)
        {
            return (float) Math.Sqrt((float)a);
        }

        double log10(double a)
        {
            return Math.Log10(a);
        }

        double fabs(double a)
        {
            return Math.Abs(a);        
        }

        [DllImport("msvcrt.dll",
        EntryPoint = "memset",
        CallingConvention = CallingConvention.Cdecl,
        SetLastError = false)]
        public static extern IntPtr memset(IntPtr dest, int c, int count);


        [DllImport("msvcrt.dll",
        EntryPoint = "memcpy",
        CallingConvention = CallingConvention.Cdecl,
        SetLastError = false)]
        public static extern IntPtr memcpy(IntPtr dest, IntPtr src, int count);

        /* create limiter */
       public  PeakLimiter(
            float maxAttackMsIn,
            float releaseMsIn,
            float thresholdIn,
            int maxChannelsIn,
            int maxSampleRateIn
            )
        {

            /* calc m_attack time in samples */
            m_attack = (int)(maxAttackMsIn * maxSampleRateIn / 1000);

            if (m_attack < 1) /* m_attack time is too short */

                m_attack = 1;

            /* length of m_pMaxBuffer sections */
            m_sectionLen = (int)sqrt((float)m_attack + 1);
            /* sqrt(m_attack+1) leads to the minimum 
                of the number of maximum operators:
                nMaxOp = m_sectionLen + (m_attack+1)/m_sectionLen */

            /* alloc limiter struct */

            m_nbrMaxBufferSection = (m_attack + 1) / m_sectionLen;
            if (m_nbrMaxBufferSection * m_sectionLen < (m_attack + 1))
                m_nbrMaxBufferSection++; /* create a full section for the last samples */

            m_pMaxBuffer = (float *) Marshal.AllocHGlobal(m_nbrMaxBufferSection * m_sectionLen * sizeof(float));
            m_pDelayBuffer = (float*)Marshal.AllocHGlobal(m_attack * maxChannelsIn * sizeof(float));
            m_pMaxBufferSlow = (float*)Marshal.AllocHGlobal(m_nbrMaxBufferSection * sizeof(float));
            m_pIndexMaxInSection = (int*)Marshal.AllocHGlobal(m_nbrMaxBufferSection * sizeof(int));

            if ((m_pMaxBuffer == (float *) IntPtr.Zero ) || (m_pDelayBuffer == (float*)IntPtr.Zero) || (m_pMaxBufferSlow == (float*)IntPtr.Zero))
            {
                destroyLimiter();
                return;
            }

            /* init parameters & states */
            m_maxBufferIndex = 0;
            m_delayBufferIndex = 0;
            m_maxBufferSlowIndex = 0;
            m_maxBufferSectionIndex = 0;
            m_maxBufferSectionCounter = 0;
            m_maxMaxBufferSlow = 0;
            m_indexMaxBufferSlow = 0;
            m_maxCurrentSection = 0;

            m_attackMs = maxAttackMsIn;
            m_maxAttackMs = maxAttackMsIn;
            m_attackConst = (float)pow(0.1, 1.0 / (m_attack + 1));
            m_releaseConst = (float)pow(0.1, 1.0 / (m_releaseMs * maxSampleRateIn / 1000 + 1));
            m_threshold = thresholdIn;
            m_channels = maxChannelsIn;
            m_maxChannels = maxChannelsIn;
            m_sampleRate = maxSampleRateIn;
            m_maxSampleRate = maxSampleRateIn;


            m_fadedGain = 1.0f;
            m_smoothState = 1.0f;

            memset((IntPtr) m_pMaxBuffer, 0, sizeof(float) * m_nbrMaxBufferSection * m_sectionLen);
            memset((IntPtr)m_pDelayBuffer, 0, sizeof(float) * m_attack * maxChannelsIn);
            memset((IntPtr)m_pMaxBufferSlow, 0, sizeof(float) * m_nbrMaxBufferSection);
            memset((IntPtr)m_pIndexMaxInSection, 0, sizeof(int) * m_nbrMaxBufferSection);
        }


        ~PeakLimiter()
        {
            destroyLimiter();
        }

        /* reset limiter */
        public int resetLimiter()
        {

            m_maxBufferIndex = 0;
            m_delayBufferIndex = 0;
            m_maxBufferSlowIndex = 0;
            m_maxBufferSectionIndex = 0;
            m_maxBufferSectionCounter = 0;
            m_fadedGain = 1.0f;
            m_smoothState = 1.0f;
            m_maxMaxBufferSlow = 0;
            m_indexMaxBufferSlow = 0;
            m_maxCurrentSection = 0;


            memset((IntPtr)m_pMaxBuffer, 0, sizeof(float) * m_nbrMaxBufferSection * m_sectionLen);
            memset((IntPtr)m_pDelayBuffer, 0, sizeof(float) * m_attack * m_maxChannels);
            memset((IntPtr)m_pMaxBufferSlow, 0, sizeof(float) * m_nbrMaxBufferSection);
            memset((IntPtr)m_pIndexMaxInSection, 0, sizeof(int) * m_nbrMaxBufferSection);

            return LIMITER_OK;
        }


        /* destroy limiter */
        public int destroyLimiter()
        {
            if (m_pMaxBuffer != (float*)IntPtr.Zero)
            {
                Marshal.FreeHGlobal((IntPtr)m_pMaxBuffer);
                m_pMaxBuffer = (float*)IntPtr.Zero;
            }
            if (m_pDelayBuffer != (float*)IntPtr.Zero)
            {
                Marshal.FreeHGlobal((IntPtr)m_pDelayBuffer);
                m_pDelayBuffer = (float*)IntPtr.Zero;
            }
            if (m_pMaxBufferSlow != (float*)IntPtr.Zero)
            {
                Marshal.FreeHGlobal((IntPtr)m_pMaxBufferSlow);
                m_pMaxBufferSlow = (float*)IntPtr.Zero;
            }
            if (m_pIndexMaxInSection != (int*)IntPtr.Zero)
            {
                Marshal.FreeHGlobal((IntPtr) m_pIndexMaxInSection);
                m_pIndexMaxInSection = (int*)IntPtr.Zero;
            }
            return LIMITER_OK;
        }

        /* apply limiter */
        public int applyLimiter_E(float* samplesIn, float* samplesOut, int nSamples)
        {
            memcpy((IntPtr) samplesOut, (IntPtr)samplesIn, nSamples * sizeof(float));
            return applyLimiter_E_I(samplesOut, nSamples);
        }

        /* apply limiter */
        public int applyLimiter_E_I(float* samples, int nSamples)
        {
            int i, j;
            float tmp, gain, maximum;

            for (i = 0; i < nSamples; i++)
            {
                /* get maximum absolute sample value of all channels that are greater in absoulte value to m_threshold */
                m_pMaxBuffer[m_maxBufferIndex] = m_threshold;
                for (j = 0; j < m_channels; j++)
                {
                    m_pMaxBuffer[m_maxBufferIndex] = max(m_pMaxBuffer[m_maxBufferIndex], (float)fabs(samples[i * m_channels + j]));
                }

                /* search maximum in the current section */
                if (m_pIndexMaxInSection[m_maxBufferSlowIndex] == m_maxBufferIndex) // if we have just changed the sample containg the old maximum value
                {
                    // need to compute the maximum on the whole section 
                    m_maxCurrentSection = m_pMaxBuffer[m_maxBufferSectionIndex];
                    for (j = 1; j < m_sectionLen; j++)
                    {
                        if (m_pMaxBuffer[m_maxBufferSectionIndex + j] > m_maxCurrentSection)
                        {
                            m_maxCurrentSection = m_pMaxBuffer[m_maxBufferSectionIndex + j];
                            m_pIndexMaxInSection[m_maxBufferSlowIndex] = m_maxBufferSectionIndex + j;
                        }
                    }
                }
                else // just need to compare the new value the cthe current maximum value
                {
                    if (m_pMaxBuffer[m_maxBufferIndex] > m_maxCurrentSection)
                    {
                        m_maxCurrentSection = m_pMaxBuffer[m_maxBufferIndex];
                        m_pIndexMaxInSection[m_maxBufferSlowIndex] = m_maxBufferIndex;
                    }
                }

                // find maximum of slow (downsampled) max buffer
                maximum = m_maxMaxBufferSlow;
                if (m_maxCurrentSection > maximum)
                {
                    maximum = m_maxCurrentSection;
                }

                m_maxBufferIndex++;
                m_maxBufferSectionCounter++;

                /* if m_pMaxBuffer section is finished, or end of m_pMaxBuffer is reached,
                store the maximum of this section and open up a new one */
                if ((m_maxBufferSectionCounter >= m_sectionLen) || (m_maxBufferIndex >= m_attack + 1))
                {
                    m_maxBufferSectionCounter = 0;

                    tmp = m_pMaxBufferSlow[m_maxBufferSlowIndex] = m_maxCurrentSection;
                    j = 0;
                    if (m_indexMaxBufferSlow == m_maxBufferSlowIndex)
                    {
                        j = 1;
                    }
                    m_maxBufferSlowIndex++;
                    if (m_maxBufferSlowIndex >= m_nbrMaxBufferSection)
                    {
                        m_maxBufferSlowIndex = 0;
                    }
                    if (m_indexMaxBufferSlow == m_maxBufferSlowIndex)
                    {
                        j = 1;
                    }
                    m_maxCurrentSection = m_pMaxBufferSlow[m_maxBufferSlowIndex];
                    m_pMaxBufferSlow[m_maxBufferSlowIndex] = 0.0f;  /* zero out the value representing the new section */

                    /* compute the maximum over all the section */
                    if (j != 0)
                    {
                        m_maxMaxBufferSlow = 0;
                        for (j = 0; j < m_nbrMaxBufferSection; j++)
                        {
                            if (m_pMaxBufferSlow[j] > m_maxMaxBufferSlow)
                            {
                                m_maxMaxBufferSlow = m_pMaxBufferSlow[j];
                                m_indexMaxBufferSlow = j;
                            }
                        }
                    }
                    else
                    {
                        if (tmp > m_maxMaxBufferSlow)
                        {
                            m_maxMaxBufferSlow = tmp;
                            m_indexMaxBufferSlow = m_maxBufferSlowIndex;
                        }
                    }

                    m_maxBufferSectionIndex += m_sectionLen;
                }

                if (m_maxBufferIndex >= (m_attack + 1))
                {
                    m_maxBufferIndex = 0;
                    m_maxBufferSectionIndex = 0;
                }

                /* needed current gain */
                if (maximum > m_threshold)
                {
                    gain = m_threshold / maximum;
                }
                else
                {
                    gain = 1;
                }

                /*avoid overshoot */

                if (gain < m_smoothState)
                {
                    m_fadedGain = min(m_fadedGain, (gain - 0.1f * (float)m_smoothState) * 1.11111111f);
                }
                else
                {
                    m_fadedGain = gain;
                }


                /* smoothing gain */
                if (m_fadedGain < m_smoothState)
                {
                    m_smoothState = m_attackConst * (m_smoothState - m_fadedGain) + m_fadedGain;  /* m_attack */
                    /*avoid overshoot */
                    if (gain > m_smoothState)
                    {
                        m_smoothState = gain;
                    }
                }
                else
                {
                    m_smoothState = m_releaseConst * (m_smoothState - m_fadedGain) + m_fadedGain; /* release */
                }

                /* fill delay line, apply gain */
                for (j = 0; j < m_channels; j++)
                {
                    tmp = m_pDelayBuffer[m_delayBufferIndex * m_channels + j];
                    m_pDelayBuffer[m_delayBufferIndex * m_channels + j] = samples[i * m_channels + j];

                    tmp *= m_smoothState;
                    if (tmp > m_threshold) tmp = m_threshold;
                    if (tmp < -m_threshold) tmp = -m_threshold;

                    samples[i * m_channels + j] = tmp;
                }

                m_delayBufferIndex++;
                if (m_delayBufferIndex >= m_attack)
                    m_delayBufferIndex = 0;

            }

            return LIMITER_OK;
        }

        /* apply limiter */
        public int applyLimiter(float** samplesIn, float** samplesOut, int nSamples)
        {
            int ind;
            for (ind = 0; ind < m_channels; ind++)
            {
                memcpy((IntPtr)samplesOut[ind], (IntPtr)samplesIn[ind], nSamples * sizeof(float));
            }
            return applyLimiter_I(samplesOut, nSamples);
        }


        /* apply limiter */
        public int applyLimiter_I(float** samples, int nSamples)
        {
            int i, j;
            float tmp, gain, maximum;

            for (i = 0; i < nSamples; i++)
            {
                /* get maximum absolute sample value of all channels that are greater in absoulte value to m_threshold */
                m_pMaxBuffer[m_maxBufferIndex] = m_threshold;
                for (j = 0; j < m_channels; j++)
                {
                    m_pMaxBuffer[m_maxBufferIndex] = max(m_pMaxBuffer[m_maxBufferIndex], (float)fabs(samples[j][i]));
                }

                /* search maximum in the current section */
                if (m_pIndexMaxInSection[m_maxBufferSlowIndex] == m_maxBufferIndex) // if we have just changed the sample containg the old maximum value
                {
                    // need to compute the maximum on the whole section 
                    m_maxCurrentSection = m_pMaxBuffer[m_maxBufferSectionIndex];
                    for (j = 1; j < m_sectionLen; j++)
                    {
                        if (m_pMaxBuffer[m_maxBufferSectionIndex + j] > m_maxCurrentSection)
                        {
                            m_maxCurrentSection = m_pMaxBuffer[m_maxBufferSectionIndex + j];
                            m_pIndexMaxInSection[m_maxBufferSlowIndex] = m_maxBufferSectionIndex + j;
                        }
                    }
                }
                else // just need to compare the new value the cthe current maximum value
                {
                    if (m_pMaxBuffer[m_maxBufferIndex] > m_maxCurrentSection)
                    {
                        m_maxCurrentSection = m_pMaxBuffer[m_maxBufferIndex];
                        m_pIndexMaxInSection[m_maxBufferSlowIndex] = m_maxBufferIndex;
                    }
                }

                // find maximum of slow (downsampled) max buffer
                maximum = m_maxMaxBufferSlow;
                if (m_maxCurrentSection > maximum)
                {
                    maximum = m_maxCurrentSection;
                }

                m_maxBufferIndex++;
                m_maxBufferSectionCounter++;

                /* if m_pMaxBuffer section is finished, or end of m_pMaxBuffer is reached,
                    store the maximum of this section and open up a new one */
                if ((m_maxBufferSectionCounter >= m_sectionLen) || (m_maxBufferIndex >= m_attack + 1))
                {
                    m_maxBufferSectionCounter = 0;

                    tmp = m_pMaxBufferSlow[m_maxBufferSlowIndex] = m_maxCurrentSection;
                    j = 0;
                    if (m_indexMaxBufferSlow == m_maxBufferSlowIndex)
                    {
                        j = 1;
                    }
                    m_maxBufferSlowIndex++;
                    if (m_maxBufferSlowIndex >= m_nbrMaxBufferSection)
                    {
                        m_maxBufferSlowIndex = 0;
                    }
                    if (m_indexMaxBufferSlow == m_maxBufferSlowIndex)
                    {
                        j = 1;
                    }
                    m_maxCurrentSection = m_pMaxBufferSlow[m_maxBufferSlowIndex];
                    m_pMaxBufferSlow[m_maxBufferSlowIndex] = 0.0f;  /* zero out the value representing the new section */

                    /* compute the maximum over all the section */
                    if (j != 0)
                    {
                        m_maxMaxBufferSlow = 0;
                        for (j = 0; j < m_nbrMaxBufferSection; j++)
                        {
                            if (m_pMaxBufferSlow[j] > m_maxMaxBufferSlow)
                            {
                                m_maxMaxBufferSlow = m_pMaxBufferSlow[j];
                                m_indexMaxBufferSlow = j;
                            }
                        }
                    }
                    else
                    {
                        if (tmp > m_maxMaxBufferSlow)
                        {
                            m_maxMaxBufferSlow = tmp;
                            m_indexMaxBufferSlow = m_maxBufferSlowIndex;
                        }
                    }

                    m_maxBufferSectionIndex += m_sectionLen;
                }

                if (m_maxBufferIndex >= (m_attack + 1))
                {
                    m_maxBufferIndex = 0;
                    m_maxBufferSectionIndex = 0;
                }

                /* needed current gain */
                if (maximum > m_threshold)
                {
                    gain = m_threshold / maximum;
                }
                else
                {
                    gain = 1;
                }

                /*avoid overshoot */

                if (gain < m_smoothState)
                {
                    m_fadedGain = min(m_fadedGain, (gain - 0.1f * (float)m_smoothState) * 1.11111111f);
                }
                else
                {
                    m_fadedGain = gain;
                }


                /* smoothing gain */
                if (m_fadedGain < m_smoothState)
                {
                    m_smoothState = m_attackConst * (m_smoothState - m_fadedGain) + m_fadedGain;  /* m_attack */
                    /*avoid overshoot */
                    if (gain > m_smoothState)
                    {
                        m_smoothState = gain;
                    }
                }
                else
                {
                    m_smoothState = m_releaseConst * (m_smoothState - m_fadedGain) + m_fadedGain; /* release */
                }

                /* fill delay line, apply gain */
                for (j = 0; j < m_channels; j++)
                {
                    tmp = m_pDelayBuffer[m_delayBufferIndex * m_channels + j];
                    m_pDelayBuffer[m_delayBufferIndex * m_channels + j] = samples[j][i];

                    tmp *= m_smoothState;
                    if (tmp > m_threshold) tmp = m_threshold;
                    if (tmp < -m_threshold) tmp = -m_threshold;

                    samples[j][i] = tmp;
                }

                m_delayBufferIndex++;
                if (m_delayBufferIndex >= m_attack)
                    m_delayBufferIndex = 0;

            }

            return LIMITER_OK;

        }

        /* get delay in samples */
        public int getLimiterDelay()
        {
            return m_attack;
        }

        /* get m_attack in Ms */
        public float getLimiterAttack()
        {
            return m_attackMs;
        }

        /* get delay in samples */
        public int getLimiterSampleRate()
        {
            return m_sampleRate;
        }

        /* get delay in samples */
        public float getLimiterRelease()
        {
            return m_releaseMs;
        }

        /* get maximum gain reduction of last processed block */
        public float getLimiterMaxGainReduction()
        {
            return -20 * (float)log10(m_smoothState);
        }

        /* set number of channels */
        public int setLimiterNChannels(int nChannelsIn)
        {
            if (nChannelsIn > m_maxChannels) return LIMITER_INVALID_PARAMETER;

            m_channels = nChannelsIn;
            resetLimiter();

            return LIMITER_OK;
        }

        /* set sampling rate */
        public int setLimiterSampleRate(int sampleRateIn)
        {
            if (sampleRateIn > m_maxSampleRate) return LIMITER_INVALID_PARAMETER;

            /* update m_attack/release constants */
            m_attack = (int)(m_attackMs * sampleRateIn / 1000);

            if (m_attack < 1) /* m_attack time is too short */
                return LIMITER_INVALID_PARAMETER;

            /* length of m_pMaxBuffer sections */
            m_sectionLen = (int)sqrt((float)m_attack + 1);

            m_nbrMaxBufferSection = (m_attack + 1) / m_sectionLen;
            if (m_nbrMaxBufferSection * m_sectionLen < (m_attack + 1))
                m_nbrMaxBufferSection++;
            m_attackConst = (float)pow(0.1, 1.0 / (m_attack + 1));
            m_releaseConst = (float)pow(0.1, 1.0 / (m_releaseMs * sampleRateIn / 1000 + 1));
            m_sampleRate = sampleRateIn;

            /* reset */
            resetLimiter();

            return LIMITER_OK;
        }

        /* set m_attack time */
        public int setLimiterAttack(float attackMsIn)
        {
            if (attackMsIn > m_maxAttackMs) return LIMITER_INVALID_PARAMETER;

            /* calculate attack time in samples */
            m_attack = (int)(attackMsIn * m_sampleRate / 1000);

            if (m_attack < 1) /* attack time is too short */
                m_attack = 1;

            /* length of m_pMaxBuffer sections */
            m_sectionLen = (int)sqrt((float)m_attack + 1);

            m_nbrMaxBufferSection = (m_attack + 1) / m_sectionLen;
            if (m_nbrMaxBufferSection * m_sectionLen < (m_attack + 1))
                m_nbrMaxBufferSection++;
            m_attackConst = (float)pow(0.1, 1.0 / (m_attack + 1));
            m_attackMs = attackMsIn;

            /* reset */
            resetLimiter();

            return LIMITER_OK;
        }

        /* set release time */
        public int setLimiterRelease(float releaseMsIn)
        {
            m_releaseConst = (float)pow(0.1, 1.0 / (releaseMsIn * m_sampleRate / 1000 + 1));
            m_releaseMs = releaseMsIn;

            return LIMITER_OK;
        }

        /* set limiter threshold */
        public int setLimiterThreshold(float thresholdIn)
        {
            m_threshold = thresholdIn;

            return LIMITER_OK;
        }

        /* set limiter threshold */
        public float getLimiterThreshold()
        {
            return m_threshold;
        }



    }
}
