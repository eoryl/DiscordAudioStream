using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using System.Threading;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.IO;
using SharpDX.DirectSound;
using System.Diagnostics;
using SharpDX.Mathematics.Interop;

namespace DiscordAudioStreamService
{
    class DirectSoundCaptureWaveIn : IWaveIn
    {
        public NAudio.Wave.WaveFormat WaveFormat { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        NAudio.Wave.WaveFormat IWaveIn.WaveFormat { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event EventHandler<WaveInEventArgs> DataAvailable;
        public event EventHandler<StoppedEventArgs> RecordingStopped;

        public void StartRecording()
        {
            throw new NotImplementedException();
        }

        public void StopRecording()
        {
            throw new NotImplementedException();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DirectSoundCapture()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
