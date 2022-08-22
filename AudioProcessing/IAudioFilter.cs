using System;
using System.Collections.Generic;
using System.Text;

namespace AudioProcessing
{
    public interface IAudioFilter 
    {
        int Init(int iInSampleRate, int iInChannels);
        int Terminate();
        int GetOutputChannelCount();
        int GetOutputSampleRate();
        int GetDelay();
        int ProcessFrames(float[] pfFrames, int iSampleCount);

        string Name
        {
            get;
            set;
        }

    }
}
