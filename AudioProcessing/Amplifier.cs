using System;
using System.Collections.Generic;
using System.Text;

namespace AudioProcessing
{
    public class Amplifier : IAudioFilter
    {
        private int channelCount = 2;
        private int sampleRate = 0;
        private float[] gain;
        string name;

        public float[] Gain 
        { 
            get => gain; 
        }


        public Amplifier(int sampleRate, int channels, float globalGain)
        {
            this.sampleRate = sampleRate;
            channelCount = channels;
            sampleRate = 0;
            gain = new float[channelCount] ;
            for (int i = 0; i < channels; i++)
                gain[i] = globalGain;

        }

        public Amplifier(float globalGain) : this (48000,2, globalGain)
        {

        }
        public Amplifier() : this(48000,2, 1.0f)
        {
        }

        public void SetGlobalGain(float gainVal)
        {
            for (int i = 0; i < channelCount; i++)
                gain[i] = gainVal;
        }

        public void SetGlobalGainDb(float gainValdB)
        {
            for (int i = 0; i < channelCount; i++)
                gain[i] = (float) AudioTools.dBFSToLinear( gainValdB);
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
            if (iInChannels < 1)
                return -1;

            sampleRate = iInSampleRate;

            float[] newGain = new float[iInChannels];

            for(int i = 0; i<iInChannels;i++)
            {
                if (i < gain.Length)
                    newGain[i] = gain[i];
                else
                    newGain[i] = 1.0f;
            }
            gain = newGain;

            return 0;

        }

        public int ProcessFrames(float[] pfFrames, int iSampleCount)
        {
            for (int i = 0; i<iSampleCount;i++)
            {
                pfFrames[i] *= gain[i % channelCount];
            }
            return 0;
        }

        public int Terminate()
        {
            return 0;
        }


        public string Name { get => name; set => name = value; }
    }
}
