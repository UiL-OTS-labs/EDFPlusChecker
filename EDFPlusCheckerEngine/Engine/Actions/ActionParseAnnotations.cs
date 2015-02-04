
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeuroLoopGainLibrary.Edf;
using System.IO;

namespace EDFPlusChecker.Engine
{
    public class ActionParseAnnotations : BaseAction
    {
        #region Action Settings

        public string PreFixMatch { get; set; }
        public string PostFixMatch { get; set; }

        #endregion Action Settings

        public override string Act()
        {
            Active = true;
            bool check = Control.EDFPlusHandle.ValidFormat;
            if (!Control.EDFPlusHandle.ValidFormat)
                throw new ActionCannotDoWhatDoBeDo(@Control.EDFPlusHandle.FileName + " is not in a valid EDF+ format! Most commonly this is because of illegal ASCII characters in the header. Only ASCII characters 32-126 are allowed (EDF specifications of 1992).");

            //Seems that the autoread function of the library is not alway accurate!
            if (!Control.EDFPlusHandle.TALRead)
                Control.EDFPlusHandle.ReadTALsToMemory();

            EdfPlusAnnotationList list = Control.EDFPlusHandle.TAL;
            
            foreach (EdfPlusAnnotation annotation in list)
            {
                int trigger = ParseAnnotation(annotation.Annotation);
                if (trigger >= 0)
                    annotation.Annotation = trigger.ToString();
                else
                    annotation.Annotation = annotation.Annotation += ":NaN";
            }

            Active = false;
            return "Action: Parsed Annotations on " + @Path.GetFileName(Control.EDFPlusHandle.FileName);
        }

        public Int32 ParseAnnotation(string original)
        {
            if( original.Contains(PreFixMatch) && original.Contains(PostFixMatch) )
            {
               int start = original.IndexOf(PreFixMatch) + PreFixMatch.Length;
               int end = original.IndexOf(PostFixMatch, start);
               String number = original.Substring(start, end - start);

               Int32 trigger;
               if (!int.TryParse(number, out trigger))
                   return -2; // if match but the match isn't a string -2
               else
                   return trigger;
            }
            else
            {
                return -1;
            }
        }

        public override string GetDescription()
        {
            return "Parse/Translate Annotations on EDF file from string format to integers";
        }

        public ActionParseAnnotations(Controller cont, string prefix, string postfix)
            : base(cont)
        {
            this.PreFixMatch = prefix;
            this.PostFixMatch = postfix;
        }
    }
}
