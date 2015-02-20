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
            try
            {
                if(ParseRecordingTriggersCheckBox.IsChecked == true)
                {
                    string Prefix = RecordingFilePrefixTextbox.Text;
                    string Postfix = RecordingFilePostfix.Text;

                    Engine.AddAction(new ActionParseTriggers(Engine, Prefix, Postfix));

                    int TriggerLowerLimit;
                    if (!int.TryParse(LogFileMinimumTriggerTextBox.Text, out TriggerLowerLimit))
                        throw new ActionNotWellConfiguredException("Minimum trigger from trigger range is not an integer.");

                    int TriggerUpperLimit;
                    if (!int.TryParse(LogFileMaximumTriggerTextBox.Text, out TriggerUpperLimit))
                        throw new ActionNotWellConfiguredException("Maximum trigger from trigger range is not an integer.");

                    int[] IgnoreTriggers_int;
                    if (TriggersToIgnoreTextBox.Text != "")
                    {
                        string[] IgnoreTriggers_str = TriggersToIgnoreTextBox.Text.Split(new char[] { ';' });
                        IgnoreTriggers_int = new int[IgnoreTriggers_str.Length];
                        int index = 0;
                        foreach (string IgnoreTrigger_str in IgnoreTriggers_str)
                        {
                            if (!int.TryParse(IgnoreTrigger_str, out IgnoreTriggers_int[index++]))
                                throw new ActionNotWellConfiguredException("Ignore triggers ill-defined!");
                        }
                    }
                    else
                        IgnoreTriggers_int = new int[0];

                    Engine.TriggerNumberLowerLimit = TriggerLowerLimit;
                    Engine.TriggerNumberUpperLimit = TriggerUpperLimit;
                    Engine.TriggerNumbersToIgnore = IgnoreTriggers_int;
                    Engine.CorrectForPauses = CorrectForPausesCheckbox.IsChecked == true;
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

        private void LogFileTriggerRangeInformationButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("There might be integers put into the Event Code of Presentation that are not to be recognised as trigger numbers.", "Information", MessageBoxButton.OK, MessageBoxImage.Question);
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
