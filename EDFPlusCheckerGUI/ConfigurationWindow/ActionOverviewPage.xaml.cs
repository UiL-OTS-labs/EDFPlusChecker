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
    public partial class ActionOverviewPage : ConfigurationPageBase
    {

        public ActionOverviewPage(Controller engine)
            : base(engine)
        {
            InitializeComponent();
        }

        public override bool ConfigureEngine(out string possibleErrorMessage)
        {
            possibleErrorMessage = "";
            return true;
        }

        public override void UndoConfigureEngine()
        {
            Engine.RemovePreviousAction();
        }

        private void ActionOverviewPage_Loaded(object sender, RoutedEventArgs e)
        {
            string[] ActionDescriptions = Engine.GetActionOverview();
            ActionOverview.Text = ">  ";
             ActionOverview.AppendText(String.Join(Environment.NewLine + ">  ", ActionDescriptions));
            ActionOverview.AppendText(Environment.NewLine + ">  Continue to next file (pair)");
        }
    }
}
