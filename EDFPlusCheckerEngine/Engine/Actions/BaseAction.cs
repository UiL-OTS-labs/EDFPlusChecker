using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDFPlusChecker.Engine
{
    public abstract class BaseAction
    {
        private bool _active;
        public bool Active { get { return _active; } set { _active = value; } }
        protected Controller Control;

        public abstract string Act();

        public BaseAction(Controller cont)
        {
            this.Control = cont;
        }

        abstract public string GetDescription();
    }
}
