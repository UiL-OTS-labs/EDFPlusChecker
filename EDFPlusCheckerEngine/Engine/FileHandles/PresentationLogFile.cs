using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;

namespace EDFPlusChecker.Engine
{
    class PresentationLogFile : IFile
    {
        private string _FileName;
        public string FileName { get { return _FileName; } }

        private Controller _owner;
        public Controller Owner { get { return _owner; } }

        private List<string> Headers;
        private List<string>[] Data;

        private Trigger[] _Triggers;

        public Trigger[] Triggers { 
            get {
                if(_Triggers == null)
                {
                    _Triggers = GetTriggers(Owner.TriggerNumbersToIgnore, Owner.TriggerNumberLowerLimit, Owner.TriggerNumberUpperLimit);
                }
                return _Triggers;
            } 
        }

        public string[] GetDataColumn(string header)
        {
            Predicate<string> HeaderFinder = (string str) => { return str == header; };
            int IndexOfColumn = Headers.FindIndex(HeaderFinder);
            return Data[IndexOfColumn].ToArray<string>();
        }

        private Trigger[] GetTriggers( int[] triggersToIgnore, int lowerTriggerLimit, int upperTriggerLimit)
        {
           string[] CodeColumn = GetDataColumn("Code");
           string[] TimingColumn = GetDataColumn("Time");
           string[] UncertaintyColumn = GetDataColumn("Uncertainty");
           
           if(CodeColumn.Length != TimingColumn.Length || TimingColumn.Length != UncertaintyColumn.Length)
                throw new ActionCannotDoWhatDoBeDo("Something fishy going on with the length of the columns in the Presentation file " + this.FileName);

           List<Trigger> Result = new List<Trigger>(CodeColumn.Length);

           for (int i = 0; i < CodeColumn.Length; i++)
           {
               int TriggerNumber = -1;
               if (int.TryParse(CodeColumn[i], out TriggerNumber) && TriggerNumber <= upperTriggerLimit && TriggerNumber >= lowerTriggerLimit && !triggersToIgnore.Contains(TriggerNumber))
               {
                   int OnsetTimeOneTenth = -9999;
                   int UncertaintyOneTenth = -9999;

                   if (!int.TryParse(TimingColumn[i], out OnsetTimeOneTenth) || !int.TryParse(UncertaintyColumn[i], out UncertaintyOneTenth))
                       throw new ActionCannotDoWhatDoBeDo("Something fishy going on with PresentationLog OnsetTime (TIME column)");

                   double OnsetTime = ((double) OnsetTimeOneTenth)/10000;
                   double Uncertainty = ((double) UncertaintyOneTenth)/10000;
                   Result.Add(new Trigger(OnsetTime, Uncertainty, TriggerNumber));
               }
           }
           return Result.ToArray<Trigger>();
        }

        public bool SaveToFile(string newFileName)
        {
            throw new NotImplementedException();
        }

        public bool CloseFile()
        {
            throw new NotImplementedException();
        }

        public PresentationLogFile(Controller owner, string fileName)
        {
            this._owner = owner;
            this._FileName = fileName;

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
