using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Allegro_PCIE_Lane_Parser.Class_Files;


namespace Allegro_PCIE_Lane_Parser.Code_Files
{
    internal class AllegroReportParse
    {
        private string pcieNetIdentifier = "P5E";
        private string clockNetIdentifier = "CLK";
        private string upiNetIdentifier = "UPI";
        private string usbNetIdentifier = "USB";
        private string positiveDiffPair = "_DP";
        private string negativeDiffPair = "_DN";


        // Extra variables
        private int totalConnNum = 0;
        private int totalRoutingLayers = 0;

        private HashSet<string> connectorRefDes = new HashSet<string>();
        private HashSet<string> routingLayers = new HashSet<string>();

        MainWindow mw = (MainWindow)Application.Current.MainWindow;

        // List data structures to hold all diff pair lane info
        private List<DiffPairLane> pcieLanesInfo = new List<DiffPairLane>();
        private List<DiffPairLane> clockLanesInfo = new List<DiffPairLane>();
        private List<DiffPairLane> upiLanesInfo = new List<DiffPairLane>();
        private List<DiffPairLane> usbLanesInfo = new List<DiffPairLane>();

        // Class specific variables
        private string viaListReportLocation { get; set; }
        private string etchLenReportLocation { get; set; }
        private string pinPairReportLocation { get; set; }

        public AllegroReportParse(string viaListLocation, string etchLenLocation, string pinPairLocation)
        {
            viaListReportLocation = viaListLocation;
            etchLenReportLocation = etchLenLocation;
            pinPairReportLocation = pinPairLocation;
        }

        // Method to read in the pin pair report and organize diff pairs in the separate lists
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

                    if (values.Count() > 2)
                    {
                        string netName = values[0];
                        string pinPair = values[1];
                        string totalLength = values[2];

                        if (netName.Contains(positiveDiffPair) || netName.Contains(negativeDiffPair))
                        {
                            // Check for PCIE lane diff pair and save it into list
                            if (netName.Contains(pcieNetIdentifier))
                            {
                                DiffPairLane pcieDiffPair = new DiffPairLane(netName, pinPair, totalLength);
                                string pcieRefDes = pcieDiffPair.FindNewConnectors();

                                if (pcieRefDes != null)
                                {
                                    connectorRefDes.Add(pcieRefDes);
                                    totalConnNum = connectorRefDes.Count;
                                }

                                pcieLanesInfo.Add(pcieDiffPair);
                            }
                        }

                        // Continue parse and check for other diff pairs (clk, usb, etc)
                        // else if ()
                    }
                }
            }
        }

        public void ParseLengthByLayerReport()
        {
            // These CSV report files are comma (,) delimited
            using (var reader = new StreamReader(etchLenReportLocation))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    // Values should be split into a 3 bit array
                    // [Net name, Pin pair, Total Length]
                    var values = line.Split(',');

                    if (values.Count() > 2)
                    {
                        string netName = values[0];
                        string layer = values[1];
                        string length = values[2];


                        if (netName.Contains(positiveDiffPair) || netName.Contains(negativeDiffPair))
                        {
                            // Check for PCIE lane diff pair and save it into list
                            if (netName.Contains(pcieNetIdentifier))
                            {
                                var lane = pcieLanesInfo.First((obj => obj.NetName.Contains(netName)));
                                if (lane != null)
                                {
                                    lane.LayerAndLengths.Add(layer, length);
                                    routingLayers.Add(layer);
                                    totalRoutingLayers = routingLayers.Count;
                                }
                            }
                        }

                        // Continue parse and check for other diff pairs (clk, usb, etc)
                        // else if ()
                    }
                }
            }

            //foreach (var item in pcieLanesInfo)
            //{
            //    Trace.WriteLine(item.NetName);

            //    foreach (var layer in item.LayerAndLengths)
            //    {
            //        Trace.WriteLine("  --  " + layer);
            //    }
            //}
        }



        public void ParseViaReport()
        {
            // These CSV report files are comma (,) delimited
            using (var reader = new StreamReader(viaListReportLocation))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    // Values should be split into a 3 bit array
                    // [Net name, Pin pair, Total Length]
                    var values = line.Split(',');

                    if (values.Count() > 2)
                    {
                        string netName = values[0];
                        string vias = values[1];


                        if (netName.Contains(positiveDiffPair) || netName.Contains(negativeDiffPair))
                        {
                            // Check for PCIE lane diff pair and save it into list
                            if (netName.Contains(pcieNetIdentifier))
                            {
                                var lane = pcieLanesInfo.First((obj => obj.NetName.Contains(netName)));
                                if (lane != null)
                                {
                                    lane.ViaCount = vias;
                                }
                            }
                        }

                        // Continue parse and check for other diff pairs (clk, usb, etc)
                        // else if ()
                    }
                }
            }
        }


        public List<DiffPairLane> GetPcieLaneInfo()
        {
            return pcieLanesInfo;
        }

        public List<DiffPairLane> GetClockLaneInfo()
        {
            return clockLanesInfo;
        }

        public List<DiffPairLane> GetUpiLaneInfo()
        {
            return upiLanesInfo;
        }

        public List<DiffPairLane> GetUsbLaneInfo()
        {
            return usbLanesInfo;
        }
    }
}