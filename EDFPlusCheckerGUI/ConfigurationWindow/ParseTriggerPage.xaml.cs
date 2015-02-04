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
    public partial class ParseTriggerPage : ConfigurationPageBase
    {

        public ParseTriggerPage(Controller engine)
            : base(engine)
        {
            InitializeComponent();
            ParseRecordingTriggersCheckBox.IsChecked = true;
        }

        public override bool ConfigureEngine(out string possibleErrorMessage)
        {
            possibleErrorMessage = "";
           ;
            

            try
            {
                if(ParseRecordingTriggersCheckBox.IsChecked == true)
                {
                    string Prefix = RecordingFilePrefixTextbox.Text;
                    string Postfix = RecordingFilePostfix.Text;

                    Engine.AddAction(new ActionParseAnnotations(Engine, Prefix, Postfix));
                }
                
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
            if (ParseRecordingTriggersCheckBox.IsChecked == true)
            {
                Engine.RemovePreviousAction();
            }
        }

        private void EDFAnnotationInformationButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Extracts the trigger number from the string based annotation in EDF+ files.", "Information", MessageBoxButton.OK, MessageBoxImage.Question);
        }

        private void ParseRecordingTriggersCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ParseRecordingTriggersGrid.IsEnabled = true;
        }

        private void ParseRecordingTriggersCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ParseRecordingTriggersGrid.IsEnabled = false;
        }
    }
}
