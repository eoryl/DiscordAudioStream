using System;
using System.Collections.Generic;
using System.Text;

namespace AudioProcessing
{
    interface IAudioSource
    {
		int Init();
		int Terminate();
		int GetChannelCount();
		int GetSampleRate();

		int GetFrames(float [] pfFrames, int iCount, ref int iActualCount);

    }
}
