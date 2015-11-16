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
using System.Windows.Shapes;

namespace EDFPlusChecker.GraphicalUserInterface.ConfigurationWindow
{
    /// <summary>
    /// Interaction logic for ActionConfigurationOpenFiles.xaml
    /// </summary>
    public partial class ConfigurationMainWindow : Window
    {
        LinkedList<ConfigurationPageBase> PageChain;
        LinkedListNode<ConfigurationPageBase> CurrentPageNode;
        MainEDFPlusCheckerWindow MainWindow;

        public void SetControls()
        {
            if (PageChain.Last.Equals(CurrentPageNode))
            {
                NextButton.Visibility = Visibility.Hidden;
                FinishButton.Visibility = Visibility.Visible;
            }
            else
            {
                NextButton.Visibility = Visibility.Visible;
                FinishButton.Visibility = Visibility.Hidden;
            }

            if (PageChain.First.Equals(CurrentPageNode))
            {
                PreviousButton.Visibility = Visibility.Hidden;
            }
            else
            {
                PreviousButton.Visibility = Visibility.Visible;
            }
        }

        public ConfigurationMainWindow(MainEDFPlusCheckerWindow mainWindow)
        {
            this.MainWindow = mainWindow;
            MainWindow.ClearConfiguration();
            InitializeComponent();
        }

        public void Show(LinkedList<ConfigurationPageBase> pageChain)
        {
            if (pageChain.Count == 0)
                throw new InvalidOperationException("Cannot call ConfigurationWindow with nothing to show!");

            this.PageChain = pageChain;
            SwitchToPage(PageChain.First);

            base.Show();
        }

        public void SwitchToPage(LinkedListNode<ConfigurationPageBase> NewPageNode)
        {
            if (NewPageNode != null)
            {
                this.CurrentPageNode = NewPageNode;
                this.PageFrame.Content = NewPageNode.Value;
                SetControls();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if(CheckCurrentConfigurationPage())
                SwitchToPage(CurrentPageNode.Next);
        }

        private bool CheckCurrentConfigurationPage()
        {
            string Message;
            bool ConfigurationOK = CurrentPageNode.Value.ConfigureEngine(out Message);
            if (!ConfigurationOK)
                MessageBox.Show(Message, "Configuration Incorrect", MessageBoxButton.OK, MessageBoxImage.Warning);
            return ConfigurationOK;
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentPageNode.Previous.Value.UndoConfigureEngine();
            SwitchToPage(CurrentPageNode.Previous);
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentConfigurationPage())
            {
                MainWindow.ConfigurationComplete();
                base.Close();
            }
            MainWindow.Focus();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ClearConfiguration();
            this.Close();
            MainWindow.Focus();
        }
    }
}
