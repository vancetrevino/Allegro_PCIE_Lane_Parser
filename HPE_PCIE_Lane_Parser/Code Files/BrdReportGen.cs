using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;

namespace Allegro_PCIE_Lane_Parser
{
    internal class BrdReportGen
    {
        private string pinPairReport = "PinPairReport.csv";
        private string viaReport = "ViaByNetReport.csv";
        private string etchLengthReport = "EtchLengthByLayerReport.csv";

        MainWindow mw = (MainWindow)Application.Current.MainWindow;

        private string currentBrdPath { get; set; }
        private string cadenceToolsPath { get; set; }

        public BrdReportGen(string path, string toolsPath)
        {
            currentBrdPath = path;
            cadenceToolsPath = toolsPath;
        }


        public bool SearchForAllReports()
        {
            bool pinPairFlag = File.Exists(currentBrdPath + @"\" + pinPairReport);
            bool viaFlag = File.Exists(currentBrdPath + @"\" + viaReport); ;
            bool etchLengthFlag = File.Exists(currentBrdPath + @"\" + etchLengthReport); ;
            
            mw.mlb_textBoxValue = "Now searching for the following Allegro generated reports (Pin pair, Via list, Etch length) \n";

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
                mw.mlb_textBoxValue += "All Allegro generated reports have been found. Now moving to the next step. \n";
            }
            else
            {
                mw.mlb_textBoxValue += "One or more files are missing. Now generating all reports. \n";
            }

            return (pinPairFlag && viaFlag && etchLengthFlag);
        }


        public void GenerateAllReports(string brdFile)
        {
            string reportCmd = $@"{cadenceToolsPath}\report.exe";
            string viaListStr = $@"{currentBrdPath}\ViaByNetReport.csv";
            string etchLenStr = $@"{currentBrdPath}\EtchLengthByLayerReport.csv";
            string pinPairStr = $@"{currentBrdPath}\PinPairReport.csv";

            string[] commandsArr =
            {
                $@"{reportCmd} -v vialist_net ""{brdFile}"" ""{viaListStr}""",
                $@"{reportCmd} -v ell ""{brdFile}"" ""{etchLenStr}""",
                $@"{reportCmd} -v elp ""{brdFile}"" ""{pinPairStr}""",
            };

            mw.mlb_textBoxValue += "-------------------- \n";
            mw.mlb_textBoxValue += "Now generating the via list report. \n";
            mw.mlb_textBoxValue += "Now generating the etch length report. \n";
            mw.mlb_textBoxValue += "Now generating the pin pair report -- This generation may take a while. \n";
            
            ExecuteCommand(commandsArr);

            mw.mlb_textBoxValue += "All reports are now being generated in the background. Please check the folder of your board file to ensure completion. \n";
        }


        private void ExecuteCommand(string[] Commands)
        {
            ProcessStartInfo ProcessInfo;
            Process Process;

            ProcessInfo = new ProcessStartInfo("cmd.exe", $"/K {Commands[0]}&{Commands[1]}&{Commands[2]}");

            ProcessInfo.CreateNoWindow = true;
            ProcessInfo.UseShellExecute = false;

            Process = Process.Start(ProcessInfo);
        }
    }
}
