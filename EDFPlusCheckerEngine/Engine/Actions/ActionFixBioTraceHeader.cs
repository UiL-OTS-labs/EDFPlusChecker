using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace EDFPlusChecker.Engine
{
    public class ActionFixBioTraceHeader : BaseAction
    {
        #region Action Settings

        public sbyte LowerSByteValueLimit;
        public sbyte UpperSByteValueLimit;
        public char ReplacementChar;

        #endregion Action Settings

        public override string Act()
        {
            if (Char.IsControl(ReplacementChar))
                throw new ActionCannotDoWhatDoBeDo("Replacement character (" + ReplacementChar + ") for the Biotrace header check is illegal!");
            

            ASCIIEncoding encoding = new ASCIIEncoding();
            Stream ByteStream = Control.EDFPlusHandle.MemoryHandle;
            ByteStream.Seek(0, SeekOrigin.Begin);

            BinaryReader br = new BinaryReader(ByteStream, encoding);
            BinaryWriter bw = new BinaryWriter(ByteStream, encoding);

            // While loop is simple. See if the bytes match up with the firstflag, then... when armed... 
            // check for the secondflag and if THAT is armed replace the following byte.
            while (ByteStream.Position < Control.EDFPlusHandle.FileInfo.HeaderBytes && ByteStream.Position < ByteStream.Length)
            {
                sbyte c = br.ReadSByte();
                if(c < LowerSByteValueLimit || c > UpperSByteValueLimit)
                {
                    ByteStream.Position--;
                    bw.Write(ReplacementChar);
                }
            }

            //make sure it is reloaded!
            Control.EDFPlusHandle.ReReadHeader();

            return "Action: Removed any illegal bytes from the EDF+-file header and replaced it with '" + this.ReplacementChar + "'.";
        }

        #region Constructor
        public ActionFixBioTraceHeader(Controller cont, int lowerByteValueLimit = 32, int upperByteValueLimit = 126, char replacementString = '?')
            : base(cont)
        {
            if (lowerByteValueLimit < sbyte.MinValue || lowerByteValueLimit > sbyte.MaxValue)
                throw new ActionCannotDoWhatDoBeDo("BioTraceFixHeader: lower byte value limit is out of range");
            if (upperByteValueLimit < sbyte.MinValue || upperByteValueLimit > sbyte.MaxValue)
                throw new ActionCannotDoWhatDoBeDo("BioTraceFixHeader: upper byte value limit is out of range");

            this.LowerSByteValueLimit = Convert.ToSByte(lowerByteValueLimit);
            this.UpperSByteValueLimit = Convert.ToSByte(upperByteValueLimit);
            
            this.ReplacementChar = replacementString;
        }
        #endregion Constructor

        public override string GetDescription()
        {
            string Description = String.Format("Read over the EDF+ file header in memory and replace any ASCII chars {0}-{1} with '{2}'", this.LowerSByteValueLimit, this.UpperSByteValueLimit, this.ReplacementChar);
            return Description;
        }
    }
}
