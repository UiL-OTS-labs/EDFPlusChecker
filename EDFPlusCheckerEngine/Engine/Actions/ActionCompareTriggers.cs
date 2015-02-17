
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeuroLoopGainLibrary.Edf;
using System.IO;
using System.Diagnostics;
namespace EDFPlusChecker.Engine
{
    public class ActionCompareTriggers : BaseAction
    {
        #region Action Settings

        public double ErrorMargin;

        public bool UneqNumberEqTiming_RemoveRec;
        public bool EqNumberUneqTiming_RemoveRec;
        public bool UneqNumberUneqTiming_AddLog;

        #endregion Action Settings

        public override string Act()
        {
            Active = true;

            this.ErrorMargin = Control.ErrorMargin;

            Trigger[] PresentationLogTriggers = Control.PresentationLogHandle.Triggers;
            Trigger[] EDFPlusTriggers = Control.EDFPlusHandle.Triggers;

            Trigger[] Map;
            if(!Control.TimeConversion.TimeConvertTriggers(PresentationLogTriggers, out Map))
                throw new ActionCannotDoWhatDoBeDo("Could not generate time conversion.");

            List<Trigger[]> AddList = new List<Trigger[]>(); // item to add, close match
            List<Trigger> RemoveList = new List<Trigger>(); // Item to remove

            for (int i = 0; i < Map.Length; i++)
            {
                bool Found = false;
                for (int j = 0; j < EDFPlusTriggers.Length; j++)
                {

                    if (Map[i].TriggerNumber == EDFPlusTriggers[j].TriggerNumber) // Same triggernumber
                    {
                        // same triggernumber, equal timing.
                        if (Map[i].SameTiming(EDFPlusTriggers[j], ErrorMargin))
                        {
                            // Well, we'll do nothing thank you.
                            Found = true;
                        }
                        // same triggernumber, unequal timing.
                        else
                        {
                            if(this.EqNumberUneqTiming_RemoveRec)
                                RemoveList.Add(EDFPlusTriggers[j]);
                        }
                    }
                    else //different triggernumber
                    {
                        // different triggernumber, equal timing.
                        if (Map[i].SameTiming(EDFPlusTriggers[j], ErrorMargin))
                        {   
                            if (this.UneqNumberEqTiming_RemoveRec)
                                RemoveList.Add(EDFPlusTriggers[j]);
                        }
                        // different triggernumber, different timing.
                        else
                        {
                            // Well, we'll do nothing thank you.
                        }
                    }
                }

                if (!Found) //it's nowhere to be found, oh no!
                {
                    if (this.UneqNumberUneqTiming_AddLog)
                        AddList.Add(new Trigger[] { Map[i], ClosestTrigger(Map[i].OnsetInSeconds, EDFPlusTriggers) });
                } 
            }

            List<double> OnSetDiff = new List<double>(PresentationLogTriggers.Length);

            // Get some (ROUGH!) statistics on the match/missmatch
            int MapIndex = 0;
            for (int EdfIndex = 0; EdfIndex < EDFPlusTriggers.Length && MapIndex < PresentationLogTriggers.Length; EdfIndex++)
            {
                if (EDFPlusTriggers[EdfIndex].TriggerNumber == Map[MapIndex].TriggerNumber) //simple hit
                {
                    //OnSetDiff.Add( Math.Abs(EDFPlusTriggers[EdfIndex].OnsetInSeconds - Map[MapIndex].OnsetInSeconds));
                    OnSetDiff.Add(EDFPlusTriggers[EdfIndex].ApproximateOnsetInSeconds - Map[MapIndex].ApproximateOnsetInSeconds);
                    MapIndex++;
                }
                else
                    if (MapIndex != PresentationLogTriggers.Length - 1 && EDFPlusTriggers[EdfIndex].TriggerNumber == Map[MapIndex + 1].TriggerNumber) //likely a missing Trigger
                    {
                        OnSetDiff.Add(EDFPlusTriggers[EdfIndex].ApproximateOnsetInSeconds - Map[MapIndex + 1].ApproximateOnsetInSeconds);
                        MapIndex += 2; // Increment once prior so that total increment becomes +2
                    }
            }

            bool PrintInConsole = true;

            //Report the comparison!
            Control.Log("Statistics on: \t" + @Path.GetFileName(Control.EDFPlusHandle.FileName), PrintInConsole);
            Control.Log("Onset of triggers used for time conversion: ", PrintInConsole);
            Control.Log(Control.TimeConversion.ToString(), PrintInConsole);
            Control.Log("Average Trigger-time difference: " + Math.Round(OnSetDiff.Average(), 5) + "s\t range: " + Math.Round(OnSetDiff.Min(), 5) + "s to " + Math.Round(OnSetDiff.Max(), 5) + "s (Note: These are approximate statistics) ", PrintInConsole);
            Control.Log("Used an error margin of: " + this.ErrorMargin + "s");

            Control.Log(Environment.NewLine + "Log. Triggers to be Added: ", PrintInConsole);
            if (AddList.Count == 0)
                Control.Log("None", PrintInConsole);
            for (int i = 0; i < AddList.Count; i++)
                Control.Log(i+1 + ": Log. " + AddList[i][0].ToString() + Environment.NewLine + "\t closest trigger in recording: [Rec. " + AddList[i][1].ToString() + "]\tdiff: " + String.Format("{0:0.000}", Math.Round(Math.Abs(AddList[i][1].ApproximateOnsetInSeconds - AddList[i][0].ApproximateOnsetInSeconds), 3)) + "s", PrintInConsole);

            Control.Log(Environment.NewLine + "Rec. Triggers to be Removed: ", PrintInConsole);
            if (RemoveList.Count == 0)
                Control.Log("None", PrintInConsole);
            for (int i = 0; i < RemoveList.Count; i++)
                Control.Log(i+1 + ": Rec. " + RemoveList[i].ToString());

            Control.Log(Environment.NewLine, PrintInConsole);

            //fill control variable with the differences
            Control.DifferenceBetweenFiles = new DifferenceFile(AddList, RemoveList);         
                     
            Active = false;
            StringBuilder strb = new StringBuilder();
            strb.Append("Action: Compared triggers between ");
            strb.Append(@Path.GetFileName(Control.EDFPlusHandle.FileName) + " and " + @Path.GetFileName(Control.PresentationLogHandle.FileName));
            strb.AppendLine(" with an allowed time difference of: " + ErrorMargin + "s");
            strb.AppendLine("Ignored ALL triggers with integers smaller than " + Control.TriggerNumberLowerLimit + " and larger than " + Control.TriggerNumberUpperLimit);
            string IgnoreTriggers = String.Join(";", Control.TriggerNumbersToIgnore);
            strb.AppendLine("Ignored speficic integers: " + ( IgnoreTriggers.Length > 0 ? IgnoreTriggers : "None") );

            return strb.ToString();
        }

