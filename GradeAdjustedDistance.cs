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
using Gpx;
using ScottPlot;
using ScottPlot.Plottable;
using ScottPlot.Renderable;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GradeAdjustedDistance
{
    public partial class GradeAdjustedDistance : Form
    {
        public GradeAdjustedDistance(string[] args)
        {
            InitializeComponent();
            Options.LoadOptions();
            if(args.Length == 1)
            {
                using (FileStream file = File.OpenRead(args[0]))
                {
                    LoadFile(file);
                    SaveRecentFile(args[0]);
                    UpdateTitle(args[0]);
                }

            }
        }

        private void UpdateTitle(string filename)
        {
            this.Text = "Fellrnr Grade Adjusted Distance " + filename;
        }

        private void openGPXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                //openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "GPX|*.gpx|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                //openFileDialog.RestoreDirectory = true;
                openFileDialog.ReadOnlyChecked = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    LoadFile(openFileDialog.OpenFile());

                    UpdateTitle(openFileDialog.FileName);
                    SaveRecentFile(openFileDialog.FileName);
                }
            }

        }

        private void LoadFile(Stream file)
        {
            reverse = false;
            totalGradeAdjustedDistance = 0;
            totalDistance = 0;
            totalDistance3d = 0;
            totalAscent = 0;
            totalGradeAdjustedDistance = 0;
            costChart = new List<double>();
            rawSlopeChart = new List<double>();
            cumulativeDistance = new List<double>();
            distancesBetween = new List<double>();
            altitudeChart = new List<double>();
            elevationChanges = new List<double>();

            ProcessGPX(file);
        }

        private void ProcessGPX(Stream input)
        {
            using (GpxReader reader = new GpxReader(input))
            {
                while (reader.Read())
                {
                    switch (reader.ObjectType)
                    {
                        case GpxObjectType.Metadata:
                            
                            break;
                        case GpxObjectType.WayPoint:
                            
                            break;
                        case GpxObjectType.Route:
                            GpxRoute gpxRoute = reader.Route;
                            ProcessGpxRoute(gpxRoute);
                            break;
                        case GpxObjectType.Track:
                            GpxTrack gpxTrack = reader.Track;
                            LoadTrack(gpxTrack);

                            break;
                    }
                }
            }
        }

        private void LoadTrack(GpxTrack gpxTrack)
        {
            gpxPoints.Clear();

            foreach (GpxTrackSegment segment in gpxTrack.Segments)
            {
                GpxPointCollection<GpxPoint> segmentPoints = segment.TrackPoints.ToGpxPoints();

                foreach (GpxPoint point in segmentPoints)
                {
                    gpxPoints.Add(point);
                }
            }

            ProcessGpxPoints();
        }

        private void ProcessGpxRoute(GpxRoute gpxRoute)
        {
            gpxPoints.Clear();
            gpxPoints = gpxRoute.ToGpxPoints();

            ProcessGpxPoints();
        }
        private void ProcessGpxPoints()
        {
            //GpxPointCollection<GpxRoutePoint> gpxRoutePoints = gpxRoute.RoutePoints;


            GpxPoint? lastPoint = null;
            double totalDistanceTmp = 0;
            cumulativeDistance.Clear();
            altitudeChart.Clear();
            IEnumerable<GpxPoint> orderedPoints = reverse? gpxPoints.Reverse() : gpxPoints;
            foreach (GpxPoint gpxPoint in orderedPoints)
            {
                if (lastPoint != null && gpxPoint.Elevation != null && lastPoint.Elevation != null)
                {
                    double distanceBetween = gpxPoint.GetDistanceFrom(lastPoint) * 1000; //Km to meters
                    double elevationChange = (double)gpxPoint.Elevation - (double)lastPoint.Elevation;
                    totalDistanceTmp += distanceBetween;
                    cumulativeDistance.Add(totalDistanceTmp);
                    altitudeChart.Add((double)gpxPoint.Elevation);
                }
                lastPoint = gpxPoint;
            }

            if(altitudeChart.Count == 0)
            {
                MessageBox.Show("Sorry, this GPX file has no altitude data");
                return;
            }

            //if (Options.Instance.SmoothingType != Options.Smoothing.None)
            //{
            //    Tuple<List<double>, List<double>> interpolatedLists = Smoothing.Interpolate(cumulativeDistanceTmp, altitudeChartTmp);
            //    cumulativeDistance = interpolatedLists.Item1;
            //    altitudeChart = interpolatedLists.Item2;
            //}
            //else
            //{
            //    cumulativeDistance = cumulativeDistanceTmp;
            //    altitudeChart = altitudeChartTmp;
            //}
            CalculateElevationAndDistanceChanges();

            ProcessElevationChanges();
        }

        private void CalculateElevationAndDistanceChanges()
        {
            elevationChanges.Clear();
            distancesBetween.Clear();
            double lastAltitude = altitudeChart.First();
            double lastDistance = cumulativeDistance.First();
            for (int i = 0; i < cumulativeDistance.Count; i++)
            {
                double elevationChange = altitudeChart[i] - lastAltitude;
                lastAltitude = altitudeChart[i];
                elevationChanges.Add(elevationChange);

                double distanceChange = cumulativeDistance[i] - lastDistance;
                lastDistance = cumulativeDistance[i];
                distancesBetween.Add(distanceChange);
            }
        }

        double totalGradeAdjustedDistance = 0;
        double totalDistance = 0;
        double totalDistance3d = 0;
        double totalAscent = 0;
        double totalElevation = 0;
        GpxPointCollection<GpxPoint> gpxPoints = new GpxPointCollection<GpxPoint>();
        bool reverse = false;
        List<double> costChart = new List<double>();
        List<double> rawSlopeChart = new List<double>();
        List<double> smoothedSlopeChart = new List<double>();
        List<double> cumulativeDistance = new List<double>();
        List<double> distancesBetween = new List<double>();
        List<double> altitudeChart = new List<double>();
        List<double> elevationChanges = new List<double>();
        string results = "";
        private List<Axis> CurrentAxis { get; set; } = new List<Axis>();

        private void ProcessElevationChanges()
        {
            cumulativeDistance.Clear();
            costChart.Clear();
            rawSlopeChart.Clear();
            totalGradeAdjustedDistance = 0;
            totalDistance = 0;
            totalDistance3d = 0;
            totalAscent = 0;
            totalElevation = 0;

            //double[] smoothedElevationChanges;
            //if (Options.Instance.SmoothingType == Options.Smoothing.AverageWindow)
            //    smoothedElevationChanges = Smoothing.WindowSmoothed(rawElevationChanges.ToArray(), Options.Instance.SmoothingWindow);
            //else if (Options.Instance.SmoothingType == Options.Smoothing.SimpleExponential)
            //    smoothedElevationChanges = Smoothing.SimpleExponentialSmoothed(rawElevationChanges.ToArray(), Options.Instance.SmoothingWindow);
            //else
            //    smoothedElevationChanges = rawElevationChanges.ToArray();


            //work out the slope first
            for (int i = 0; i < distancesBetween.Count; i++)
            {
                double distanceBetween = distancesBetween[i];
                double elevationChange = elevationChanges[i];
                if (distanceBetween > 0)
                {

                    double slope = elevationChange / distanceBetween;

                    //constrain slope before smoothing. A discontinuity in the GPX will otherwise remain after smoothing
                    if (slope > Options.Instance.MaxSlope)
                        slope = Options.Instance.MaxSlope;

                    if (slope < Options.Instance.MinSlope)
                        slope = Options.Instance.MinSlope;


                    totalElevation += elevationChange;
                    if (elevationChange > 0) { totalAscent += elevationChange; }
                    rawSlopeChart.Add(slope);

                    double distance3d = Math.Sqrt(Math.Pow(distanceBetween, 2) + Math.Pow(elevationChange, 2));
                    totalDistance3d += distance3d;

                }
                else
                {
                    rawSlopeChart.Add(0);
                }
            }

            if (Options.Instance.SmoothingType == Options.Smoothing.AverageWindow)
                smoothedSlopeChart = Smoothing.WindowSmoothed(rawSlopeChart.ToArray(), Options.Instance.SmoothingWindow);
            else if (Options.Instance.SmoothingType == Options.Smoothing.SimpleExponential)
                smoothedSlopeChart = Smoothing.SimpleExponentialSmoothed(rawSlopeChart.ToArray(), Options.Instance.SmoothingWindow);
            else
                smoothedSlopeChart = rawSlopeChart;


            //then calculate cost from smoothed grade (which is the same as smoothing elevation weighted by distance)
            for (int i = 0; i < distancesBetween.Count; i++)
            {
                double distanceBetween = distancesBetween[i];
                double slope = smoothedSlopeChart[i];

                //=POWER(A2,2)*15.14+A2*2.896+1.0098
                //double cost = Math.Pow(slope, 2) * 15.14 + slope * 2.896 + 1.0098;
                double cost = Math.Pow(slope, 2) * Options.Instance.GradeAdjustmentX2 + slope * Options.Instance.GradeAdjustmentX + Options.Instance.GradeAdjustmentOffset;
                double gradeAdjustedDistance = distanceBetween * cost;
                totalGradeAdjustedDistance += gradeAdjustedDistance;
                totalDistance += distanceBetween;
                costChart.Add(cost);
                cumulativeDistance.Add(totalDistance);
            }

            UpdateGraphs();
        }
        public void UpdateGraphs()
        {
            formsPlot1.Plot.Clear();
            foreach (Axis axis in CurrentAxis) { formsPlot1.Plot.RemoveAxis(axis); }
            CurrentAxis.Clear();

            results = string.Format("Distance {0:N} Km, Grade Adjusted Distance {1:N} Km, 3D Distance {2:n} Km, total ascent {3:N}m, net elevation {4:N}m", totalDistance / 1000,           totalGradeAdjustedDistance / 1000, totalDistance3d / 1000, totalAscent, totalElevation);

            int elevationIndex = 0;
            formsPlot1.Plot.XAxis2.Label(results);
            var elevationGraph = formsPlot1.Plot.AddScatter(cumulativeDistance.ToArray(), altitudeChart.ToArray());
            formsPlot1.Plot.YAxis.Label("Elevation");
            formsPlot1.Plot.YAxis.Color(elevationGraph.Color);
            elevationGraph.YAxisIndex = 0;

            Axis yAxis;
            int arrayIndex = 1;
            if (Options.Instance.ShowCost)
            {
                var costGraph = formsPlot1.Plot.AddScatter(cumulativeDistance.ToArray(), costChart.ToArray());
                yAxis = formsPlot1.Plot.AddAxis(ScottPlot.Renderable.Edge.Left);
                if (yAxis != null)
                {
                    yAxis.AxisIndex = arrayIndex++;
                    yAxis.Color(costGraph.Color);
                    costGraph.YAxisIndex = yAxis.AxisIndex;
                    yAxis.Label("Cost");
                    CurrentAxis.Add(yAxis);
                }
            }

            if (Options.Instance.ShowSmoothedElevationChanges)
            {
                var elevationChangesGraph = formsPlot1.Plot.AddScatter(cumulativeDistance.ToArray(), elevationChanges.ToArray());
                yAxis = formsPlot1.Plot.AddAxis(ScottPlot.Renderable.Edge.Left);
                if (yAxis != null)
                {
                    yAxis.AxisIndex = arrayIndex++;
                    yAxis.Color(elevationChangesGraph.Color);
                    elevationChangesGraph.YAxisIndex = yAxis.AxisIndex;
                    yAxis.Label("Elevation Change");
                    CurrentAxis.Add(yAxis);
                }

            }

            if (Options.Instance.ShowSlope)
            {
                var slopeGraph = formsPlot1.Plot.AddScatter(cumulativeDistance.ToArray(), smoothedSlopeChart.ToArray());
                yAxis = formsPlot1.Plot.AddAxis(ScottPlot.Renderable.Edge.Left);
                if (yAxis != null)
                {
                    yAxis.AxisIndex = arrayIndex++;
                    yAxis.Color(slopeGraph.Color);
                    slopeGraph.YAxisIndex = yAxis.AxisIndex;
                    yAxis.Label("slope");
                    CurrentAxis.Add(yAxis);
                }
            }

            if (Options.Instance.ShowRawElevationChanges)
            {
                var rawElevationChangesGraph = formsPlot1.Plot.AddScatter(cumulativeDistance.ToArray(), elevationChanges.ToArray());
                yAxis = formsPlot1.Plot.AddAxis(ScottPlot.Renderable.Edge.Left);
                if (yAxis != null)
                {
                    yAxis.AxisIndex = arrayIndex++;
                    yAxis.Color(rawElevationChangesGraph.Color);
                    rawElevationChangesGraph.YAxisIndex = yAxis.AxisIndex;
                    yAxis.Label("Raw Elevation Changes");
                    CurrentAxis.Add(yAxis);
                }
            }

            if (Options.Instance.ShowColorMarkers && Options.Instance.ColorElevationMetric != Options.ColorElevation.None)
            {
                List<double> colorChart;

                if (Options.Instance.ColorElevationMetric == Options.ColorElevation.Cost)
                    colorChart = costChart;
                else
                    colorChart = smoothedSlopeChart;

                double minCost = colorChart.Min();
                double maxCost = colorChart.Max();
                for (int i = 0; i < cumulativeDistance.Count; i++)
                {
                    double colorValue = colorChart[i];
                    double percent = ((colorValue - minCost) / (maxCost - minCost)) * 100.0;
                    int red;
                    int green;
                    Color color;
                    if (percent < 50)
                    {
                        green = 255;
                        red = (int)Math.Round(percent * 5.10);
                        color = Color.FromArgb(255, (int)red, (int)green, 0);
                    }
                    else
                    {
                        red = 255;
                        double percentGreen = percent - 50.0;
                        double ratioGreen = percentGreen * 5.10;
                        double unroundedGreen = 255 - ratioGreen;
                        green = (int)Math.Round(unroundedGreen);
                        color = Color.FromArgb(255, (int)red, (int)green, 0);
                    }
                    //var green = percent < 50 ? 255 : Math.Round(256 - (percent - 50) * 5.12);
                    //var red = percent > 50 ? 255 : Math.Round((percent) * 5.12);
                    //Color color = Color.FromArgb(255, (int)red, (int)green, 0);
                    MarkerPlot markerPlot = formsPlot1.Plot.AddMarker(cumulativeDistance[i], altitudeChart[i], MarkerShape.filledCircle, 10, color);
                    markerPlot.YAxisIndex = elevationIndex;
                }
            }

            formsPlot1.Refresh();

        }


        public static double DistanceTo(double lat1, double lon1, double lat2, double lon2, char unit = 'K')
        {
            double rlat1 = Math.PI * lat1 / 180;
            double rlat2 = Math.PI * lat2 / 180;
            double theta = lon1 - lon2;
            double rtheta = Math.PI * theta / 180;
            double dist =
                Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
                Math.Cos(rlat2) * Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            switch (unit)
            {
                case 'K': //Kilometers -> default
                    return dist * 1.609344;
                case 'N': //Nautical Miles 
                    return dist * 0.8684;
                case 'M': //Miles
                    return dist;
            }

            return dist;
        }
        

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {

        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsForm1 optionsForm = new OptionsForm1();
            optionsForm.ShowDialog();
            if (cumulativeDistance.Count > 0)
                ProcessElevationChanges();
        }

        private void copyResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(results);
        }

        const int MRUnumber = 20;
        System.Collections.Generic.Queue<string> MRUlist = new Queue<string>();

        private void GradeAdjustedDistance_Load(object sender, EventArgs e)
        {
            LoadRecentList();
            foreach (string item in MRUlist)
            {
                ToolStripMenuItem fileRecent = new ToolStripMenuItem(item, null, RecentFile_click);  //create new menu for each item in list
                recentToolStripMenuItem.DropDownItems.Add(fileRecent); //add the menu to "recent" menu
            }
        }
        /// <summary>
        /// store a list to file and refresh list
        /// </summary>
        /// <param name="path"></param>
        private void SaveRecentFile(string path)
        {
            recentToolStripMenuItem.DropDownItems.Clear(); //clear all recent list from menu
            LoadRecentList(); //load list from file
            if (!(MRUlist.Contains(path))) //prevent duplication on recent list
                MRUlist.Enqueue(path); //insert given path into list
            while (MRUlist.Count > MRUnumber) //keep list number not exceeded given value
            {
                MRUlist.Dequeue();
            }
            foreach (string item in MRUlist)
            {
                ToolStripMenuItem fileRecent = new ToolStripMenuItem(item, null, RecentFile_click);  //create new menu for each item in list
                recentToolStripMenuItem.DropDownItems.Add(fileRecent); //add the menu to "recent" menu
            }
            //writing menu list to file
            StreamWriter stringToWrite = new StreamWriter(System.Environment.CurrentDirectory + "\\Recent.txt"); //create file called "Recent.txt" located on app folder
            foreach (string item in MRUlist)
            {
                stringToWrite.WriteLine(item); //write list to stream
            }
            stringToWrite.Flush(); //write stream to file
            stringToWrite.Close(); //close the stream and reclaim memory
        }
        /// <summary>
        /// load recent file list from file
        /// </summary>
        private void LoadRecentList()
        {//try to load file. If file isn't found, do nothing
            MRUlist.Clear();
            try
            {
                StreamReader listToRead = new StreamReader(System.Environment.CurrentDirectory + "\\Recent.txt"); //read file stream
                string? line;
                while ((line = listToRead.ReadLine()) != null) //read each line until end of file
                {
                    MRUlist.Enqueue(line); //insert to list
                }

                listToRead.Close(); //close the stream
            }
            catch (Exception)
            {

                //throw;
            }

        }
        /// <summary>
        /// click menu handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecentFile_click(object? sender, EventArgs e)
        {
            if (sender != null)
            {
                using (FileStream file = File.OpenRead(sender.ToString()!))
                {
                    LoadFile(file);
                }
            }
        }

        private void GradeAdjustedDistance_FormClosed(object sender, FormClosedEventArgs e)
        {
            Options.SaveOptions();
        }

        private void reverseRouteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            reverse = !reverse;
            ProcessGpxPoints();

        }

    }
}