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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GradeAdjustedDistance
{
    public class Options
    {
        public Options() //Public to allow deserialization
        {
        }
        static Options() { }
        public static Options Instance { get; private set; } = new Options();

        private const string fileName = @"GradeAdjustedDistanceOptions.json";

        public static void LoadOptions()
        {
            if (File.Exists(fileName))
            {
                Console.WriteLine("Reading saved file");
                string jsonFromFile = File.ReadAllText(fileName);
                Options? options = JsonSerializer.Deserialize<Options>(jsonFromFile);
                if(options != null)
                {
                    Instance = options;
                }
            }

        }

        public static void SaveOptions()
        {
            string json = JsonSerializer.Serialize(Instance);
            File.WriteAllText(fileName, json);
        }

        //Default formula - //=POWER(A2,2)*15.14+A2*2.896+1.0098

        [Description("Multiplier for X squared where X is the slope")]
        public double GradeAdjustmentX2 { get; set; } = 15.14;

        [Description("Multiplier for X where X is the slope")]
        public double GradeAdjustmentX { get; set; } = 2.896;

        [Description("Offset for grade adjustment")]
        public double GradeAdjustmentOffset { get; set; } = 1.00; //was 1.0098 but that's silly as flat has to be 1.0

        [Description("Min slope (GPX can have noise that creates silly slopes)")]
        public double MinSlope { get; set; } = -0.5;

        [Description("Max slope (GPX can have noise that creates silly slopes)")]
        public double MaxSlope { get; set; } = 0.5;

        [Description("Show line for cost adjustment")]
        public bool ShowCost { get; set; } = false;

        public enum ColorElevation { Cost, Slope, None };

        [Description("Color the elevation line with another metric")]
        public ColorElevation ColorElevationMetric { get; set; } = ColorElevation.Slope;

        [Description("Show line for cost as markers (coloured)")]
        public bool ShowColorMarkers { get; set; } = false;

        [Description("Show line for raw elevation changes")]
        public bool ShowRawElevationChanges { get; set; } = false;

        [Description("Show line for smoothed elevation changes")]
        public bool ShowSmoothedElevationChanges { get; set; } = false;

        [Description("Show line for calculated slope")]
        public bool ShowSlope { get; set; } = false;

        public enum Smoothing { AverageWindow, SimpleExponential, InterpolateOnly, None };

        [Description("How to do the elevation smoothing")]
        public Smoothing SmoothingType { get; set; } = Smoothing.SimpleExponential;

        [Description("How big should the smoothing window be (in meters)")]
        public int SmoothingWindow { get; set; } = 50;
    }
}
