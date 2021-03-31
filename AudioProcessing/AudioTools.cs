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

    }
}
