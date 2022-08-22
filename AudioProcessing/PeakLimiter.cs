using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace AudioProcessing
{
    unsafe public class PeakLimiter : IAudioFilter, IDisposable
    {
        private string name;
        private IntPtr limiter;
        // To detect redundant calls
        private bool _disposed = false;

        private float attack;
        private float release;
        private float threshold;

        private int sampleRate;
        private int channelCount;


        public float Attack { get => attack; set => attack = value; }
        public float Release { get => release; set => release = value; }
        public float Threshold { get => threshold; set => threshold = value; }
        public float ThresholdDb 
        { 
            get 
            { 
                return AudioTools.linearTodBFSf(threshold); 
            } 
            set 
            {
                threshold = AudioTools.dBFSToLinearf(value);
            } 
        }

        public PeakLimiter(float attack, float release, float threshold)
        {
            limiter = IntPtr.Zero;
            this.attack = attack;
            this.release = release;
            this.threshold = threshold;
            sampleRate = 48000;
            channelCount = 2;
        }

        public PeakLimiter()
        {
            limiter = IntPtr.Zero;
            attack = 20.0f;
            release = 20.0f;
            threshold = AudioTools.dBFSToLinearf(-3.0f);
            sampleRate = 48000;
            channelCount = 2;
    }


        ~PeakLimiter() => Dispose(false);

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
            }

            Terminate();

            _disposed = true;
        }

        [DllImport("PeakLimiter.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr CreateLimiter(
            float fAttack,
            float fRelease,
            float fThreshold,
            int iChannels,
            int iSampleRate
            );

        [DllImport("PeakLimiter.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr ReleaseLimiter(IntPtr hLimiter);

        [DllImport("PeakLimiter.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int applyLimiter_E_I(IntPtr hLimiter, float* samples, int count);

        [DllImport("PeakLimiter.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern float getMaxGainReduction(IntPtr hLimiter);

        [DllImport("PeakLimiter.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int setAttack(IntPtr hLimiter, float value);

        [DllImport("PeakLimiter.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int setRelease(IntPtr hLimiter, float value);

        [DllImport("PeakLimiter.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int setThreshold(IntPtr hLimiter, float value);

        [DllImport("PeakLimiter.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int setSampleRate(IntPtr hLimiter, int value);

        [DllImport("PeakLimiter.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int setNChannels(IntPtr hLimiter, int value);

        [DllImport("PeakLimiter.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int getDelay(IntPtr hLimiter);


        // IAudioFilter interface implementation
        public int GetDelay()
        {
            if (limiter == IntPtr.Zero)
                return 0;
            return getDelay(limiter);
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
            if (limiter != IntPtr.Zero)
            {
                Terminate();
            }

            channelCount = iInChannels;
            sampleRate = iInSampleRate;
            limiter = CreateLimiter(attack, release, threshold, iInChannels, iInSampleRate);
            return 0;
        }

        public int ProcessFrames(float[] pfFrames, int iSampleCount)
        {
            if (limiter == IntPtr.Zero)
                return -1;
            if (channelCount <1)
                return -1;

            int res = 0;
            fixed (float * samples = pfFrames)
            {
                res = applyLimiter_E_I(limiter, samples, iSampleCount/channelCount);
            }

            return res;
        }

        public int Terminate()
        {
            if (limiter != IntPtr.Zero)
            {
                ReleaseLimiter(limiter);
                limiter = IntPtr.Zero;
            }
            return 0;
        }
        public string Name { get => name; set => name = value; }

    }
}
