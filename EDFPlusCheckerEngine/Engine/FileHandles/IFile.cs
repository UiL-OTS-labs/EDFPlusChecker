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
            string Description =  "Onset: " + OnsetInSeconds + "s";
            Description += UncertaintyInSeconds <= 0.0 ? "" : " ~+ " + Math.Round( (UncertaintyInSeconds * 1000),1) + "ms";
            Description += " Trigger: " + TriggerNumber;
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
