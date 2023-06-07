/*
 * Copyright 2023, Jonathan Savage, fellrnr.com
 * 
 * 
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation 
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, 
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
 * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GradeAdjustedDistance
{
    public class Smoothing
    {

        public static Tuple<List<double>, List<double>> Interpolate(List<double> listX, List<double> listY)
        {
            if(listX.Count < 2)
            {
                return new Tuple<List<double>, List<double>>(listX, listY);
            }

            List<double> resultX = new List<double>();
            List<double> resultY = new List<double>();


            resultX.Add(listX.First());
            resultY.Add(listY.First());

            for (int i=0; i < listX.Count-1; i++) //Note: count - 1
            {
                double x1 = listX[i];
                double x2 = listX[i+1];
                double y1 = listY[i];
                double y2 = listY[i+1];

                double xi = (x2 - x1);
                double yi = (y2 - y1);

                //number of parts is xi as int
                double divider = Math.Floor(xi);

                // divide difference into n parts
                xi = xi / divider;
                yi = yi / divider;

                // set new temp vars equal to first point
                double xf = x1;
                double yf = y1;


                // increment temp vars by difference-division parts
                for (int j = 0; j < divider; j++)
                {
                    xf += xi;
                    yf += yi;

                    resultX.Add(xf);
                    resultY.Add(yf);
                }
                resultX.Add(x2);
                resultY.Add(y2);

                /* for debug - I had a GPX that 120m drop in 45cm distance
                for(int k = 0; k < resultY.Count-1; k++)
                {
                    double diff = resultY[k + 1] - resultY[k];
                    diff = Math.Abs(diff);
                    if (diff > 10)
                        Console.WriteLine(diff);
                }
                */
            }


            Tuple<List<double>, List<double>> retval = new Tuple<List<double>, List<double>>(resultX, resultY);
            return retval;
        }

        public static List<double> WindowSmoothed(double[] rawData, int windowSize)
        {
            //double[] smoothed = new double[rawData.Length];
            List<double> smoothed = new List<double>();
            double[] buffer = new double[windowSize];


            int bufferIndex = 0;
            double sum = 0;
            for (int i = 0; i < rawData.Length; i++)
            {
                double nextInput = rawData[i];
                sum = sum - buffer[bufferIndex] + nextInput;

                // overwrite the old value with the new one
                buffer[bufferIndex] = nextInput;

                // increment the buffer index 
                bufferIndex = (bufferIndex + 1) % windowSize;

                // calculate the average
                smoothed.Add(((double)sum) / windowSize);
            }

            return smoothed;
        }

        public static List<double> SimpleExponentialSmoothed(double[] rawData, int windowSize)
        {
            List<double> smoothed = new List<double>();


            int smoothCounter = 0;
            double smoothedValue = 0;
            for (int i = 0; i < rawData.Length; i++)
            {
                double nextInput = rawData[i];
                if (smoothCounter < windowSize)
                {
                    smoothCounter++;
                }

                smoothedValue = (smoothedValue * (smoothCounter - 1.0) / smoothCounter) + nextInput / smoothCounter;
                // calculate the average
                smoothed.Add(smoothedValue);
            }

            return smoothed;
        }

    }
}
