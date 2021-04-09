using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
//using NAudio.Wave;

namespace AudioProcessing
{
    public class AudioBufferConverter
    {

        public static void ConvertByteArrayToShortArray(byte[] buffer, int len, int bitPerSample, ref short[] outbuffer, ref int outlen)
        {

            if (bitPerSample == 8)
            {
                if ((outbuffer == null) || (outlen < len))
                {
                    outbuffer = new short[outlen];
                }
                outlen = len;
                for (int i = 0; i < outlen; i++)
                {
                    outbuffer[i] = (sbyte)buffer[i];
                    outbuffer[i] <<= 8;
                }
            }
            else if (bitPerSample == 16)
            {
                if ((outbuffer == null) || (outlen < len/2))
                {
                    outbuffer = new short[outlen];
                }
                outlen = len / 2;
                for (int i = 0; i < outlen; i++)
                {
                    //outbuffer[i] = (short)((buffer[2 * i]) << 8);
                    //outbuffer[i] |= (short) buffer[2 * i + 1];

                    outbuffer[i] = (short)(buffer[2 * i] | buffer[2 * i + 1] << 8);
                }

            }
            else if (bitPerSample == 24)
            {
                //TODO: implement 24bit conversion
                //outlen = len / 3;
                //outbuffer = new short[outlen];
                //for (int i = 0; i < outlen; i++)
                //{
                    //outbuffer[i] = buffer[i * 3];
                    //outbuffer[i] <<= 8;
                    //outbuffer[i] += buffer[(i * 3) + 1];

                    //outbuffer[i] = (short)(buffer[3 * i] | buffer[3 * i + 1] << 8);
                //}
            }
            else if (bitPerSample == 32)
            {
                if ((outbuffer == null) || (outlen < len/4))
                {
                    outbuffer = new short[outlen];
                }
                outlen = len / 4;

                //according to NAudio developper WaveBuffer is faster
                //WaveBuffer waveBuffer = new WaveBuffer(len);
                //waveBuffer.BindTo(buffer);
                //for (int i = 0; i < outlen; i++)
                //{
                //    outbuffer[i] = (short)(waveBuffer.FloatBuffer[i] * short.MaxValue);
                //}

                //float[] bufferf = new float[outlen];
                //Buffer.BlockCopy(buffer, 0, bufferf, 0, len);
                //for (int i = 0; i < outlen; i++)
                //{
                //    float sample = bufferf[i];
                //    outbuffer[i] = (short)(sample * short.MaxValue);
                //}


                for (int i = 0; i < outlen; i++)
                {
                    float sample = BitConverter.ToSingle(buffer, i * 4);
                    outbuffer[i] = (short) ((sample<0)? (-sample * short.MinValue):(sample * short.MaxValue));
                }

            }

        }

        public static void ConvertByteArrayToFloatArray(byte[] buffer, int len, int bitPerSample, ref float[] outbuffer, ref int outlen)
        {
            if (bitPerSample == 8)
            {
                if ((outbuffer == null) || (outlen < len))
                {
                    outbuffer = new float[len];
                }
                outlen = len;
                for (int i = 0; i < outlen; i++)
                {
                    outbuffer[i] = (sbyte)buffer[i];
                    outbuffer[i] = (outbuffer[i] < 0f) ? outbuffer[i] / 128f : outbuffer[i] / 127f;
                }
            }
            else if (bitPerSample == 16)
            {
                if ((outbuffer == null) || (outlen < len /2))
                {
                    outbuffer = new float[len / 2];
                }
                outlen = len / 2;
                for (int i = 0; i < outlen; i++)
                {
                    outbuffer[i] = ((short) buffer[2 * i ] | (short)(buffer[(2 * i) + 1] << 8));
                    //outbuffer[i] /= 32767f;
                    outbuffer[i] = (outbuffer[i] < 0f) ? outbuffer[i] / 32768f : outbuffer[i] / 32767f;
                }

            }
            else if (bitPerSample == 24)
            {
                if ((outbuffer == null) || (outlen < len/3))
                {
                    outbuffer = new float[len / 3];
                }
                outlen = len / 3;
                for (int i = 0; i < outlen; i++)
                {
                    // TODO: add 24bit to float conversion
                    //outbuffer[i] = (short)(buffer[3 * i] | buffer[3 * i + 1] << 8);
                }
            }
            else if (bitPerSample == 32)
            {
                if ((outbuffer == null) || (outlen < len/4))
                {
                    outbuffer = new float[len / 4];
                }
                outlen = len / 4;
                // according to NAudio developper WaveBuffer is faster
                // followed by blockcopy and bitconverter
                //wavebuffer wavebuffer = new wavebuffer(len);
                //wavebuffer.bindto(buffer);
                //for (int i = 0; i < outlen; i++)
                //{
                //    outbuffer[i] = wavebuffer.floatbuffer[i];
                //}

                Buffer.BlockCopy(buffer, 0, outbuffer, 0, len);

                //for (int i = 0; i < outlen; i++)
                //{
                //    outbuffer[i] = BitConverter.ToSingle(buffer, i * 4);
                //}

            }
        }

