using System;
using System.Collections.Generic;
using System.Text;
using NAudio.Wave;
using System.Linq;

namespace DiscordAudioStreamService
{
    class AudioBufferConverter
    {

        public static void ConvertBitDepthTo16bit(byte[] buffer, int len, int bitPerSample, ref short[] outbuffer, ref int outlen)
        {

            if (bitPerSample == 8)
            {
                outlen = len;
                outbuffer = new short[outlen];
                for (int i = 0; i < outlen; i++)
                {
                    outbuffer[i] = buffer[i];
                    outbuffer[i] <<= 8;
                }
            }
            else if (bitPerSample == 16)
            {
                outlen = len / 2;
                outbuffer = new short[outlen];
                for (int i = 0; i < outlen; i++)
                {
                    //outbuffer[i] = (short)((buffer[2 * i]) << 8);
                    //outbuffer[i] |= (short) buffer[2 * i + 1];

                    outbuffer[i] = (short)(buffer[2 * i] | buffer[2 * i + 1] << 8);
                }

            }
            else if (bitPerSample == 24)
            {
                outlen = len / 3;
                outbuffer = new short[outlen];
                for (int i = 0; i < outlen ; i++)
                {
                    //outbuffer[i] = buffer[i * 3];
                    //outbuffer[i] <<= 8;
                    //outbuffer[i] += buffer[(i * 3) + 1];

                    outbuffer[i] = (short)(buffer[3 * i] | buffer[3 * i + 1] << 8);
                }
            }
            else if (bitPerSample == 32)
            {
                outlen = len / 4;
                outbuffer = new short[outlen];

                //according to NAudio developper WaveBuffer is faster
                WaveBuffer waveBuffer = new WaveBuffer(len);
                waveBuffer.BindTo(buffer);
                for (int i = 0; i < outlen; i++)
                {
                    outbuffer[i] = (short)(waveBuffer.FloatBuffer[i] * short.MaxValue);
                }

                //float[] bufferf = new float[outlen];
                //Buffer.BlockCopy(buffer, 0, bufferf, 0, len);
                //for (int i = 0; i < outlen; i++)
                //{
                //    float sample = bufferf[i];
                //    outbuffer[i] = (short) (sample * short.MaxValue);
                //}


                //for (int i = 0; i < outlen; i++)
                //{
                //    float sample = BitConverter.ToSingle(buffer, i* 4);
                //    outbuffer[i] = (short)(sample * short.MaxValue);
                //}

            }

        }

        //public void ConvertByteAraryToFloat(float buffer, int len, ref byte [] outBuffer, ref int outLen)
        //{
        //    // todo implement len management use Array.Copy 
        //    outBuffer = BitConverter.GetBytes(buffer).ToArray() ;
        //    outLen = outBuffer.Length;
        //}

        public static void ConvertShortArrayToByte(short[] buffer, int len,  ref byte[] outbuffer, ref int outlen)
        {
            outlen = len * 2;
            outbuffer = new byte[outlen];

            for (int i = 0; i < len; i++)
            {
                outbuffer[i * 2] = (byte)(buffer[i] & 0xff);
                outbuffer[(i * 2) + 1] = (byte)(buffer[i] >> 8);
            }

            // Alternatives are Array.Copy, BitConverter or Buffer.BlockCoopy
            //Buffer.BlockCopy(buffer, 0, outbuffer, 0,  len);

        }

        //public void ConvertFloatArrayToByte(float[] buffer, int len, ref byte[] outbuffer, ref int outlen)
        //{
        //    outlen = len * 4;
        //    outbuffer = new byte[outlen];
        //    for (int i = 0; i < len; i++)
        //    {
        //        float fsample = buffer[i];
        //        short sample = (short)((fsample < 0.0f) ? (fsample * short.MinValue) : (fsample * short.MinValue));
        //        outbuffer[i * 2] = (byte)(sample & 0xff);
        //        outbuffer[(i * 2) + 1] = (byte)(sample >> 8);
        //    }
        //}

        public static void MonoToStereo(byte [] buffer, int bufferlen, ref byte [] outbuffer, ref int outbufferlen)
        {
            bufferlen -= (bufferlen % 2);
            outbufferlen = bufferlen * 2;
            outbuffer = new byte[outbufferlen];

            for (int i =0;i<bufferlen;i+=2)
            {
                outbuffer[(i * 2)    ] = buffer[i];
                outbuffer[(i * 2) + 1] = buffer[i+1];
                outbuffer[(i * 2) + 2] = buffer[i];
                outbuffer[(i * 2) + 3] = buffer[i+1];
            }
        }

    }
}
