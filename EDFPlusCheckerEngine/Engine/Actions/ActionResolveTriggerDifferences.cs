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

            DifferenceFile DifFile = Control.DifferenceBetweenFiles;
            Active = true;

            // Translate the replace list to additions to the AddList and RemoveList
            foreach (Trigger[] trig in DifFile.ReplaceList)
            {
                if(!DifFile.RemoveList.Contains(trig[0]))
                    DifFile.RemoveList.Add(trig[0]);
                
                bool TriggerInAddList = false;
                foreach (Trigger[] addTrigger in DifFile.AddList)
	            {
		            if(addTrigger[0].Equals(trig[1]))
                    {
                        TriggerInAddList = true;
                        break;
                    }
	            }

                if(!TriggerInAddList)
                {
                    Trigger[] temp = {trig[1], trig[1]};
                    DifFile.AddList.Add(temp);
                } 
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

            return "Action: Resolved differences in Triggers for " + @Path.GetFileName(Control.EDFPlusHandle.FileName) + "(only in memory, still need to save)\n\t" + (this.AddFlag ? " Added " : " Not Added ") + (this.RemoveFlag ? " Removed " : " Not Removed");
        }

        #region Constructor

        public ActionResolveTriggerDifferences(Controller cont, double errorMargin, bool removeFlag, bool addFlag)
            : base(cont)
        {
            this.ErrorMargin = errorMargin;
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
