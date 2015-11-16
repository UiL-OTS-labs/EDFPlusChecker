using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDFPlusChecker.Engine
{
    public class ActionOpenFiles : BaseAction
    {
        #region Action Settings

        private string EDFFileName { get; set; }

        private string LogFileName { get; set; }

        private bool PreReadTALS;

        #endregion Action Settings

        // Starts acting on files
       public override string Act()
        {
            if (Control.EDFFileStack.Count == 0)
                throw new ActionCannotDoWhatDoBeDo("Empty EDF File stack");
            Active = true;

            string EDFFileName = Control.EDFFileStack.Pop();

            string PresentationLogFilename = "";
            if (Control.PresentationLogFileStack.Count > 0)
                PresentationLogFilename = Control.PresentationLogFileStack.Pop();

            if (File.Exists(@EDFFileName))
            {
                Control.EDFPlusHandle = new EDFPlusFile(this.Control, @EDFFileName);
                if(PreReadTALS) // Sometimes we want to do fixes on the memorystream before we load the model into classes etc.
                    Control.EDFPlusHandle.ReadTALsToMemory();
            }
            else
                throw new ActionCannotDoWhatDoBeDo("Couldn't find or access file: " + @EDFFileName); 

            if (File.Exists(@PresentationLogFilename))
                Control.PresentationLogHandle = new PresentationLogFile(this.Control, @PresentationLogFilename);
            else
                throw new ActionCannotDoWhatDoBeDo("Couldn't find or access file: " + @PresentationLogFilename); 
            Active = false;
            
            string Description = "Action: Opened File(s): ";
            Description += @Path.GetFileName(EDFFileName);
            if (File.Exists(LogFileName))
            {
                Description += ", " + @Path.GetFileName(LogFileName);
            }
            return Description;
        }

        #region Constructor

        public ActionOpenFiles(Controller cont, bool preReadTALS = false)
            : base(cont)
        {
            this.PreReadTALS = preReadTALS;
        }

        #endregion Constructor

        public override string GetDescription()
        {
            return "Open Files and Load them in Memory";
        }
    }
}
