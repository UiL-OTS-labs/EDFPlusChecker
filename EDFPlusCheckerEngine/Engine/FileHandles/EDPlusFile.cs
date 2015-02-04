using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using NeuroLoopGainLibrary.Edf;

namespace EDFPlusChecker.Engine
{
    class EDFPlusFile : NeuroLoopGainLibrary.Edf.EdfPlusFile, IFile
    {
        #region public properties
        public long AnnotationSignalOffsetBytes 
        {
            get {

                long offset = SignalInfo[AnnotationSignalNrs[0]].BufferOffset;
                foreach (int SignalNr in AnnotationSignalNrs)
                {
                    if (offset >= SignalInfo[SignalNr].BufferOffset)
                        offset = SignalInfo[SignalNr].BufferOffset;
                }
                return offset;
            }
        }

        public bool AddTrigger(Trigger trigger)
        {
            int IndexOfAnnotation = this.TAL.Add(trigger.ApproximateOnsetInSeconds, trigger.TriggerNumber.ToString());
            if (IndexOfAnnotation >= 0)
                return true;
            else
                return false;
        }

        public bool RemoveTrigger(Trigger trigger, double errorMargin)
        {
            foreach (EdfPlusAnnotation Annotation in this.TAL)
            {

                if (Annotation.Annotation == trigger.TriggerNumber.ToString())
                {
                    if (
                        Annotation.Onset <= trigger.OnsetInSeconds + trigger.UncertaintyInSeconds + errorMargin
                        && Annotation.Onset >= trigger.OnsetInSeconds - errorMargin
                        )
                    {
                        return this.TAL.Remove(Annotation);
                    }
                }
            }
            return false;
        }

        public bool TALRead
        {
            get
            {
                return PrereadTALs != PrereadTALs.None;
            }

            private set{}
        }
        #endregion

        #region Constructor
        public EDFPlusFile(string aFileName, bool doOpenReadOnly = false, bool doUseMemoryStream = true, PrereadTALs aPrereadTALs = PrereadTALs.None, bool doOpen = true) 
            : base(@aFileName, doOpenReadOnly, doUseMemoryStream, aPrereadTALs, doOpen)
        {
        }
        #endregion Constructor

        public bool SaveToFile(string newFilename)
        {
            return SaveToFile(newFilename, false);
        }

        public bool SaveToFile(string newFileName, bool overwrite = false)
        {
            if (File.Exists(newFileName) && !overwrite)
            {
                DialogResult Feedback = MessageBox.Show("Error creating new file: File already exists", "Error", MessageBoxButtons.OK);
                return false;
            }

            // Write annotations to file
            if (!WriteModifiedTALBlocks())
                MessageBox.Show("Error writing annotations to buffer/file. Size?", "Error", MessageBoxButtons.OK);

            // if you are using the UseMemoryStream
            if (UseMemoryStream)
            {
                using (FileStream file = new FileStream(newFileName, FileMode.Create, System.IO.FileAccess.Write))
                {
                    MemoryHandle.WriteTo(file);
                }
            }

            Active = false;

            return true;
        }

        public void ReadTALsToMemory()
        {
            TALPrereader(null);
            DoHandlePrereadTALsFinished(this);
            PrereadTALs = PrereadTALs.OnOpenFile;
        }

        public void ReReadHeader()
        {
            ReadHeaderInfo();
            ReadSignalInfo();

            this.ValidFormat = (CheckHeader() && CheckSignals());

            CalculateDataBlockSize();
            DoDataBufferSizeChanged();
        }

        public Trigger[] GetTriggers()
        {
            if (!TALRead)
                ReadTALsToMemory();
            List<Trigger> Result = new List<Trigger>();

            foreach(EdfPlusAnnotation Annotation in this.TAL)
            {
                int trigger = -1;
                if (int.TryParse(Annotation.Annotation, out trigger))
                {
                    double OnsetTime = Annotation.Onset;
                    Result.Add(new Trigger(OnsetTime, 0.0, trigger));
                }
            }
           Result.Sort(
                delegate(Trigger t1, Trigger t2) {
                return t1.OnsetInSeconds.CompareTo(t2.OnsetInSeconds);
            });

            return Result.ToArray();
        }

        public bool CloseFile()
        {
            Active = false;
            return true;
        }
    }
}
