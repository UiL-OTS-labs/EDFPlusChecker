using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using EDFPlusChecker.Engine;


namespace EDFPlusChecker.GraphicalUserInterface.ConfigurationWindow
{
    /// <summary>
    /// Interaction logic for OpenFilesPage.xaml
    /// </summary>
    public partial class CompareTriggerPage : ConfigurationPageBase
    {

        public CompareTriggerPage(Controller engine)
            : base(engine)
        {
            InitializeComponent();
        }

        public override bool ConfigureEngine(out string possibleErrorMessage)
        {
            possibleErrorMessage = "";

            try
            {
              ActionCompareTriggers action = new ActionCompareTriggers(Engine, 
                  this.EqualTimingUnEqualNumbers_RemoveRecording.IsChecked == true, 
                  this.UnequalTimingEqualNumbers_RemoveRecording.IsChecked == true,  
                  this.NoMatchEver_AddLogged.IsChecked == true);
                Engine.AddAction(action);
            }
            catch (ActionNotWellConfiguredException e)
            {
                possibleErrorMessage = e.Message;
                return false;
            }
            return true;
        }

        public override void UndoConfigureEngine()
        {
            Engine.RemovePreviousAction();
        }
    }
}