        //public void ConvertByteAraryToFloat(float buffer, int len, ref byte [] outBuffer, ref int outLen)
        //{
        //    // todo implement len management use Array.Copy 
        //    outBuffer = BitConverter.GetBytes(buffer).ToArray() ;
        //    outLen = outBuffer.Length;
        //}

        public static void ConvertShortArrayToByteArray(short[] buffer, int len, ref byte[] outbuffer, ref int outlen)
        {
            if ((outbuffer == null) ||(outlen < len *2))
            {
                outbuffer = new byte[len * 2];
            }
            outlen = len * 2;
            for (int i = 0; i < len; i++)
            {
                outbuffer[i * 2] = (byte)(buffer[i] & 0xff);
                outbuffer[(i * 2) + 1] = (byte)(buffer[i] >> 8);
            }

            // Alternatives are Array.Copy, BitConverter or Buffer.BlockCoopy
            //Buffer.BlockCopy(buffer, 0, outbuffer, 0,  len);

        }

        public static void ConvertFloatArrayToByteArray(float[] buffer, int len, int bitPerSample, ref byte[] outbuffer, ref int outlen)
        {
            if (bitPerSample ==8)
            {

            }
            else if (bitPerSample == 16)
            {
                if ((outbuffer == null) || (outlen < len *2))
                {
                    outbuffer = new byte[len * 2];
                }
                outlen = len * 2;
                for (int i = 0; i < len; i++)
                {
                    float fsample = buffer[i];
                    //short sample = (short) (fsample * short.MaxValue); 
                    short sample = (short)((fsample < 0.0f) ? (fsample * -short.MinValue) : (fsample * short.MaxValue));
                    outbuffer[i * 2] = (byte)(sample & 0xff);
                    outbuffer[(i * 2) + 1] = (byte)(sample >> 8);
                }
            }
            else if (bitPerSample == 24)
            {
                //TODO: implement 24bit conversion
            }
            else if(bitPerSample == 32)
            {
                if ((outbuffer == null) || (outlen < len * 4))
                {
                    outbuffer = new byte[len * 4];
                }
                outlen = len * 4;
                Buffer.BlockCopy(buffer, 0, outbuffer, 0, len);

            }
        }

        // specialised method
        public static void ConvertMono16bitByteArrayToStereoShortArray(byte[] buffer, int bufferlen, ref byte[] outbuffer, ref int outbufferlen)
        {
            bufferlen -= (bufferlen % 2);
            if ((outbuffer == null) || (outbufferlen < bufferlen * 2))
            {
                outbuffer = new byte[bufferlen * 2];
            }
            outbufferlen = bufferlen * 2;
            for (int i = 0; i < bufferlen; i += 2)
            {
                outbuffer[(i * 2)] = buffer[i];
                outbuffer[(i * 2) + 1] = buffer[i + 1];
                outbuffer[(i * 2) + 2] = buffer[i];
                outbuffer[(i * 2) + 3] = buffer[i + 1];
            }
        }

        // generic method
        public static void MonoToStereo<T>(T[] buffer, int len, ref T[] outbuffer, ref int outlen)
        {
            if ((outbuffer == null) || (outlen < len * 2))
            {
                outbuffer = new T[len * 2];
            }
            outlen = len * 2;

            for (int i = 0; i < len; i++)
            {
                outbuffer[(i * 2)] = buffer[i];
                outbuffer[(i * 2) + 1] = buffer[i];
            }

        }
    }
}
