﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using System.Windows.Forms;
using EDFPlusChecker.Engine;
using System.ComponentModel;

namespace EDFPlusChecker
{
    public class Controller
    {
        public bool HasLogFiles
        {
            get
            {
                return PresentationLogFileStack == null || PresentationLogFileStack.Count != 0;
            }
        }

        public double ErrorMargin;

        internal EDFPlusFile EDFPlusHandle { get; set; }
        internal PresentationLogFile PresentationLogHandle { get; set; }
    
        internal Stack<string> EDFFileStack;
        internal Stack<string> PresentationLogFileStack;
        internal DifferenceFile DifferenceBetweenFiles;

        internal TimeFrameConverter TimeConversion;
        
        public int[] TriggerNumbersToIgnore = new int[0];
        public int TriggerNumberLowerLimit = 0;
        public int TriggerNumberUpperLimit = 9999;
        public bool CorrectForPauses;

        private StreamWriter ApplicationLogFile;

        internal BaseAction CurrentAction;

        private LinkedList<BaseAction> ActionChain;

        public string ApplicationLogFileName = "EDFPlus_checker_log.log";

        #region Action Control

        public int AddAction(BaseAction action)
        {
            ActionChain.AddLast(action);
            return 0;
        }

        public string[] GetActionOverview()
        {
            List<string> result = new List<string>();

            foreach (BaseAction Action in ActionChain)
            {
                result.Add(Action.GetDescription());
            }
            return result.ToArray();
        }

        public BaseAction RemovePreviousAction()
        {
            BaseAction Last = ActionChain.Last();
            ActionChain.RemoveLast();
            return Last;
        }

        public void ClearActionChain()
        {
            this.ActionChain.Clear();
        }

        #endregion Action Control

        #region Structs def
      
        #endregion Structs def

        private void FlushPrevious()
        {
            TimeConversion = new TimeFrameConverter(this);
            DifferenceBetweenFiles = null;
            CurrentAction = null;
            EDFPlusHandle = null;
            PresentationLogHandle = null;
        }

        public bool SetFileLists(string[] EDFFiles, string[] LogFiles)
        {
            this.EDFFileStack = new Stack<string>();
            this.PresentationLogFileStack = new Stack<string>();

            PresentationLogFileStack.Clear();
            EDFFileStack.Clear();

            if (EDFFiles.Length == 0)
                throw new ActionNotWellConfiguredException("EDF-file list is empty!");

            foreach (string item in EDFFiles)
            {
                if (!File.Exists(item))
                    throw new ActionNotWellConfiguredException("EDF-file list contains items that do not exist!");
                this.EDFFileStack.Push(item);
            }
            if(LogFiles.Length != 0)
            {
                if (LogFiles.Length != EDFFiles.Length)
                {
                    throw new ActionNotWellConfiguredException("EDF-file and log-file lists seem to be of different length!");
                }
                else
                {
                    foreach (string item in LogFiles)
                    {
                        if (!File.Exists(item))
                            throw new ActionNotWellConfiguredException("Log-file list contains items that do not exist!");
                        this.PresentationLogFileStack.Push(item);
                    }
                }
            }

            return true;
        }

        private void UpdateProgress(BackgroundWorker worker, double percentage, string message = "")
        {
            worker.ReportProgress((int)(percentage * 100), message);
        }

        public bool StartExecution()
        {
            return StartExecution(new BackgroundWorker());
        }

        public bool StartExecution(BackgroundWorker sendingWorker)
        {
            if (this.EDFFileStack == null || this.PresentationLogFileStack == null)
                throw new ActionCannotDoWhatDoBeDo("No files were presented. Initalise file stack prior to executing!");
            string applicationLogFileName = this.ApplicationLogFileName;

            int counter = 1;
            string Holder = applicationLogFileName;
            while (File.Exists(Holder))
            {
                Holder = Path.GetDirectoryName(applicationLogFileName) + Path.GetFileNameWithoutExtension(applicationLogFileName) + "(" + counter + ")" + Path.GetExtension(applicationLogFileName);
                counter++;
                if (counter > 100)
                    throw new FileNotFoundException("Could not generate an Application Log file after " + counter + " attemtps!");
            }

            this.ApplicationLogFileName = Holder;
            ApplicationLogFile = new StreamWriter(this.ApplicationLogFileName);
            Log("Keeping detailed log in: " + applicationLogFileName, true);

            int TotalNumberOfFiles = EDFFileStack.Count;

            while(EDFFileStack.Count > 0)
            {
                this.FlushPrevious(); // make absolutely sure nothing of the previous files will be used.

                try
                {
                    foreach (BaseAction Action in ActionChain)
                    {
                        if (sendingWorker.CancellationPending)
                        {
                            return false;
                        }
                        CurrentAction = Action;
                        string Description = CurrentAction.Act();
                        UpdateProgress(sendingWorker, -1, Description);
                        Log(Description, true);
                    }
                }
                catch (ActionCannotDoWhatDoBeDo e)
                {
                    Log("ERROR: " + e.Message, true);
                    DialogResult Choice = MessageBox.Show(e.Message + "\n\n Do you want to continue with the next file? ", "Critical Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                    if (Choice == DialogResult.No)
                        break;
                    UpdateProgress(sendingWorker, (1.0 - (double)EDFFileStack.Count / TotalNumberOfFiles));
                }
                UpdateProgress(sendingWorker, (1.0 - (double)EDFFileStack.Count / TotalNumberOfFiles));
                
                Log(new String('=', 50), true);
            }

            UpdateProgress(sendingWorker, -1, "FINISHED!");
            ApplicationLogFile.Close();
            return true;
        }

        public void Log(String lines, bool PrintConsole = false)
        {
            // Write the string to a file.append mode is enabled so that the log
            // lines get appended to  test.txt than wiping content and writing the log
            ApplicationLogFile.WriteLine(lines);
            if(PrintConsole)
                Console.WriteLine(lines);
        }

        #region Constructor
        public Controller()
        {
            ActionChain = new LinkedList<BaseAction>();
            TimeConversion = new TimeFrameConverter(this);
        }
        #endregion Constructor
    }
}
