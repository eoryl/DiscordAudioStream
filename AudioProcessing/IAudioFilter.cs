using System;
using System.Collections.Generic;
using System.Text;

namespace AudioProcessing
{
    interface IAudioFilter : IAudioDestination
    {
         int GetDelay();
    }
}
