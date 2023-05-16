using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;
using System.Windows.Controls;

namespace Allegro_PCIE_Lane_Parser
{
    internal class BrdReportGen
    {
        private string pinPairReport = "PinPairReport.csv";
        private string viaReport = "ViaByNetReport.csv";
        private string etchLengthReport = "EtchLengthByLayerReport.csv";

        private bool pinPairFlag = false;
        private bool viaFlag = false;
        private bool etchLengthFlag = false;

        private string viaListReportLocation = "";
        private string etchLenReportLocation = "";
        private string pinPairReportLocation = "";

        private string currentBrdPath;
        private string cadenceToolsPath;
        private TextBlock boardTextBlock;
        private Button analyzeBoardButton;

        public BrdReportGen(string path, string toolsPath, Button analyzeButton, TextBlock textBlock)
        {
            currentBrdPath = path;
            cadenceToolsPath = toolsPath;
            boardTextBlock = textBlock;
            analyzeBoardButton = analyzeButton;
        }

        private void CurrentReportStatus()
        {
            pinPairFlag = File.Exists(currentBrdPath + @"\" + pinPairReport);
            viaFlag = File.Exists(currentBrdPath + @"\" + viaReport);
            etchLengthFlag = File.Exists(currentBrdPath + @"\" + etchLengthReport);
        }

        private void UpdateReportLocations()
        {
            viaListReportLocation = $@"{currentBrdPath}\ViaByNetReport.csv";
            etchLenReportLocation = $@"{currentBrdPath}\EtchLengthByLayerReport.csv";
            pinPairReportLocation = $@"{currentBrdPath}\PinPairReport.csv";
        }


        public bool SearchForAllReports()
        {
            CurrentReportStatus();

            boardTextBlock.Text += "Now searching for the following Allegro generated reports (Pin pair, Via list, Etch length) \n";

            // Check if each file exist
            if (pinPairFlag)
            {
                boardTextBlock.Text += " - Pin pair report has been found \n";
            }
            else
            {
                boardTextBlock.Text += " - Pin pair report has NOT been found \n";
            }

            if (viaFlag)
            {
                viaFlag = true;
                boardTextBlock.Text += " - Via list report has been found \n";
            }
            else
            {
                boardTextBlock.Text += " - Via list report has NOT been found \n";
            }

            if (etchLengthFlag)
            {
                etchLengthFlag = true;
                boardTextBlock.Text += " - Etch length report has been found \n";
            }
            else
            {
                boardTextBlock.Text += " - Etch length report has NOT been found \n";
            }

            // If one or more files is missing, begin generating files. Otherwise move on to next step. 
            if (pinPairFlag && viaFlag && etchLengthFlag)
            {
                boardTextBlock.Text += "**** All necessary reports have been found. You can click on 'Analyze Board & Start' to move to the next step. ****\n";
                analyzeBoardButton.IsEnabled = true;
            }
            else
            {
                boardTextBlock.Text += "------------------------------------------------------------------------ \n";
                boardTextBlock.Text += "One or more files are missing. Now beginning to generate all reports. \n";
                //boardTextBlock.Text += "One or more files are missing. Click 'Start/Run' to generate all reports. \n";
            }

            return (pinPairFlag && viaFlag && etchLengthFlag);
        }


        public void GenerateAllReports(string brdFile)
        {
            string reportCmd = $@"{cadenceToolsPath}\report.exe";
            UpdateReportLocations();

            string[] commandsArr =
            {
                $@"{reportCmd} -v elp ""{brdFile}"" ""{pinPairReportLocation}""",
                $@"{reportCmd} -v ell ""{brdFile}"" ""{etchLenReportLocation}""",
                $@"{reportCmd} -v vialist_net ""{brdFile}"" ""{viaListReportLocation}""",
            };

            boardTextBlock.Text += "------------------------------------------------------------------------ \n";
            boardTextBlock.Text += "Now generating the via list report. \n";
            boardTextBlock.Text += "Now generating the etch length report. \n";
            boardTextBlock.Text += "Now generating the pin pair report -- This generation may take a while. \n";

            ProcessStart(commandsArr);

            boardTextBlock.Text += "All reports are now being generated in the background. Please check the folder of your board file to ensure completion. \n";
        }

        public async void GeneratingStatus()
        {
            while (!pinPairFlag)
            {
                await Task.Delay(2000);
                CurrentReportStatus();
                boardTextBlock.Text += "  --- Generating \n";
            }

            boardTextBlock.Text += "**** All necessary reports have been generated. Click on 'Analyze Board & Start' to move to the next step. ****\n";
            analyzeBoardButton.IsEnabled = true;
        }

        public (string, string, string) GetReportLocations()
        {
            UpdateReportLocations();
            return (viaListReportLocation, etchLenReportLocation, pinPairReportLocation);
        }

        private void ProcessStart(string[] Commands)
        {
            Parallel.ForEach(Commands, command =>
                {
                    ProcessStartInfo ProcessInfo;
                    Process myProcess;

                    ProcessInfo = new ProcessStartInfo("cmd.exe", $"/c {command}");

                    ProcessInfo.RedirectStandardOutput = true;
                    ProcessInfo.CreateNoWindow = true;
                    ProcessInfo.UseShellExecute = false;

                    myProcess = Process.Start(ProcessInfo);
                });
        }
    }
}
