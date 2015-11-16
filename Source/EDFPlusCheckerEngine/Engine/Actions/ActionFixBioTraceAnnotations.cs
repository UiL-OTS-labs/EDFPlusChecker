using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NeuroLoopGainLibrary.Edf;

namespace EDFPlusChecker.Engine
{
    public class ActionFixBioTraceAnnotations : BaseAction
    {
        #region Action Settings

        public string FirstFlag { get; set; }
        public string SecondFlag { get; set; }
        public char ReplacementChar;
        
        #endregion Action Settings

        public override string Act()
        {
            Active = true;

            if (Control.EDFPlusHandle.TALRead)
                throw new ActionCannotDoWhatDoBeDo("TAL are already read in, no use going over the origin buffer! Call open files such that this does not happen!");

            UTF8Encoding encoding = new UTF8Encoding();

            byte[] FirstFlagArray = encoding.GetBytes(FirstFlag.ToCharArray());
            int FirstCounter = 0;
            byte[] SecondFlagArray = encoding.GetBytes(SecondFlag.ToCharArray());
            int SecondCounter = 0;

            char[] rep = { ReplacementChar };
            byte[] replacement = encoding.GetBytes(rep, 0, 1);

            Stream ByteStream = Control.EDFPlusHandle.MemoryHandle;
            long dataBlockOffset = Control.EDFPlusHandle.AnnotationSignalOffsetBytes;
            if (ByteStream.Position != dataBlockOffset)
                ByteStream.Seek(dataBlockOffset, SeekOrigin.Begin);

            

            BinaryReader br = new BinaryReader(ByteStream, encoding);
            BinaryWriter bw = new BinaryWriter(ByteStream, encoding);
            bool FirstHit = false;

            int replacements = 0;
            // While loop is simple. See if the bytes match up with the firstflag, then... when armed... 
            // check for the secondflag and if THAT is armed replace the following byte.
            while(ByteStream.Position < ByteStream.Length)
            {
                byte c = br.ReadByte();
                if (FirstHit)
                {
                    if (c == SecondFlagArray[SecondCounter])
                    {
                        if (SecondCounter == SecondFlagArray.Length - 1)
                        {      
                            ByteStream.WriteByte(replacement[0]);     
                            FirstHit = false;
                            replacements++;
                            SecondCounter = 0;
                            FirstCounter = 0;
                        }
                        else
                        {
                            SecondCounter++;
                        }
                    }
                    else
                    {
                        SecondCounter = 0;
                    }
                }
                else
                {
                    if (c == FirstFlagArray[FirstCounter])
                    {
                        if (FirstCounter == FirstFlagArray.Length - 1)
                        {
                            FirstHit = true;
                            FirstCounter = 0;
                        }
                        else
                        {
                            FirstCounter++;
                        }
                    }
                    else
                    {
                        FirstCounter = 0;
                    }
                }
                
            }
            Active = false;

            if (replacements == 0)
                throw new ActionCannotDoWhatDoBeDo("Did not fix any of the faulty bytes in the BioTrace Fix. Recheck the flags!");

            Control.EDFPlusHandle.ReadTALsToMemory();
            EdfPlusAnnotationList list = Control.EDFPlusHandle.TAL;

            return "Action: Annotations fixed in " + @Path.GetFileName(Control.EDFPlusHandle.FileName);
        }


        public override string GetDescription()
        {
            return "Fix byte infections as seen from BioTrace Recording Software.";
        }

        public ActionFixBioTraceAnnotations(Controller cont, string firstFlag, string secondFlag, char replacementChar)
            : base (cont)
        {
            this.FirstFlag = firstFlag;
            this.SecondFlag = secondFlag;
            this.ReplacementChar = replacementChar;
        }
    }
}