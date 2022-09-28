using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace Allegro_PCIE_Lane_Parser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string mlb_directory = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        public string mlb_textBoxValue
        {
            get {  return mlb_textBox.Text; }
            set { mlb_textBox.Text = value; }
        }

        private void mlb_btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "BRD files (*.brd)|*.brd|All files (*.*)|*.*";

            // Launch OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = openFileDialog.ShowDialog();

            // Get the selected file name and display in a TextBox.
            // Load content of file in a TextBlock
            if ((result == true) && (openFileDialog.FileName.EndsWith(".brd")))
            {
                mlb_directory = Path.GetDirectoryName(openFileDialog.FileName);
                mlb_boardFileLocation.Text = openFileDialog.FileName;
                mlb_runProgram.IsEnabled = true;
            }
            else
            {
                mlb_boardFileLocation.Text = "Invalid board file. Please input a valid board file with the extension '.brd'";
            }
        }

        private void mlb_settings_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mlb_runProgram_Click(object sender, RoutedEventArgs e)
        {
            // Will need to add a class/function to parse allegro reports to get total lane and connector usage
            mlb_textBox.Text = String.Empty;

            // Allegro report check and generation 
            BrdReportGen mlbReports = new BrdReportGen(mlb_directory, @"C:\Cadence\SPB_17.2\tools\bin");
            bool reportCheck = mlbReports.SearchForAllReports();
            if (!reportCheck) { mlbReports.GenerateAllReports(mlb_boardFileLocation.Text); }

            // Parse Allegro reports

        }
    }
}
