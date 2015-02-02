
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeuroLoopGainLibrary.Edf;
using System.IO;

namespace EDFPlusChecker.Engine
{
    public class ActionCompareTriggers : BaseAction
    {
        #region Action Settings

        public double ErrorMargin;
        public int WindowSize;
        public int TriggerUpperLimit;
        public int TriggerLowerLimit;
        public bool RemoveRecTriggerPriorToPlace;

        #endregion Action Settings

        public override string Act()
        {
            Active = true;

            Control.ErrorMargin = this.ErrorMargin;

            Trigger[] PresentationLogTriggers = Control.PresentationLogHandle.GetTriggers(TriggerLowerLimit, TriggerUpperLimit);
            Trigger[] EDFPlusTriggers = Control.EDFPlusHandle.GetTriggers();

            double[,] TimeStamps; // First row holds Presentation time, second row holds the EDF
            Trigger[] Map = Control.PresentationLogHandle.TimeConvertTriggers(PresentationLogTriggers, EDFPlusTriggers, WindowSize, ErrorMargin, out TimeStamps);

            List<Trigger[]> AddList = new List<Trigger[]>(); // item to add, close match
            List<Trigger> RemoveList = new List<Trigger>(); // Item to remove
            List<Trigger[]> ReplaceList = new List<Trigger[]>(); // Item to replace, replacement

            for (int i = 0; i < Map.Length; i++)
            {
                bool Stop = false;
                for (int j = 0; !Stop && j < EDFPlusTriggers.Length; j++)
                {      
                    //Right on the trigger
                    if(Map[i].SameTiming(EDFPlusTriggers[j], ErrorMargin))
                    {
                        if (Map[i].TriggerNumber != EDFPlusTriggers[j].TriggerNumber)
                        {
                            Trigger[] temp = { EDFPlusTriggers[j], Map[i] };
                            ReplaceList.Add(temp);
                        }
                        Stop = true; //Assuming a single hit...
                        break;
                    }

                    //beyond the trigger.
                    if (EDFPlusTriggers[j].OnsetInSeconds > Map[i].OnsetInSeconds + Map[i].UncertaintyInSeconds + ErrorMargin)
                    {
                        Trigger CloseMatch;
                        if (
                                j == 0
                                || j == EDFPlusTriggers.Length - 1
                                || Math.Abs(EDFPlusTriggers[j].OnsetInSeconds - Map[i].OnsetInSeconds) < Math.Abs(EDFPlusTriggers[j - 1].OnsetInSeconds - Map[i].OnsetInSeconds)
                            )
                            CloseMatch = EDFPlusTriggers[j];
                        else
                            CloseMatch = EDFPlusTriggers[j - 1];

                        Trigger[] temp = { Map[i], CloseMatch };
                        AddList.Add(temp);
                        Stop = true;
                        break;
                    }
                }

                if (!Stop) //finally, we couldn't find anything are one of the last triggers where the beyond time cut off does not work:
                {
                    Trigger[] temp = { Map[i], EDFPlusTriggers[EDFPlusTriggers.Length - 1] };
                    AddList.Add(temp);
                } 
            }

            if(RemoveRecTriggerPriorToPlace) // just remove all occurances!
            {
                foreach (Trigger[] AddTrigger in AddList)
                {
                    foreach (Trigger RecTrigger in EDFPlusTriggers)
                    {
                        if(AddTrigger[0].TriggerNumber == RecTrigger.TriggerNumber)
                            RemoveList.Add(RecTrigger);
                    }
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
                        //OnSetDiff.Add(Math.Abs(EDFPlusTriggers[EdfIndex].OnsetInSeconds - Map[MapIndex + 1].OnsetInSeconds));
                        OnSetDiff.Add(EDFPlusTriggers[EdfIndex].ApproximateOnsetInSeconds - Map[MapIndex + 1].ApproximateOnsetInSeconds);
                        MapIndex += 2; // Increment once prior so that total increment becomes +2
                    }
            }

            //foreach (double Dif in OnSetDiff)
            //{
            //    Control.Log(Dif.ToString());   
            //}

            bool PrintInConsole = false;

            //Report the comparison!
            Control.Log("Statistics on: \t" + @Path.GetFileName(Control.EDFPlusHandle.FileName), PrintInConsole);
            Control.Log("Onset of triggers used for time conversion: ", PrintInConsole);
            Control.Log("EDF File Recording:\t" + Math.Round(TimeStamps[0, 0], 3) + "s and " + Math.Round(TimeStamps[0, 1], 3) + "s", PrintInConsole);
            Control.Log("Presentation Log:\t" + Math.Round(TimeStamps[1, 0], 3) + "s and " + Math.Round(TimeStamps[1, 1], 3) + "s", PrintInConsole);
            Control.Log("Average Trigger-time difference: " + Math.Round(OnSetDiff.Average(),5) + "s\t range: " + Math.Round(OnSetDiff.Min(),5) + "s to " + Math.Round(OnSetDiff.Max(),5) +  "s (Note: These are approximate statistics) ", PrintInConsole);
            
            Control.Log(Environment.NewLine + "Log. Triggers to be Added: ", PrintInConsole);
            if (AddList.Count == 0)
                Control.Log("None", PrintInConsole);
            for (int i = 0; i < AddList.Count; i++)
                Control.Log("Log. " + AddList[i][0].ToString() + "\t closest match: [Rec. " + AddList[i][1].ToString() + "]\tdiff: " + Math.Round(Math.Abs(AddList[i][1].ApproximateOnsetInSeconds - AddList[i][0].ApproximateOnsetInSeconds), 3) + "s", PrintInConsole);
            
            Control.Log(Environment.NewLine + "Rec. Triggers to be Removed: ", PrintInConsole);
            if (RemoveList.Count == 0)
                Control.Log("None", PrintInConsole);
            for (int i = 0; i < RemoveList.Count; i++)
                Control.Log("Rec. " + RemoveList[i].ToString());
            
            Control.Log(Environment.NewLine + "Rec. Triggers to be Replaced: ", PrintInConsole);
            if (ReplaceList.Count == 0)
                Control.Log("None", PrintInConsole);
            for (int i = 0; i < ReplaceList.Count; i++)
                Control.Log("Rec. " + ReplaceList[i][0].ToString() + " by Log. " + ReplaceList[i][1].ToString(), PrintInConsole);
            Control.Log(Environment.NewLine, PrintInConsole);

            //fill control variable with the differences
            Control.DifferenceBetweenFiles = new DifferenceFile(AddList, new List<Trigger[]>(0), ReplaceList);

            Active = false;
            return "Action: Compared triggers between " + @Path.GetFileName(Control.EDFPlusHandle.FileName) + " and " + @Path.GetFileName(Control.PresentationLogHandle.FileName) + " with an allowed time difference of: " + ErrorMargin + "s";
        }

        public override string GetDescription()
        {
            string Description = "Compare Triggers between EDF and Presentation Log and report in LOG.";
            Description += RemoveRecTriggerPriorToPlace ? "\n\t(Log. Triggers that are set to be added will prompt all Rec. Triggers with the same trigger number to be removed prior to be added.)" : "";
            return Description;
        }

        public ActionCompareTriggers(Controller cont, int windowSize, double errorMargin, int triggerLowerLimit, int triggerUpperLimit, bool removeRecTriggerPriorToPlace)
            : base(cont)
        {
            this.RemoveRecTriggerPriorToPlace = removeRecTriggerPriorToPlace;
            this.WindowSize = windowSize;
            this.ErrorMargin = errorMargin;
            this.TriggerUpperLimit = triggerUpperLimit;
            this.TriggerLowerLimit = triggerLowerLimit;
        }
    }
}
