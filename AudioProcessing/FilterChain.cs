using System;
using System.Collections.Generic;
using System.Text;

namespace AudioProcessing
{
    public class FilterChain : IAudioFilter
    {
        private string name;
        private bool initialised ;
        private List<IAudioFilter> filters;
        public FilterChain()
        {
            initialised = false;
            filters = new List<IAudioFilter>();
        }

        public List<IAudioFilter> Filters { get => filters; }

        // IAudioFilter interface implementation
        public int GetDelay()
        {
            int delay = 0;

            foreach(IAudioFilter filter in filters)
            {
                delay += filter.GetDelay();
            }
            return delay;
        }

        public int GetOutputChannelCount()
        {
            if (filters.Count < 1) return 0;
            return filters[filters.Count - 1].GetOutputChannelCount();
        }

        public int GetOutputSampleRate()
        {
            if (filters.Count < 1) return 0;
            return filters[filters.Count - 1].GetOutputSampleRate();
        }

        public int Init(int iInSampleRate, int iInChannels)
        {
            int errorCode = 0;
            foreach (IAudioFilter filter in filters)
            {
                errorCode = filter.Init(iInSampleRate, iInChannels);

                if (errorCode != 0)
                    break;

                iInSampleRate = filter.GetOutputSampleRate();
                iInChannels = filter.GetOutputChannelCount();
            }
            if (errorCode == 0)
                initialised = true;
            return errorCode;
        }

        public int ProcessFrames(float[] pfFrames, int iSampleCount)
        {
            int res = 0;
            foreach (IAudioFilter filter in Filters)
            {
                res = filter.ProcessFrames(pfFrames, iSampleCount);
                if (res != 0)
                    break;
            }

            return res;
        }

        public int Terminate()
        {
            foreach (IAudioFilter filter in filters)
            {
                filter.Terminate();
            }
            initialised = false;
            return 0;
        }
        public string Name { get => name; set => name = value; }

    }
}
