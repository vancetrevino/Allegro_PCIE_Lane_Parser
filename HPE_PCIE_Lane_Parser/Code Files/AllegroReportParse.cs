using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Allegro_PCIE_Lane_Parser.Code_Files
{
    internal class AllegroReportParse
    {
        // Dictionary data structures to hold all PCIE info
        private Dictionary<string, Dictionary<string, string>> netNameDict = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, string> connectorPinPairDict = new Dictionary<string, string>();
        private Dictionary<string, string> capacitorDict = new Dictionary<string, string>();

        // Other extra variables
        private int totalConnNum = 0;



        MainWindow mw = (MainWindow)Application.Current.MainWindow;


        // Class specific variables
        private string currentBrdPath { get; set; }
        private string viaListReportLocation { get; set; }
        private string etchLenReportLocation { get; set; }
        private string pinPairReportLocation { get; set; }

        public AllegroReportParse(string path, string viaListLocation, string etchLenLocation, string pinPairLocation)
        {
            currentBrdPath = path;
            viaListReportLocation = viaListLocation;
            etchLenReportLocation = etchLenLocation;
            pinPairReportLocation = pinPairLocation;
        }


        public void ParsePinPairReport()
        {
            // These CSV report files are comma (,) delimited
            using (var reader = new StreamReader(pinPairReportLocation))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    // Values should be split into a 3 bit array
                    // [Net name, Pin pair, Total Length]
                    var values = line.Split(',');
                }
            }
        }
    }
}