        public Trigger ClosestTrigger(double onset, Trigger[] triggers)
        {
            if(triggers.Length == 1)
                return triggers[0];

            Trigger CloseMatch = triggers[0];
            double SmallestDifference = Math.Abs(CloseMatch.OnsetInSeconds-onset);

            foreach(Trigger trigger in triggers)
            {
                if(Math.Abs(trigger.OnsetInSeconds - onset) <= SmallestDifference)
                {
                    CloseMatch = trigger;
                    SmallestDifference = Math.Abs(trigger.OnsetInSeconds - onset);
                }
            }

            return CloseMatch;
        }

        public override string GetDescription()
        {
            string Description = "Compare triggers between logged file and recorded file.";
            Description += this.EqNumberUneqTiming_RemoveRec ? Environment.NewLine + "\tRemove a recorded trigger if equal numbers but unqual timing." : "";
            Description += this.UneqNumberEqTiming_RemoveRec ? Environment.NewLine + "\tRemove a recorded trigger if unequal number but equal timing." : "";
            Description += this.UneqNumberUneqTiming_AddLog ? Environment.NewLine +  "\tAdd a logged trigger is an exact match was never found." : "";
            return Description;
        }

        public ActionCompareTriggers(Controller cont, bool uneqNumberEqTiming_RemoveRec, bool eqNumberUneqTiming_RemoveRec, bool uneqNumberUneqTiming_AddLog)
            : base(cont)
        {
            this.UneqNumberEqTiming_RemoveRec = uneqNumberEqTiming_RemoveRec;

            this.EqNumberUneqTiming_RemoveRec = eqNumberUneqTiming_RemoveRec;
            
            this.UneqNumberUneqTiming_AddLog = uneqNumberUneqTiming_AddLog; // only on the end.
        }
    }
}
