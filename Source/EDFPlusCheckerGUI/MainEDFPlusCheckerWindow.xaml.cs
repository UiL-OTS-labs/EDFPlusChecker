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
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using EDFPlusChecker.GraphicalUserInterface.ConfigurationWindow;

namespace EDFPlusChecker.GraphicalUserInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainEDFPlusCheckerWindow : Window
    {
        private Controller Engine;
        private BackgroundWorker MyBackGroundWorker;
        private Storyboard RunningIconRotate;

        private LinkedList<ConfigurationPageBase> ActionConfigurationChain;

        public MainEDFPlusCheckerWindow()
        {
            InitializeComponent();
            
            Engine = new Controller();
            MyBackGroundWorker = new BackgroundWorker();
            MyBackGroundWorker.DoWork += new DoWorkEventHandler(MyBackGroundWorker_DoWork);
            MyBackGroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(MyBackGroundWorker_RunWorkerCompleted);
            MyBackGroundWorker.ProgressChanged += new ProgressChangedEventHandler(MyBackGroundWorker_ProgressChanged);
            MyBackGroundWorker.WorkerReportsProgress = true;
            MyBackGroundWorker.WorkerSupportsCancellation = true;

            ActionConfigurationChain = new LinkedList<ConfigurationPageBase>();
            ActionConfigurationChain.AddLast(new OpenFilesPage(Engine));
            ActionConfigurationChain.AddLast(new FixBioTracePage(Engine));
            ActionConfigurationChain.AddLast(new ParseTriggerPage(Engine));
            ActionConfigurationChain.AddLast(new TimeConversionPage(Engine));
            ActionConfigurationChain.AddLast(new CompareTriggerPage(Engine));
            ActionConfigurationChain.AddLast(new SaveFilesPage(Engine));
            ActionConfigurationChain.AddLast(new ActionOverviewPage(Engine));

            // set up to rotate
            //RunningIconRotate = this.FindResource("RotateIcon") as Storyboard;
            //Storyboard.SetTarget(RunningIconRotate, this.RunningIcon);
         
        }

        protected void MyBackGroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker sendingWorker = (BackgroundWorker)sender;
            e.Cancel = !Engine.StartExecution(sendingWorker);
        }

        protected void MyBackGroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(e.Cancelled)
                StatusTextBox.Text = "User Canceled";
            else if (e.Error == null)
                StatusTextBox.Text = "Done!";
            else
            {
                StatusTextBox.Text = "An error has occured!";
                MessageBox.Show("There was an error: " + e.Error.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            this.ConsoleOutputTextBox.AppendText(StatusTextBox.Text);

            ExitButton.IsEnabled = true;
            StartButton.IsEnabled = false;
            LogButton.IsEnabled = true;
            CancelButton.Visibility = System.Windows.Visibility.Hidden;
            Mouse.OverrideCursor = null;

            //RunningIconRotate.Stop();
        }

        protected void MyBackGroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage > 0)
            {
                ProgressBar.Value = e.ProgressPercentage;
                PercentageLabel.Content = e.ProgressPercentage + "%";
            }
            string Description = e.UserState as string;
            this.ConsoleOutputTextBox.AppendText(Description + Environment.NewLine);
            this.ConsoleOutputTextBox.Focus();
            this.ConsoleOutputTextBox.CaretIndex = this.ConsoleOutputTextBox.Text.Length;
            this.ConsoleOutputTextBox.ScrollToEnd();
        }

        public void ConfigurationComplete()
        {
            StartButton.IsEnabled = true;
        }

        public void ClearConfiguration()
        {
            Engine.ClearActionChain();
            StartButton.IsEnabled = false;
            LogButton.IsEnabled = false;
        }

        private void ConfigureButton_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationMainWindow ConfWindow = new ConfigurationMainWindow(this);
            ConfWindow.Show(ActionConfigurationChain);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            //Storyboard sb = this.FindResource("RotateIcon") as Storyboard;
            //Storyboard.SetTarget(sb, this.ProgressIcon);
            //sb.Begin();
            
            if (!MyBackGroundWorker.IsBusy)
            {
                ExitButton.IsEnabled = false;
                StartButton.IsEnabled = false;
                CancelButton.Visibility = System.Windows.Visibility.Visible;

                StatusTextBox.Text = "Running...";
                
                PercentageLabel.Content = "0%";
                Mouse.OverrideCursor = Cursors.Wait;
                //RunningIconRotate.Begin();
                MyBackGroundWorker.RunWorkerAsync();
            }
            //Engine.StartExecution(this.ApplicationLogPath);
            //sb.Stop();
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            AboutBox AboutBox1 = new AboutBox();
            AboutBox1.Show();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            MyBackGroundWorker.CancelAsync();
        }

        private void LogButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(@Engine.ApplicationLogFileName);
        }
    }
}
