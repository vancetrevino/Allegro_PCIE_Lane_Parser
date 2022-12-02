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

        MainWindow mw = (MainWindow)Application.Current.MainWindow;

        private string currentBrdPath;
        private string cadenceToolsPath;

        public BrdReportGen(string path, string toolsPath)
        {
            currentBrdPath = path;
            cadenceToolsPath = toolsPath;
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

            mw.mlb_textBoxValue += "Now searching for the following Allegro generated reports (Pin pair, Via list, Etch length) \n";

            // Check if each file exist
            if (pinPairFlag)
            {
                mw.mlb_textBoxValue += " - Pin pair report has been found \n";
            }
            else
            {
                mw.mlb_textBoxValue += " - Pin pair report has NOT been found \n";
            }

            if (viaFlag)
            {
                viaFlag = true;
                mw.mlb_textBoxValue += " - Via list report has been found \n";
            }
            else
            {
                mw.mlb_textBoxValue += " - Via list report has NOT been found \n";
            }

            if (etchLengthFlag)
            {
                etchLengthFlag = true;
                mw.mlb_textBoxValue += " - Etch length report has been found \n";
            }
            else
            {
                mw.mlb_textBoxValue += " - Etch length report has NOT been found \n";
            }

            // If one or more files is missing, begin generating files. Otherwise move on to next step. 
            if (pinPairFlag && viaFlag && etchLengthFlag)
            {
                mw.mlb_textBoxValue += "All Allegro necessary reports have been found. You can click on either 'Analyze Board File' or 'Start/Run' to move to the next step. \n";
                mw.mlb_analyzeBoard.IsEnabled = true;
            }
            else
            {
                mw.mlb_textBoxValue += "-------------------- \n";
                mw.mlb_textBoxValue += "One or more files are missing. Now beginning to generate all reports. \n";
                //mw.mlb_textBoxValue += "One or more files are missing. Click 'Start/Run' to generate all reports. \n";
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

            mw.mlb_textBoxValue += "-------------------- \n";
            mw.mlb_textBoxValue += "Now generating the via list report. \n";
            mw.mlb_textBoxValue += "Now generating the etch length report. \n";
            mw.mlb_textBoxValue += "Now generating the pin pair report -- This generation may take a while. \n";

            ProcessStart(commandsArr);

            mw.mlb_textBoxValue += "All reports are now being generated in the background. Please check the folder of your board file to ensure completion. \n";
        }

        public async void GeneratingStatus()
        {
            while (!pinPairFlag)
            {
                await Task.Delay(2000);
                CurrentReportStatus();
                mw.mlb_textBoxValue += "  --- Generating \n";
            }

            mw.mlb_textBoxValue += "*** All reports have been generated. Click on 'Analyze Board File' to move to the next step. \n";
            mw.mlb_analyzeBoard.IsEnabled = true;
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
