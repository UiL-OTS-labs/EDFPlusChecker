using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDFPlusChecker.Engine
{
    public class ActionNotWellConfiguredException : Exception
    {
        public ActionNotWellConfiguredException(string message)
        : base(message)
        {
        }

    }

    public class ActionCannotDoWhatDoBeDo : Exception
    {
        public ActionCannotDoWhatDoBeDo(string message)
            : base(message)
        {
        }

    }
}
