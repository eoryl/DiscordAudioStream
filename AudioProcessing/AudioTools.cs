using System;
using System.Collections.Generic;
using System.Text;

namespace AudioProcessing
{
    public class AudioTools
    {

        /** Get dbFS value of a a float sample (min value -1.0 to max value 1.0)
        * @Returns dB full scale value
        */
        public static double linearTodBFS(double linear)
        {
            return (
                20.0 * 
                Math.Log10(
                    Math.Abs(linear)
                    )
                );

        }

        /** Get dbFS and return a linear value of a flat sample (min value -1.0 to max value 1.0)
        * @Returns lienar value
        */

        public static double dBFSToLinear(double dbFS)
        {
            return Math.Pow(10, (dbFS) / 20.0);
        }

        // single precision Math requires .NET 5.0 or .NET standard 2.1
        // TODO: change when moving to new framework
        public static float linearTodBFSf(float linear)
        {
            return (float) linearTodBFS(linear);
            //return (
            //    20.0f *
            //    MathF.Log10(
            //        MathF.Abs(linear)
            //        )
            //    );
        }

        public static float dBFSToLinearf(float dbFS)
        {
            return (float) Math.Pow(10, (dbFS) / 20.0f);
            //return MathF.Pow(10, (dbFS) / 20.0f);
        }


    }
}
