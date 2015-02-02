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
    public partial class CompareTriggersPage : ConfigurationPageBase
    {

        public CompareTriggersPage(Controller engine)
            : base(engine)
        {
            InitializeComponent();
        }

        public override bool ConfigureEngine(out string possibleErrorMessage)
        {
            possibleErrorMessage = "";

            if (this.IsEnabled == true && ComparingTriggersCheckBox.IsChecked == true)
            try
            {
                int WindowSize;
                if(!int.TryParse(VerificationWindowSizeTextBox.Text, out WindowSize))
                    throw new ActionNotWellConfiguredException("Window size is not an integer.");

                double ErrorMarginInMilliSeconds;
                if (!double.TryParse(VerificationErrorMarginTextBox.Text, out ErrorMarginInMilliSeconds))
                    throw new ActionNotWellConfiguredException("Error margin (ms) is not an a valid double.");

                int TriggerLowerLimit;
                if(!int.TryParse(LogFileMinimumTriggerTextBox.Text, out TriggerLowerLimit))
                    throw new ActionNotWellConfiguredException("Minimum trigger from trigger range is not an integer.");

                int TriggerUpperLimit;
                if (!int.TryParse(LogFileMaximumTriggerTextBox.Text, out TriggerUpperLimit))
                    throw new ActionNotWellConfiguredException("Maximum trigger from trigger range is not an integer.");

                Engine.AddAction(new ActionCompareTriggers(Engine, WindowSize, ErrorMarginInMilliSeconds / 1000, TriggerLowerLimit, TriggerUpperLimit, RemoveLogTriggersFromRecCheckBox.IsChecked == true));
              } catch(ActionNotWellConfiguredException e)
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

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            TimeConversionGroupBox.IsEnabled = true;
            LogFileCompareTriggersGroupBox.IsEnabled = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            TimeConversionGroupBox.IsEnabled = false;
            LogFileCompareTriggersGroupBox.IsEnabled = false;
        }

        private void TimeConversionInformationButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("There are two stacks of triggers: the recorded stack and the logged stack. The onsets of both lists need to be synchronized. In order we to do so we need a start and an end trigger for each stack. The algorithm tries to find these four triggers by making sure the start triggers of the stacks have equal triggers numbers and the end triggers of the stacks have equal trigger numbers. It then calculates a time conversion using this formula:\n\nRectime_for_missing_marker = ( Logtime_for_missing_marker - LogStart) * (RecordingEnd- RecordingStart) / (LogEnd - LogStart) ) + RecordingStart.\n\n The time conversion is considered correct if there is an array of set length of triggers in both stacks that are equal in both trigger number and trigger timing with a set error margin. If it fails it retries the checks with other newly selected triggers from the stacks.", "Information", MessageBoxButton.OK, MessageBoxImage.Question);
        }

        private void LogFileTriggerRangeInformationButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("There might be integers put into the Event Code of Presentation that are not to be recognised as trigger numbers.", "Information", MessageBoxButton.OK, MessageBoxImage.Question);
        }

        private void RemoveLogTriggersFromRecInformationButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Searches and removes a log trigger in the recording based soley on trigger number. Program only does this when it's pairing recording trigger was not found at or around the specific time point determined by the time conversion.\n\n Use this if triggers sometimes pop us at completely wrong time points in the recording and want to prevent duplicates in any output files.", "Information", MessageBoxButton.OK, MessageBoxImage.Question);
        }

        private void CompareTriggersPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = Engine.HasLogFiles;
            ComparingTriggersCheckBox.IsChecked = Engine.HasLogFiles;
            TimeConversionGroupBox.IsEnabled = Engine.HasLogFiles;
            LogFileCompareTriggersGroupBox.IsEnabled = Engine.HasLogFiles;
            RemoveLogTriggersFromRecCheckBox.IsEnabled = Engine.HasLogFiles;
        }
    }
}
