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
using HPE_High_Speed_Lane_Parser;
using System.Configuration;

namespace Allegro_PCIE_Lane_Parser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string mlb_directory = "";
        string mlb_fileName = "";
        string riser_directory = "";
        string riser_fileName = "";

        string viaReportLocation = "";
        string etchLengthReportLocation = "";
        string pinPairReportLocation = "";

        List<DiffPairLane> pcieLaneInfoList = new List<DiffPairLane>();
        List<DiffPairLane> clockLanesInfoList = new List<DiffPairLane>();
        List<DiffPairLane> upiLanesInfoList = new List<DiffPairLane>();
        List<DiffPairLane> usbLanesInfoList = new List<DiffPairLane>();

        HashSet<string> connectorRefDesHash = new HashSet<string>();
        string[] MainConnectorRefDes = new string[2];

        List<LaneGroup> aLaneGroupComplete = new List<LaneGroup>();
        List<LaneGroup> bLaneGroupComplete = new List<LaneGroup>();
        List<LaneGroup> upiRxGroupComplete = new List<LaneGroup>();
        List<LaneGroup> upiTxGroupComplete = new List<LaneGroup>();
        List<LaneGroup> otherLaneGroupComplete = new List<LaneGroup>();

        Dictionary<string, int> pciePinPairIndexes = new Dictionary<string, int>();

        UserProjectSettings userSettings = new UserProjectSettings();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow subWindow = new SettingsWindow();
            subWindow.Show();
        }

        private void mlb_btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            FindAndOpenBoardFile(mlb_analyzeBoard, mlb_boardFileLocation, ref mlb_textBlock, ref mlb_directory, ref mlb_fileName);
        }

        private void mlb_analyzeBoard_Click(object sender, RoutedEventArgs e)
        {
            AnalyzeBoardAndPrintCsvFile(ref mlb_textBlock, "mlb", mlb_directory, mlb_fileName);
        }

        private void riser_btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            FindAndOpenBoardFile(riser_analyzeBoard, riser_boardFileLocation, ref riser_textBlock, ref riser_directory, ref riser_fileName);
        }

        private void riser_analyzeBoard_Click(object sender, RoutedEventArgs e)
        {
            AnalyzeBoardAndPrintCsvFile(ref riser_textBlock, "riser", riser_directory, riser_fileName);
        }

        
        private void FindAndOpenBoardFile(Button analyzeBoardButton, TextBox boardFileLocation, ref TextBlock boardOutputTextBlock, ref string boardDirectory, ref string boardFileName)
        {
            analyzeBoardButton.IsEnabled = false;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "BRD files (*.brd)|*.brd|All files (*.*)|*.*";

            // Launch OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = openFileDialog.ShowDialog();

            // Get the selected file name and display in a TextBox.
            // Load content of file in a TextBlock
            if ((result == true) && (openFileDialog.FileName.EndsWith(".brd")))
            {
                boardDirectory = Path.GetDirectoryName(openFileDialog.FileName);
                boardFileName = Path.GetFileName(openFileDialog.FileName);
                boardFileLocation.Text = openFileDialog.FileName;

                boardOutputTextBlock.Text = String.Empty;
                boardOutputTextBlock.Text = "A valid board layout file has been detected. \n";
                boardOutputTextBlock.Text += "------------------------------------------------------------------------ \n\n";

                // Allegro report check and generation 
                BrdReportGen boardReports = new BrdReportGen(boardDirectory, userSettings.cadenceToolLocation, analyzeBoardButton, boardOutputTextBlock);
                bool reportCheck = boardReports.SearchForAllReports();
                if (!reportCheck)
                {
                    boardReports.GenerateAllReports(boardFileLocation.Text);
                    boardReports.GeneratingStatus(mlb_scrollViewer);
                }

                (viaReportLocation, etchLengthReportLocation, pinPairReportLocation) = boardReports.GetReportLocations();
            }
            else
            {
                boardFileLocation.Text = "Invalid board file. Please input a valid board file with the extension '.brd'\n";
            }
        }

        private void AnalyzeBoardAndPrintCsvFile(ref TextBlock boardOutputTextBlock, string boardType, string boardDirectory, string boardFileName)
        {
            userSettings.Reload();
            boardOutputTextBlock.Text = "------------------------------------------------------------------------ \n";
            boardOutputTextBlock.Text += "Now analyzing and parsing all of the generated Allegro Reports. \n";
            // Parse the generated Allegro reports
            AllegroReportParse parser = new AllegroReportParse(viaReportLocation, etchLengthReportLocation, pinPairReportLocation, boardOutputTextBlock, userSettings);

            // Merge all the data parsed from the Allegro reports 
            MergeLaneInfo merge = new MergeLaneInfo(boardOutputTextBlock);

            PrintToCSV csvPrint = new PrintToCSV(boardDirectory, boardFileName, boardOutputTextBlock);

            parser.ParsePinPairReport();
            parser.ParseLengthByLayerReport();
            parser.ParseViaReport();

            pcieLaneInfoList = parser.GetPcieLaneInfo();
            pciePinPairIndexes = parser.GetPinPairIndex(pcieLaneInfoList);
            MainConnectorRefDes = parser.FindMainConnectorRefDes(pcieLaneInfoList, boardType);
            connectorRefDesHash = parser.FindConnectorsAttachedToMainConnector(pcieLaneInfoList, MainConnectorRefDes);        

            boardOutputTextBlock.Text += "Finished parsing all reports. Now beginning to merge all lanes. \n";
            boardOutputTextBlock.Text += "Exporting final lane data to a CSV file. \n";
            boardOutputTextBlock.Text += "------------------------------------------------------------------------ \n";

            merge.MergePcieLanes(pcieLaneInfoList, pciePinPairIndexes, MainConnectorRefDes, connectorRefDesHash, boardType);

            aLaneGroupComplete = merge.GetASideGroup();
            bLaneGroupComplete = merge.GetBSideGroup();

            merge.CheckGroupsForMissingLayers(aLaneGroupComplete);
            merge.CheckGroupsForMissingLayers(bLaneGroupComplete);

            csvPrint.DiffPairLaneGroupsOrdering(aLaneGroupComplete, bLaneGroupComplete);

            if (boardType == "mlb")
            {
                clockLanesInfoList = parser.GetClockLaneInfo();
                upiLanesInfoList = parser.GetUpiLaneInfo();
                usbLanesInfoList = parser.GetUsbLaneInfo();

                merge.MergeOtherLanes("UPI", upiLanesInfoList);
                merge.MergeOtherLanes("CLK", clockLanesInfoList);
                merge.MergeOtherLanes("USB", usbLanesInfoList);

                upiRxGroupComplete = merge.GetUpiRxGroup();
                upiTxGroupComplete = merge.GetUpiTxGroup();
                otherLaneGroupComplete = merge.GetOtherLaneGroup();

                merge.CheckGroupsForMissingLayers(upiRxGroupComplete);
                merge.CheckGroupsForMissingLayers(upiTxGroupComplete);
                merge.CheckGroupsForMissingLayers(otherLaneGroupComplete);

                csvPrint.DiffPairLaneGroupsOrdering(upiRxGroupComplete, upiTxGroupComplete);
                csvPrint.OtherLaneGroupsOrdering(otherLaneGroupComplete);
            }

            csvPrint.WriteToCSV();
        }
    }
}