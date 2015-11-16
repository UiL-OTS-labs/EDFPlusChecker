using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace EDFPlusChecker.GraphicalUserInterface.ConfigurationWindow
{
    public abstract class ConfigurationPageBase : Page 
    {
        protected Controller Engine;

        public abstract bool ConfigureEngine(out string message);

        public abstract void UndoConfigureEngine();

        public ConfigurationPageBase(Controller engine)
            : base()
        {
            this.Engine = engine;
        }
    }
}
