using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDFPlusChecker.Engine
{
    public struct Trigger
    {
        public double OnsetInSeconds;

        public double ApproximateOnsetInSeconds
        {
            get {
                return OnsetInSeconds + (UncertaintyInSeconds / 2);
            } 
        }
        public double UncertaintyInSeconds;

        public int TriggerNumber;
        public Trigger(double d, double u, int t)
        {
            OnsetInSeconds = d;
            UncertaintyInSeconds = u;
            TriggerNumber = t;
        }

        public bool SameTiming(Trigger reference, double errorMargin)
        {
            return (
                reference.OnsetInSeconds <= (this.OnsetInSeconds + this.UncertaintyInSeconds + errorMargin)
                &&
                reference.OnsetInSeconds >= (this.OnsetInSeconds - errorMargin)
                );
        }

        public override string ToString()
        {
            string Description = "Onset: " + new String(' ', 5 - (int)Math.Floor(Math.Log10((int)OnsetInSeconds) + 1)) + String.Format("{0:0.000}",OnsetInSeconds) + "s";
            Description += UncertaintyInSeconds <= 0.0 ? "" : " ~+ " + String.Format("{0:0.0}", (UncertaintyInSeconds * 1000)) + "ms";
            Description += " Trigger: " + new String(' ', 3 - (int)Math.Floor(Math.Log10(TriggerNumber) + 1)) + TriggerNumber;
            return Description;
        }
    }


    interface IFile
    {
        string FileName { get; }
        Trigger[] Triggers { get;}
        Controller Owner { get; }

        bool SaveToFile(string newFileName);
        bool CloseFile();
    }
}
