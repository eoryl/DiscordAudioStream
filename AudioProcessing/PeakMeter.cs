using System;
using System.Collections.Generic;
using System.Text;

namespace AudioProcessing
{
    public class PeakMeter : IAudioFilter
    {
        private int channelCount;
        private int sampleRate;
        private float[] peakLastPass;
        private float[] peakSinceStart;

        public float[] PeakLastPass { get => peakLastPass; }
        public float[] PeakSinceStart { get => peakSinceStart; }

        public PeakMeter()
        {
            channelCount = 2;
            sampleRate = 0;
            peakLastPass = new float[2] { 0f,0f};
            peakSinceStart = new float[2] { 0f, 0f };
        }

        public float GlobalPeakLastPass
        {
            get
            {
                float res = 0f;
                for (int i = 0; i < channelCount; i++)
                {
                    res = Math.Max(res,peakLastPass[i]);
                }
                return res;
            }
        }


        public float GlobalPeakLastPassdBFS
        {
            get 
            {
                return (float) AudioTools.linearTodBFS(GlobalPeakLastPass);
            }
        }

        public float GlobalPeakSinceStart
        {
            get
            {
                float res = 0f;
                for (int i = 0; i < channelCount; i++)
                {
                    res = Math.Max(res, peakSinceStart[i]);
                }
                return res;
            }
        }


        public float GlobalPeakSinceStartdBFS
        {
            get
            {
                return (float)AudioTools.linearTodBFS(GlobalPeakSinceStart);
            }
        }

        // IAudioFilter interface implementation

        public int GetDelay()
        {
            return 0;
        }

        public int GetOutputChannelCount()
        {
            return channelCount;
        }

        public int GetOutputSampleRate()
        {
            return sampleRate;
        }

        public int Init(int iInSampleRate, int iInChannels)
        {
            if (channelCount < 0) return -1;
            sampleRate = iInSampleRate;
            channelCount = iInChannels;
            peakLastPass = new float[channelCount];
            peakSinceStart = new float[channelCount];

            for (int i = 0;i<channelCount;i++)
            {
                peakLastPass[i] = 0f;
                peakSinceStart[i] = 0f;
            }

            return 0;

        }

        public int ProcessFrames(float[] pfFrames, int iSampleCount)
        {
            for (int i = 0; i < channelCount; i++)
            {
                peakLastPass[i] = 0f;
            }
            for (int i = 0; i < iSampleCount; i++)
            {
                peakLastPass[i % channelCount] = Math.Max(Math.Abs( pfFrames[i]), peakLastPass[i % channelCount]);
            }
            for (int i = 0; i < channelCount; i++)
            {
                peakSinceStart[i] = Math.Max( peakLastPass[i], peakSinceStart[i] );
            }

            return 0;
        }

        public int Terminate()
        {
            return 0;
        }
    }
}
