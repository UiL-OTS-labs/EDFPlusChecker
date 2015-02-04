using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeuroLoopGainLibrary;

namespace EDFPlusChecker.Engine
{
    public class ActionResolveTriggerDifferences : BaseAction
    {
        #region Action Settings

        double ErrorMargin;

        bool ReplaceFlag;
        bool AddFlag;
        bool RemoveFlag;

        #endregion Action Settings

        // Starts acting on files
        public override string Act()
        {
            if (Control.DifferenceBetweenFiles == null)
                throw new ActionCannotDoWhatDoBeDo("There was no Difference between the files generated!");
            if(Control.DifferenceBetweenFiles.IsEmpty())
                return "Action: No Trigger differences existed that are in need of resolution!";

            Active = true;

            if(ReplaceFlag)
                foreach (Trigger[] trig in Control.DifferenceBetweenFiles.ReplaceList)
                {
                    if (!Control.EDFPlusHandle.RemoveTrigger(trig[0], ErrorMargin) || !Control.EDFPlusHandle.AddTrigger(trig[1]))
                        throw new ActionCannotDoWhatDoBeDo("Could not resolve to replace Rec. trigger. " + trig[0].ToString() + " with Log. " + trig[1].ToString());
                }

            if(RemoveFlag)
                foreach (Trigger trig in Control.DifferenceBetweenFiles.RemoveList)
                {
                    if (!Control.EDFPlusHandle.RemoveTrigger(trig, ErrorMargin))
                        throw new ActionCannotDoWhatDoBeDo("Could not resolve to remove Rec. trigger. " + trig.ToString());
                }

            if(AddFlag)
                foreach (Trigger[] trig in Control.DifferenceBetweenFiles.AddList)
                {
                    if (!Control.EDFPlusHandle.AddTrigger(trig[0]))
                        throw new ActionCannotDoWhatDoBeDo("Could not resolve to add trigger. " + trig[0].ToString());
                }


            

            Active = false;

            return "Action: Resolved differences in Triggers for " + @Path.GetFileName(Control.EDFPlusHandle.FileName) + "(only in memory, still need to save)\n\t" + (this.AddFlag ? " Added " : " Not Added ") + (this.RemoveFlag ? " Removed " : " Not Removed") + (this.ReplaceFlag ? " Replaced " : "Not Replaced");
        }

        #region Constructor

        public ActionResolveTriggerDifferences(Controller cont, double errorMargin, bool replaceFlag, bool removeFlag, bool addFlag)
            : base(cont)
        {
            this.ErrorMargin = errorMargin;
            this.ReplaceFlag = replaceFlag;
            this.RemoveFlag = removeFlag;
            this.AddFlag = addFlag;
        }

        #endregion Constructor

        public override string GetDescription()
        {
            return "Resolve differences in memory";
        }
    }
}
