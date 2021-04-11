using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioProcessing;
using NAudio.Wave;

namespace AudioProcessingTest
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length < 0)
                return;

            PeakMeter peakMeter1 = new PeakMeter();
            Amplifier amplifier = new Amplifier(AudioTools.dBFSToLinearf(12f));
            PeakLimiter limiter = new PeakLimiter(20, 20, AudioTools.dBFSToLinearf(-3.0f));
            PeakMeter peakMeter2 = new PeakMeter();

            FilterChain filterChain = new FilterChain();

            filterChain.Filters.Add(peakMeter1);
            filterChain.Filters.Add(amplifier);
            filterChain.Filters.Add(limiter);
            filterChain.Filters.Add(peakMeter2);

            WaveFileReader waveFileReader = new WaveFileReader(args[0]);
            waveFileReader.Position = 0;
            byte[] byteBuffer = new byte[10 * 1024];
            float [] floatBuffer = null;
            int bytesRead = 0, samplesAvailable = 0;

            filterChain.Init(waveFileReader.WaveFormat.SampleRate, waveFileReader.WaveFormat.Channels);

            int processed = 0;
            while (true)
            {
                bytesRead = waveFileReader.Read(byteBuffer, 0, 10 * 1024);

                if (bytesRead == 0) break;
                AudioBufferConverter.ConvertByteArrayToFloatArray(byteBuffer, bytesRead, waveFileReader.WaveFormat.BitsPerSample, ref floatBuffer, ref samplesAvailable);

                filterChain.ProcessFrames(floatBuffer, samplesAvailable);
                processed += bytesRead;

            }
            System.Console.WriteLine("processed: " + processed);
            System.Console.WriteLine("peak1: " + peakMeter1.GlobalPeakSinceStartdBFS);
            System.Console.WriteLine("peak2: " + peakMeter2.GlobalPeakSinceStartdBFS);

            System.Console.ReadKey();
        }
    }
}
