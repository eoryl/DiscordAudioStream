using System;
using System.Collections.Generic;
using System.Text;

namespace AudioProcessing
{
    interface IAudioDestination
    {
        int Init(int iSampleRate, int iChannels);
        int Terminate();
        int ProcessFrames(float[] pfFrames, int iSampleCount);
    }
}
