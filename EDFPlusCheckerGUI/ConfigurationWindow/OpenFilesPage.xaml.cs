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
    public partial class OpenFilesPage : ConfigurationPageBase
    {
        private Microsoft.Win32.OpenFileDialog RecordingFilesDialog;
        private Microsoft.Win32.OpenFileDialog LogFilesDialog;

        public OpenFilesPage(Controller engine)
            : base(engine)
        {
            InitializeComponent();

            RecordingFilesDialog = new Microsoft.Win32.OpenFileDialog();
            RecordingFilesDialog.Multiselect = true;
            RecordingFilesDialog.Filter = "EDF Plus files (*.edf)|*.edf|All files (*.*)|*.*";

            LogFilesDialog = new Microsoft.Win32.OpenFileDialog();
            LogFilesDialog.Multiselect = true;
            LogFilesDialog.Filter = "Presentation Log files (*.log)|*.log|All files (*.*)|*.*";
        }

        public override bool ConfigureEngine(out string possibleErrorMessage)
        {
            possibleErrorMessage = "No Details";
            
            string[] Splitter = { Environment.NewLine };
            string[] RecordingFiles = RecordingFilesTextBox.Text.Split(Splitter, StringSplitOptions.RemoveEmptyEntries);
            string[] LogFiles = LogFilesTextBox.Text.Split(Splitter, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                Engine.SetFileLists(RecordingFiles, LogFiles);
                Engine.AddAction(new ActionOpenFiles(Engine));
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

        private void RecordingFilesTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string[] Splitter = { Environment.NewLine };
            RecordingFilesTextBoxLines.Content = RecordingFilesTextBox.Text.Split(Splitter, StringSplitOptions.RemoveEmptyEntries).Length.ToString();
        }

        private void LogFilesTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string[] Splitter = { Environment.NewLine };
            LogFilesTextBoxLines.Content = LogFilesTextBox.Text.Split(Splitter, StringSplitOptions.RemoveEmptyEntries).Length.ToString();
        }

        private void RecordingFilesButton_Click(object sender, RoutedEventArgs e)
        {
            RecordingFilesDialog.ShowDialog();
            string[] FileList = RecordingFilesDialog.FileNames;
            RecordingFilesTextBox.Text = String.Join(Environment.NewLine, FileList);
        }

        private void LogFilesButton_Click(object sender, RoutedEventArgs e)
        {
            LogFilesDialog.ShowDialog();
            string[] FileList = LogFilesDialog.FileNames;
            LogFilesTextBox.Text = String.Join(Environment.NewLine, FileList);
        }
    }
}
