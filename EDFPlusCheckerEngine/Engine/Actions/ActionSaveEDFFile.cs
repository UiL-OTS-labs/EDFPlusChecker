using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDFPlusChecker.Engine
{
    public class ActionSaveEDFFile : BaseAction
    {
        #region Action Settings

        public string Prefix;
        public string SavePath;

        #endregion Action Settings

        // Starts acting on files
        public override string Act()
        {
            Active = true;
            if (!Directory.Exists(SavePath))
                Directory.CreateDirectory(SavePath);

            string NewFileName = SavePath + Path.DirectorySeparatorChar + Prefix + Path.GetFileName(Control.EDFPlusHandle.FileName);

            if(File.Exists(NewFileName))
                throw new ActionCannotDoWhatDoBeDo("Safety feature: cannot overwrite " + NewFileName + "! Check your configuration!");

            if(!Control.EDFPlusHandle.SaveToFile(NewFileName))
                throw new ActionCannotDoWhatDoBeDo("Could not save " + NewFileName);
            
            //Same 
            Active = false;

            return "Action: Saved changes in " + @Path.GetFileName(Control.EDFPlusHandle.FileName) + " as " + NewFileName;
        }

        #region Constructor

        public ActionSaveEDFFile(Controller cont, string path, string prefix)
            : base(cont)
        {
            this.SavePath = path;
            this.Prefix = prefix;
        }

        #endregion Constructor

        public override string GetDescription()
        {
            return "Save EDF Files with prefix: " + this.Prefix + ".";
        }
    }
}
