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
using System.Windows.Forms;



namespace EDFPlusChecker.GraphicalUserInterface.ConfigurationWindow
{
    /// <summary>
    /// Interaction logic for OpenFilesPage.xaml
    /// </summary>
    public partial class SaveFilesPage : ConfigurationPageBase
    {

        private SaveFileDialog ApplicationLogPathDialog;
        private FolderBrowserDialog OutputDirectoryPathDialog;

        public SaveFilesPage(Controller engine)
            : base(engine)
        {
            InitializeComponent();
            ApplicationLogPathDialog = new SaveFileDialog();
            ApplicationLogPathDialog.Filter = "Log files (*.log)|*.log|All files (*.*)|*.*";

            OutputDirectoryPathDialog = new FolderBrowserDialog();
        }

        public override bool ConfigureEngine(out string possibleErrorMessage)
        {
            possibleErrorMessage = "";

            if (this.IsEnabled != true)
                return true;

            try
            {
                if (ResolveDifferencesCheckBox.IsChecked == true)
                {
                    bool AddFlag = AddTriggersFlagCheckBox.IsChecked == true;
                    bool RemoveFlag = RemoveTriggersFlagCheckBox.IsChecked == true;
                    bool ReplaceFlag = ReplaceTriggersFlagCheckBox.IsChecked == true;

                    Engine.AddAction(new ActionResolveTriggerDifferences(Engine, Engine.ErrorMargin, ReplaceFlag, RemoveFlag, AddFlag));

                }
                    
                if(SaveModifiedFilesCheckbox.IsChecked == true)
                {
                    string SavePath = OutputDirectoryTextBox.Text;
                    string Prefix = NewFilesPrefixTextBox.Text;
                    Engine.AddAction(new ActionSaveEDFFile(Engine, SavePath, Prefix));
                }
                Engine.ApplicationLogFileName = this.ApplicationLogPathTextBox.Text;
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
            if (ResolveDifferencesCheckBox.IsChecked == true)
            {
                Engine.RemovePreviousAction();
            }

            if (SaveModifiedFilesCheckbox.IsChecked == true)
            {
                Engine.RemovePreviousAction();
            }
        }

        private void SelectOutputFileButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationLogPathDialog.ShowDialog();
            ApplicationLogPathTextBox.Text = ApplicationLogPathDialog.FileName;
        }

        private void SaveFilesPage_Loaded(object sender, RoutedEventArgs e)
        {
            ResolveDifferencesCheckBox.IsEnabled = Engine.HasLogFiles;
            ResolveDifferencesCheckBox.IsChecked = Engine.HasLogFiles;
            SaveModifiedFilesCheckbox.IsChecked = true;
            OutputDirectoryTextBox.Text = OutputDirectoryPathDialog.SelectedPath;
        }

        private void OutputDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            OutputDirectoryPathDialog.SelectedPath = OutputDirectoryTextBox.Text;
            OutputDirectoryPathDialog.ShowDialog();
            OutputDirectoryTextBox.Text = OutputDirectoryPathDialog.SelectedPath;
        }

        private void SaveModifiedFilesCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            OutPutFilesGrid.IsEnabled = false;
        }

        private void SaveModifiedFilesCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            OutPutFilesGrid.IsEnabled = true;
        }

        private void ResolveDifferencesCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ResolveDetailGroupBox.IsEnabled = true;
        }

        private void ResolveDifferencesCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ResolveDetailGroupBox.IsEnabled = false;
        }
    }
}
