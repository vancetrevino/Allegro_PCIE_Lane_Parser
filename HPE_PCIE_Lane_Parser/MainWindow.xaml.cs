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
using Allegro_PCIE_Lane_Parser.Code_Files;
using Allegro_PCIE_Lane_Parser.Class_Files;

namespace Allegro_PCIE_Lane_Parser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string mlb_directory = "";
        string mlb_fileName = "";
        string viaReportLocation = "";
        string etchLengthReportLocation = "";
        string pinPairReportLocation = "";

        List<DiffPairLane> pcieLaneInfoList = new List<DiffPairLane>();
        List<DiffPairLane> clockLanesInfoList = new List<DiffPairLane>();
        List<DiffPairLane> upiLanesInfoList = new List<DiffPairLane>();
        List<DiffPairLane> usbLanesInfoList = new List<DiffPairLane>();

        HashSet<string> connectorRefDesHash = new HashSet<string>();

        List<LaneGroup> aLaneGroupComplete = new List<LaneGroup>();
        List<LaneGroup> bLaneGroupComplete = new List<LaneGroup>();

        Dictionary<string, int> pciePinPairIndexes = new Dictionary<string, int>();

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
                mlb_fileName = Path.GetFileName(openFileDialog.FileName);
                mlb_boardFileLocation.Text = openFileDialog.FileName;

                mlb_textBox.Text = String.Empty;
                mlb_textBox.Text = "A valid board layout file has been detected. \n";

                // Allegro report check and generation 
                BrdReportGen mlbReports = new BrdReportGen(mlb_directory, @"C:\Cadence\SPB_17.2\tools\bin");
                bool reportCheck = mlbReports.SearchForAllReports();
                if (!reportCheck) 
                { 
                    mlbReports.GenerateAllReports(mlb_boardFileLocation.Text);
                    mlbReports.GeneratingStatus();
                }

                (viaReportLocation, etchLengthReportLocation, pinPairReportLocation) = mlbReports.GetReportLocations();
    }
            else
            {
                mlb_boardFileLocation.Text = "Invalid board file. Please input a valid board file with the extension '.brd'";
            }
        }

        private void mlb_settings_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mlb_analyzeBoard_Click(object sender, RoutedEventArgs e)
        {
            mlb_textBox.Text = "Now analyzing all of the generated Allegro Reports to parse the lanes. \n";
            // Parse the generated Allegro reports
            AllegroReportParse parser = new AllegroReportParse(viaReportLocation, etchLengthReportLocation, pinPairReportLocation);

            parser.ParsePinPairReport();
            parser.ParseLengthByLayerReport();
            parser.ParseViaReport();

            pcieLaneInfoList = parser.GetPcieLaneInfo();
            clockLanesInfoList = parser.GetClockLaneInfo();
            upiLanesInfoList = parser.GetUpiLaneInfo();
            usbLanesInfoList = parser.GetUsbLaneInfo();
            connectorRefDesHash = parser.GetConnRefDes();
            pciePinPairIndexes = parser.GetPinPairIndex(pcieLaneInfoList);

            foreach (var x in clockLanesInfoList)
            {
                foreach (var y in x.LayerAndLengths)
                {
                    Trace.WriteLine(x.NetName + " -- " + x.FullPinPair + " -- " + x.ViaCount + " -- " + y.Key + " - " + y.Value);
                }
            }

            mlb_textBox.Text += "Finished parsing all reports. Please click 'Start/Run' to continue the program. \n";
        }

        private void mlb_runProgram_Click(object sender, RoutedEventArgs e)
        {
            // Will need to add a class/function to parse allegro reports to get total lane and connector usage
            mlb_textBox.Text = String.Empty;
            mlb_textBox.Text = "Now beginning to run the program. \n";
            mlb_textBox.Text += "Exporting final lane data to a CSV file. \n";

            // Merge all the data parsed from the Allegro reports 
            MergeLaneInfo merge = new MergeLaneInfo(connectorRefDesHash);

            merge.MergePcieLanes(pcieLaneInfoList, pciePinPairIndexes);

            aLaneGroupComplete = merge.GetASideGroup();
            bLaneGroupComplete = merge.GetBSideGroup();


            // Begin printing out to a CSV file.
            PrintToCSV csvPrint = new PrintToCSV(mlb_directory, mlb_fileName, aLaneGroupComplete, bLaneGroupComplete);
            csvPrint.LaneGroupsOrdering();
            csvPrint.WriteToCSV();

            // mlb_directory
        }
    }
}
