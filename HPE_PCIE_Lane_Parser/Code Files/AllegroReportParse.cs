﻿
using Allegro_PCIE_Lane_Parser.Class_Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace Allegro_PCIE_Lane_Parser.Code_Files
{
    internal class AllegroReportParse
    {
        // Class specific variables & Constructor
        private string viaListReportLocation { get; set; }
        private string etchLenReportLocation { get; set; }
        private string pinPairReportLocation { get; set; }
        public AllegroReportParse(string viaListLocation, string etchLenLocation, string pinPairLocation)
        {
            viaListReportLocation = viaListLocation;
            etchLenReportLocation = etchLenLocation;
            pinPairReportLocation = pinPairLocation;
        }

        private string pcieNetIdentifier = "P5E";
        private string clockNetIdentifier = "CLK";
        private string upiNetIdentifier = "UPI";
        private string usbNetIdentifier = "USB";
        private string positiveDiffPair = "_DP";
        private string negativeDiffPair = "_DN";


        // Extra variables
        MainWindow mw = (MainWindow)Application.Current.MainWindow;

        // List data structures to hold all diff pair lane info
        private List<DiffPairLane> pcieLanesInfo = new List<DiffPairLane>();
        private List<DiffPairLane> clockLanesInfo = new List<DiffPairLane>();
        private List<DiffPairLane> upiLanesInfo = new List<DiffPairLane>();
        private List<DiffPairLane> usbLanesInfo = new List<DiffPairLane>();

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
                                pcieLanesInfo.Add(pcieDiffPair);
                            }

                            // Continue parse and check for other diff pairs (clk, usb, upi)
                            else if (netName.Contains(clockNetIdentifier))
                            {
                                DiffPairLane clockDiffPair = new DiffPairLane(netName, pinPair, totalLength);
                                var clockLane = clockLanesInfo.FirstOrDefault(obj => obj.NetName.Contains(netName), null);
                                if (clockLane == null)
                                {
                                    clockLanesInfo.Add(clockDiffPair);
                                }
                            }
                            else if (netName.Contains(upiNetIdentifier))
                            {
                                DiffPairLane upiDiffPair = new DiffPairLane(netName, pinPair, totalLength);
                                upiLanesInfo.Add(upiDiffPair);
                            }
                            else if (netName.Contains(usbNetIdentifier))
                            {
                                DiffPairLane usbDiffPair = new DiffPairLane(netName, pinPair, totalLength);
                                var usbLane = usbLanesInfo.FirstOrDefault(obj => obj.NetName.Contains(netName), null);
                                if (usbLane == null)
                                {
                                    usbLanesInfo.Add(usbDiffPair);
                                }
                            }
                        }
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
                                var pcieLane = pcieLanesInfo.FirstOrDefault(obj => obj.NetName.Contains(netName), null);
                                if (pcieLane != null)
                                {
                                    pcieLane.LayerAndLengths.Add(layer, length);
                                }
                            }

                            // Continue parse and check for other diff pairs (clk, usb, upi)
                            else if (netName.Contains(clockNetIdentifier))
                            {
                                var clockLane = clockLanesInfo.FirstOrDefault(obj => obj.NetName.Contains(netName), null);
                                if (clockLane != null)
                                {
                                    clockLane.LayerAndLengths.Add(layer, length);
                                }
                            }
                            else if (netName.Contains(upiNetIdentifier))
                            {
                                var upiLane = upiLanesInfo.FirstOrDefault(obj => obj.NetName.Contains(netName), null);
                                if (upiLane != null)
                                {
                                    upiLane.LayerAndLengths.Add(layer, length);
                                }
                            }
                            else if (netName.Contains(usbNetIdentifier))
                            {
                                var usbLane = usbLanesInfo.FirstOrDefault(obj => obj.NetName.Contains(netName), null);
                                if (usbLane != null)
                                {
                                    usbLane.LayerAndLengths.Add(layer, length);
                                }
                            }
                        }
                    }
                }
            }
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
                                var pcieLane = pcieLanesInfo.FirstOrDefault(obj => obj.NetName.Contains(netName), null);
                                if (pcieLane != null)
                                {
                                    pcieLane.ViaCount = vias;
                                }
                            }

                            // Continue parse and check for other diff pairs (clk, usb, etc)
                            else if (netName.Contains(clockNetIdentifier))
                            {
                                var clockLane = clockLanesInfo.FirstOrDefault(obj => obj.NetName.Contains(netName), null);
                                if (clockLane != null)
                                {
                                    clockLane.ViaCount = vias;
                                }
                            }
                            else if (netName.Contains(upiNetIdentifier))
                            {
                                var upiLane = upiLanesInfo.FirstOrDefault(obj => obj.NetName.Contains(netName), null);
                                if (upiLane != null)
                                {
                                    upiLane.ViaCount = vias;
                                }
                            }
                            else if (netName.Contains(usbNetIdentifier))
                            {
                                var usbLane = usbLanesInfo.FirstOrDefault(obj => obj.NetName.Contains(netName), null);
                                if (usbLane != null)
                                {
                                    usbLane.ViaCount = vias;
                                }
                            }
                        }
                    }
                }
            }
        }

        public Dictionary<string, int> GetPinPairIndex(List<DiffPairLane> diffPairLanes)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            for (var i = 0; i < diffPairLanes.Count; i++)
            {
                if (diffPairLanes[i].PinPairEnd.Contains("C") && !diffPairLanes[i].PinPairEnd.Contains("U") && !result.ContainsKey(diffPairLanes[i].PinPairEnd))
                {
                    result.Add(diffPairLanes[i].PinPairEnd, i);
                }
                else if (diffPairLanes[i].PinPairStart.Contains("C") && !diffPairLanes[i].PinPairStart.Contains("U") && !result.ContainsKey(diffPairLanes[i].PinPairStart))
                {
                    result.Add(diffPairLanes[i].PinPairStart, i);
                }
            }

            return result;
        }

        public string[] FindCpuRefDes(List<DiffPairLane> diffPairLanes)
        {
            Dictionary<string, int> RefDesInstancesCount = new Dictionary<string, int>();
            string[] CpuRefDes = new string[2];

            int highestRefDesCount = 0;
            int nextHighestRefDesCount = 0;

            for (var i = 0; i < diffPairLanes.Count; i++)
            {
                string firstRefDes = diffPairLanes[i].PinPairStart.Split(".")[0];
                string secondRefDes = diffPairLanes[i].PinPairEnd.Split(".")[0];

                if (firstRefDes.Contains('U') && !RefDesInstancesCount.ContainsKey(firstRefDes))
                {
                    RefDesInstancesCount.Add(firstRefDes, 1);
                }
                else if (secondRefDes.Contains('U') && !RefDesInstancesCount.ContainsKey(secondRefDes))
                {
                    RefDesInstancesCount.Add(secondRefDes, 1);
                }
                else if (RefDesInstancesCount.ContainsKey(firstRefDes))
                {
                    RefDesInstancesCount[firstRefDes] += 1;
                }
                else if (RefDesInstancesCount.ContainsKey(secondRefDes))
                {
                    RefDesInstancesCount[secondRefDes] += 1;
                }
            }

            foreach (var refDes in RefDesInstancesCount)
            {
                if (refDes.Value > nextHighestRefDesCount)
                {
                    nextHighestRefDesCount = refDes.Value;
                    CpuRefDes[0] = refDes.Key;
                }

                if (refDes.Value >= highestRefDesCount && refDes.Key != CpuRefDes[0])
                {
                    highestRefDesCount = refDes.Value;
                    CpuRefDes[1] = refDes.Key;
                }
            }

            return CpuRefDes;
        }

        public HashSet<string> FindConnectorsAttachedToCpu(List<DiffPairLane> diffPairLanes, string[] cpuRefDes)
        {
            HashSet<string> connectorsAttachedToCpu = new HashSet<string>();

            for (var i = 0; i < diffPairLanes.Count; i++)
            {
                string firstRefDes = diffPairLanes[i].PinPairStart.Split(".")[0];
                string secondRefDes = diffPairLanes[i].PinPairEnd.Split(".")[0];

                if (firstRefDes.Contains('J') && cpuRefDes.Any(secondRefDes.Contains))
                {
                    connectorsAttachedToCpu.Add(firstRefDes);
                }
                else if (secondRefDes.Contains('J') && cpuRefDes.Any(firstRefDes.Contains))
                {
                    connectorsAttachedToCpu.Add(secondRefDes);
                }
            }

            return connectorsAttachedToCpu;
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