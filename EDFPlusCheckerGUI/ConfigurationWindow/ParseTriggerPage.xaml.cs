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
            FixBioTraceTriggersCheckBox.IsChecked = true;
            ParseRecordingTriggersCheckBox.IsChecked = true;
        }

        public override bool ConfigureEngine(out string possibleErrorMessage)
        {
            possibleErrorMessage = "";
           ;
            

            try
            {
                if(FixBioTraceTriggersCheckBox.IsChecked == true)
                {
                    string FirstFlag = BioTraceFixFirstFlagTextBox.Text;
                    string SecondFlag = BioTraceFixSecondFlagTextBox.Text;
                    char[] ReplacementChar = BioTraceFixReplaceMentTextBox.Text.ToCharArray();
                    Engine.AddAction(new ActionFixBioTraceAnnotations(Engine, FirstFlag, SecondFlag, ReplacementChar[0]));
                }
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
            if (FixBioTraceTriggersCheckBox.IsChecked == true)
            {
                Engine.RemovePreviousAction();
            }
            if (ParseRecordingTriggersCheckBox.IsChecked == true)
            {
                Engine.RemovePreviousAction();
            }
        }

        private void FixBioTraceTriggersCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            BioTraceTriggersGrid.IsEnabled = false;
        }

        private void FixBioTraceTriggersCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            BioTraceTriggersGrid.IsEnabled = true;
        }

        private void BioTraceInformationButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("BioTrace software (v2012c) has a bug. The EDF+ export function allows incorrect bytes in the EDF annotations. Usually it is in this format: [Trigger: 21('<INCORRECT BYTE>')].\n\nThis function fixes this by scanning through the raw EDF+ file in memory. When the byte sequence of the first flag is detected in sequence the detection is continued with the second-flag byte representation. At a hit the next byte is replaced and the first-flag detection is rearmed.\n\n For instance: [Trigger 21:('<byte>')] becomes [Trigger 21:('t')].", "Information", MessageBoxButton.OK, MessageBoxImage.Question);
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
