using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace EDFPlusChecker.Engine
{
    public class TimeFrameConverter
    {
        public Controller Control;

        private double[,] TimeStamps;

        //EEGtime_missing_marker = ( (Prestime_missing_marker - PresentationFirst) * (AnalyzerLast- AnalyzerFirst) / (PresentationLast - PresentationFirst) ) + AnalyzerFirst
        public bool TimeConvertTriggers(Trigger[] mapFrom, out Trigger[] mapToo)
        {
            if(TimeStamps == null)
                throw new ActionCannotDoWhatDoBeDo("No Time Conversion has been calculated yet while trying to Time Convert triggers!");

            Trigger[] Result = new Trigger[mapFrom.Length];

            //use the newly found timestamps to translate the Triggers
            for (int i = 0; i < Result.Length; i++)
            {
                Result[i] = new Trigger(this.MapTimePoint(mapFrom[i].OnsetInSeconds, TimeStamps), mapFrom[i].UncertaintyInSeconds, mapFrom[i].TriggerNumber);
            }

            mapToo = Result;

            return true;
        }

        public string ToString()
        {
            StringBuilder Description = new StringBuilder();
            Description.Append("Time Conversion Target:\t");
            Description.AppendLine(Math.Round(TimeStamps[0, 0], 3) + "s and " + Math.Round(TimeStamps[0, 1], 3) + "s");
            Description.Append("Time Conversion Source:\t");
            Description.AppendLine(Math.Round(TimeStamps[1, 0], 3) + "s and " + Math.Round(TimeStamps[1, 1], 3) + "s");
            return Description.ToString();
        }

        public bool FindTimeConversion(Trigger[] mapFrom, Trigger[] mapToo, int verificationWindowSize, double errorMargin)
        {
            double[,] TempTimeStamps = new double[2, 2];

            bool TimeConversionFound = false;

            //Initiate search starting from the tops.
            for (int i = 0; i < mapToo.Length; i++)
            {
                for (int j = 0; j < mapFrom.Length; j++)
                {
                    if (mapToo[i].TriggerNumber == mapFrom[j].TriggerNumber)
                    {
                        for (int a = mapToo.Length - 1; a > i && a >= 0; a--)
                        {
                            for (int b = mapFrom.Length - 1; b > j && b >= 0; b--)
                            {
                                if (mapToo[a].TriggerNumber == mapFrom[b].TriggerNumber)
                                {
                                    TempTimeStamps[0, 0] = mapFrom[j].ApproximateOnsetInSeconds;
                                    TempTimeStamps[0, 1] = mapFrom[b].ApproximateOnsetInSeconds;
                                    TempTimeStamps[1, 0] = mapToo[i].ApproximateOnsetInSeconds;
                                    TempTimeStamps[1, 1] = mapToo[a].ApproximateOnsetInSeconds;

                                    if (FindVerificationWindow(mapFrom, mapToo, verificationWindowSize, errorMargin, TempTimeStamps))
                                    {
                                        this.TimeStamps = TempTimeStamps;
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return TimeConversionFound;
        }

        private bool FindVerificationWindow(Trigger[] mapFrom, Trigger[] mapToo, int windowSize, double errorMargin, double[,] timeStamps)
        {
            Trigger[] WindowToo = new Trigger[windowSize];
            Trigger[] WindowFrom = new Trigger[windowSize];

            bool WindowFound = false;
            int StartIndex = mapFrom.Length - (windowSize + 1); // we start from the back because at the start there might be some synchrony problems whereas this at the end is less likely.
            while (!WindowFound && StartIndex >= 0)
            {
                Array.Copy(mapFrom, StartIndex--, WindowFrom, 0, windowSize); // Shift window one position upwards
                int WindowFromIndex = WindowFrom.Length - 1;
                int WindowTooIndex = WindowToo.Length - 1;

                for (int i = mapToo.Length - 1; i >= 0; i--) // Run through the MapToo list and see if we find integers in the same order
                {
                    if (mapToo[i].TriggerNumber == WindowFrom[WindowFromIndex].TriggerNumber) //If match
                    {
                        WindowToo[WindowTooIndex--] = mapToo[i]; //set
                        WindowFromIndex--;
                    }

                    if (WindowTooIndex < 0) // bottom of the list
                    {
                        WindowFound = true;
                        break;
                    }
                }

                if (WindowFound) // Check if the Windows are not only equal in TriggerNumbers but also have roughly the same time.
                {
                    for (int i = 0; i < WindowFrom.Length; i++)
                    {
                        double TooNewTime = MapTimePoint(WindowFrom[i].OnsetInSeconds, timeStamps);
                        if (
                             TooNewTime > (WindowToo[i].OnsetInSeconds + WindowFrom[i].UncertaintyInSeconds + errorMargin)
                              &&
                              TooNewTime < (WindowToo[i].OnsetInSeconds - errorMargin)
                            )
                        {
                            WindowFound = false; // ploted time is beyond the (J/Gl)itter galaxy!
                            break;
                        }
                    }
                }
            }

            return WindowFound;
        }

        private double MapTimePoint(double originFrom, double[,] p)
        {
            double TargetToo = p[1, 0] +
                (
                    (originFrom - p[0, 0])
                    *
                    (
                        (p[1, 1] - p[1, 0]) / (p[0, 1] - p[0, 0])
                    )
                );
            return Math.Round(TargetToo, 4);
        }

        public TimeFrameConverter(Controller control)
        {
            this.Control = control;
        }
    }
}
