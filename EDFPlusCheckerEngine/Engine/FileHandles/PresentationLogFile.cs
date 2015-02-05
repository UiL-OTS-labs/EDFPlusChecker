using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

namespace EDFPlusChecker.Engine
{
    class PresentationLogFile : IFile
    {
        public string FileName;

        private List<string> Headers;
        private List<string>[] Data;

        bool IFile.SaveToFile(string newFileName)
        {
            throw new NotImplementedException();
        }
        
        public string[] GetDataColumn(string header)
        {
            Predicate<string> HeaderFinder = (string str) => { return str == header; };
            int IndexOfColumn = Headers.FindIndex(HeaderFinder);
            return Data[IndexOfColumn].ToArray<string>();
        }

        public Trigger[] GetTriggers()
        {
            return GetTriggers(0, 256);
        }

        public Trigger[] GetTriggers(int lowerTriggerLimit = 0, int upperTriggerLimit = 255)
        {
           string[] CodeColumn = GetDataColumn("Code");
           string[] TimingColumn = GetDataColumn("Time");
           string[] UncertaintyColumn = GetDataColumn("Uncertainty");
           
           if(CodeColumn.Length != TimingColumn.Length || TimingColumn.Length != UncertaintyColumn.Length)
                throw new ActionCannotDoWhatDoBeDo("Something fishy going on with the length of the columns in the Presentation file " + this.FileName);

           List<Trigger> Result = new List<Trigger>(CodeColumn.Length);

           for (int i = 0; i < CodeColumn.Length; i++)
           {
               int Trigger = -1;
               if (int.TryParse(CodeColumn[i], out Trigger) && Trigger <= upperTriggerLimit && Trigger >= lowerTriggerLimit)
               {
                   int OnsetTimeOneTenth = -9999;
                   int UncertaintyOneTenth = -9999;

                   if (!int.TryParse(TimingColumn[i], out OnsetTimeOneTenth) || !int.TryParse(UncertaintyColumn[i], out UncertaintyOneTenth))
                       throw new ActionCannotDoWhatDoBeDo("Something fishy going on with PresentationLog OnsetTime (TIME column)");

                   double OnsetTime = ((double) OnsetTimeOneTenth)/10000;
                   double Uncertainty = ((double) UncertaintyOneTenth)/10000;
                   Result.Add(new Trigger(OnsetTime, Uncertainty, Trigger));
               }
           }
           return Result.ToArray<Trigger>();
        }


        //EEGtime_missing_marker = ( (Prestime_missing_marker - PresentationFirst) * (AnalyzerLast- AnalyzerFirst) / (PresentationLast - PresentationFirst) ) + AnalyzerFirst
        public Trigger[] TimeConvertTriggers(Trigger[] mapFrom, Trigger[] mapToo,  int windowSize, double errorMargin, out double[,] TimeStamps)
        {
            Trigger[] Result = new Trigger[mapFrom.Length];
            
            if(!FindTimeConversion(mapFrom, mapToo, windowSize, errorMargin, out TimeStamps))
                throw new ActionCannotDoWhatDoBeDo("Could not generate a time conversion from " + this.FileName);
           
            //use the newly found timestamps to translate the Triggers
            for (int i = 0; i < Result.Length; i++)
            {
                Result[i] = new Trigger(this.MapTimePoint(mapFrom[i].OnsetInSeconds, TimeStamps), mapFrom[i].UncertaintyInSeconds, mapFrom[i].TriggerNumber);
            }

            return Result;
        }

        private bool FindTimeConversion(Trigger[] mapFrom, Trigger[] mapToo, int verificationWindowSize, double errorMargin, out double[,] timeStamps)
        {
            timeStamps = new double[2, 2];

            bool TimeConversionFound = false;

            //Initiate search starting from the tops.
            for (int i = 0; i < mapToo.Length; i++)
			{
			    for (int j = 0; j < mapFrom.Length; j++)
			    {
			        if(mapToo[i].TriggerNumber == mapFrom[j].TriggerNumber)
                    {
                        for (int a = mapToo.Length - 1; a > i && a >= 0; a--)
                        {
                            for (int b = mapFrom.Length - 1; b > j && b >= 0 ; b--)
                            {
                                if(mapToo[a].TriggerNumber == mapFrom[b].TriggerNumber)
                                {
                                    timeStamps[0, 0] = mapFrom[j].ApproximateOnsetInSeconds;
                                    timeStamps[0, 1] = mapFrom[b].ApproximateOnsetInSeconds;
                                    timeStamps[1, 0] = mapToo[i].ApproximateOnsetInSeconds;
                                    timeStamps[1, 1] = mapToo[a].ApproximateOnsetInSeconds;
                                    
                                    if (FindVerificationWindow(mapFrom, mapToo, verificationWindowSize, errorMargin, timeStamps))
                                        return true;
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

                for (int i = mapToo.Length-1; i >= 0; i--) // Run through the MapToo list and see if we find integers in the same order
                {
                    if (mapToo[i].TriggerNumber == WindowFrom[WindowFromIndex].TriggerNumber) //If match
                    {
                        WindowToo[WindowTooIndex--] = mapToo[i]; //set
                        WindowFromIndex--;
                    }

                    if(WindowTooIndex < 0) // bottom of the list
                    {
                        WindowFound = true;
                        break;
                    }
                }
                
                if(WindowFound) // Check if the Windows are not only equal in TriggerNumbers but also have roughly the same time.
                {
                    for (int i = 0; i < WindowFrom.Length; i++)
                    {
                        double TooNewTime = MapTimePoint(WindowFrom[i].OnsetInSeconds, timeStamps);
                        if (
                             TooNewTime > (WindowToo[i].OnsetInSeconds + errorMargin)
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
            double TargetToo =  p[1,0] +
                (
                    (originFrom - p[0,0]) 
                    * 
                    (
                        (p[1,1] - p[1,0]) / (p[0,1] - p[0,0])
                    ) 
                );
            return Math.Round(TargetToo, 4);
        }

        bool IFile.CloseFile()
        {
            throw new NotImplementedException();
        }

        public PresentationLogFile(string fileName)
        {
            this.FileName = fileName;
            StreamReader reader = new StreamReader(File.OpenRead(fileName));
            List<Trigger> TriggerList = new List<Trigger>();

            using (TextFieldParser parser = new TextFieldParser(reader))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters("\t");
                string[] IgnoreTokens = {"Scenario", "Logfile"};
                parser.CommentTokens = IgnoreTokens;

                // setup the headers and DataList
                bool HeadersFound = false;
                while(!HeadersFound)
                {
                    if (parser.EndOfData)
                        throw new ActionCannotDoWhatDoBeDo("Could not find header line ('Subject') in Presentation log: " + fileName);
                    string[] HeaderStrings = parser.ReadFields();
                    if(HeaderStrings[0] == "Subject")
                    {
                        this.Headers = HeaderStrings.ToList<string>();
                        HeadersFound = true;
                    }
                }
                
                List<string>[] DataList = new List<string>[Headers.Count];
                for (int i = 0; i < DataList.Length; i++)
                    DataList[i] = new List<string>();

                int col = 0;
                while (!parser.EndOfData)
                {
                    //Processing each row
                    string[] fields = parser.ReadFields();
                    foreach (string field in fields)
                    {
                        DataList[col].Add(field);
                        col++;
                    }

                    for (; col < Headers.Count; col++)
                    {
                        DataList[col].Add("0");
                    }

                    col = 0;
                }
                Data = DataList;
            }
        }

    }
}
