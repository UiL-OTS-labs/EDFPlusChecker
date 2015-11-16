using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace EDFPlusChecker.Engine
{
    public class ActionGenerateTimeConversion : BaseAction
    {
        public int VerificationWindowSize;

        public override string Act()
        {
            Trigger[] PresentationLogTriggers = Control.PresentationLogHandle.Triggers;
            Trigger[] EDFPlusTriggers = Control.EDFPlusHandle.Triggers;

            if (!Control.TimeConversion.FindTimeConversion(PresentationLogTriggers, EDFPlusTriggers, VerificationWindowSize, Control.ErrorMargin))
                throw new ActionCannotDoWhatDoBeDo("Failed to calculate a time conversion for " + @Path.GetFileName(Control.EDFPlusHandle.FileName) + " and " + @Path.GetFileName(Control.PresentationLogHandle.FileName));

            return "Action: Calculated a time conversion between: " + @Path.GetFileName(Control.EDFPlusHandle.FileName) + " and " + @Path.GetFileName(Control.PresentationLogHandle.FileName);
        }

        public override string GetDescription()
        {
            return "Calculate a time conversion between the files.";
        }

        public ActionGenerateTimeConversion(Controller control, int verificationWindowSize)
            : base(control)
        {
            this.VerificationWindowSize = verificationWindowSize;
        }

    }
}
