using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Allegro_PCIE_Lane_Parser.Class_Files;

namespace Allegro_PCIE_Lane_Parser.Code_Files
{
    internal class PrintToCSV
    {
        private string boardDirectory { get; set; }
        private string boardFileName { get; set; }

        public PrintToCSV (string boardDirectory, string boardFileName)
        {
            this.boardDirectory = boardDirectory;
            this.boardFileName = boardFileName;
        }

        private List<List<string>> outputList = new List<List<string>>();

        public void BeginHeaderAndLists(List<string> tempList, List<string> headers, string connectorRefDesGroup)
        {
            // Start off CSV file with a header for each column and the connector REF DES to show the grouping. 
            if (!string.IsNullOrEmpty(connectorRefDesGroup))
            {
                headers.Add("\nRef Des,");
                tempList.Add(connectorRefDesGroup + ",");
            }
        }

        public void ChangeHeaderGroup(List<string> headers, string connectorRefDesGroup, ref string previousRefDes)
        {
            if (previousRefDes != connectorRefDesGroup.Split('.')[0])
            {
                previousRefDes = connectorRefDesGroup.Split('.')[0];
                outputList.Add(headers);
            }
        }

        // Method to begin ordering each lane grouping
        public void OtherLaneGroupsOrdering(List<LaneGroup> laneGroupComplete)
        {
            string previousRefDes = "";
            int maxLaneCount = laneGroupComplete.Count;

            for (var primaryIndex = 0; primaryIndex < maxLaneCount; primaryIndex++)
            {
                List<string> tempList = new List<string>();
                List<string> headers = new List<string>();

                string connectorRefDesGroup = laneGroupComplete[primaryIndex].GroupName;

                LaneGroup laneGroup = laneGroupComplete[primaryIndex];

                BeginHeaderAndLists(tempList, headers, connectorRefDesGroup);

                ParseLaneGroups(tempList, headers, laneGroup.SecondNet, laneGroup.SecondLayerAndLengths);
                ParseLaneGroups(tempList, headers, laneGroup.FirstNet, laneGroup.FirstLayerAndLengths);
                ParseTotalViaOnLane(tempList, headers, laneGroup.TotalViaCount);

                headers.Add(",");
                tempList.Add(",");

                headers.Add("\n");
                tempList.Add("\n");


                ChangeHeaderGroup(headers, connectorRefDesGroup, ref previousRefDes);
                outputList.Add(tempList);
            }
        }

        // Method to begin ordering each lane grouping
        public void DiffPairLaneGroupsOrdering(List<LaneGroup> aLaneGroupComplete, List<LaneGroup> bLaneGroupComplete)
        {
            string previousRefDes = "";
            int secondaryIndex = 0;
            int maxLaneCount = Math.Max(aLaneGroupComplete.Count, bLaneGroupComplete.Count);

            for (var primaryIndex = 0; primaryIndex < maxLaneCount; primaryIndex++)
            {
                List<string> tempList = new List<string>();
                List<string> headers = new List<string>();

                LaneGroup laneA = new LaneGroup();
                LaneGroup laneB = new LaneGroup();

                string connectorRefDesGroup = "";
                string fewerLanesFlag = "LANE_B";

                // Verify which List of lanes contains a larger count
                // This is to prevent the edge case if certain lanes aren't connected to their endpoint. 
                // Using primary and secondary indexes to move through each list.
                // Secondary index will only increase if all lane checks pass.
                if (aLaneGroupComplete.Count >= bLaneGroupComplete.Count)
                {
                    laneA = aLaneGroupComplete[primaryIndex];
                    laneB = bLaneGroupComplete[secondaryIndex];
                    connectorRefDesGroup = laneA.PinPair;
                    fewerLanesFlag = "LANE_B";
                }
                else
                {
                    laneA = bLaneGroupComplete[secondaryIndex];
                    laneB = aLaneGroupComplete[primaryIndex];
                    connectorRefDesGroup = laneB.PinPair;
                    fewerLanesFlag = "LANE_A";
                }

                if ((laneA.GroupName.Contains("UPI") || laneB.GroupName.Contains("UPI")))
                {
                    connectorRefDesGroup = "UPI";
                }
    

                    BeginHeaderAndLists(tempList, headers, connectorRefDesGroup);

                // Check to see if the A side and B side lanes match each other: net name and port-number wise.
                // AKA: Both are referring to the same lane and port number. --> P5E2A<0>_TX and P5E2A<0>_RX
                // Ignore for UPI lanes, as some UPI lanes are routed to different ports.
                CheckLaneEquality(tempList, headers, laneA, laneB, fewerLanesFlag, ref secondaryIndex);

                ChangeHeaderGroup(headers, connectorRefDesGroup, ref previousRefDes);
                outputList.Add(tempList);
            }
        }

        public void CheckLaneEquality(List<string> tempList, List<string> headers, LaneGroup laneA, LaneGroup laneB, string fewerLanesFlag, ref int secondaryIndex)
        {
            // Split each lane net name string by the TX and RX nomenclature, and use the first part of the string in the comparison. 
            string[] laneSeparators = new String[] { "RX", "TX", "RT", "TR" };
            string laneANetNameIdentifier = laneA.FirstNet.Split(laneSeparators, StringSplitOptions.RemoveEmptyEntries).First();
            string laneBNetNameIdentifier = laneB.FirstNet.Split(laneSeparators, StringSplitOptions.RemoveEmptyEntries).First();

            string laneANetNameNumber = laneA.FirstNet.Split('<').Last();
            string laneBNetNameNumber = laneB.FirstNet.Split('<').Last();

            string firstNetNameComplete = laneANetNameIdentifier + laneANetNameNumber;
            string secondNetNameComplete = laneBNetNameIdentifier + laneBNetNameNumber;

            LaneGroup tempLaneGroup = new LaneGroup();

            if ((firstNetNameComplete == secondNetNameComplete) || (laneA.FirstNet.Contains("UPI") || laneB.FirstNet.Contains("UPI")))
            {
                ChooseParsingLanes(tempList, headers, laneA, laneB);

                secondaryIndex += 1;
            }
            else if (firstNetNameComplete != secondNetNameComplete && fewerLanesFlag == "LANE_A")
            {
                tempLaneGroup.FirstNet = "Lane not found...";

                ChooseParsingLanes(tempList, headers, tempLaneGroup, laneB);
            }
            else if (firstNetNameComplete != secondNetNameComplete && fewerLanesFlag == "LANE_B")
            {
                tempLaneGroup.FirstNet = "Lane not found...";

                ChooseParsingLanes(tempList, headers, laneA, tempLaneGroup);
            }


            headers.Add("\n");
            tempList.Add("\n");
        }

        public void ChooseParsingLanes(List<string> tempList, List<string> headers, LaneGroup laneA, LaneGroup laneB)
        {
            ParseLaneGroups(tempList, headers, laneA.FirstNet, laneA.FirstLayerAndLengths);
            ParseLaneGroups(tempList, headers, laneA.SecondNet, laneA.SecondLayerAndLengths);
            ParseTotalViaOnLane(tempList, headers, laneA.TotalViaCount);

            headers.Add(",");
            tempList.Add(",");

            ParseLaneGroups(tempList, headers, laneB.FirstNet, laneB.FirstLayerAndLengths);
            ParseLaneGroups(tempList, headers, laneB.SecondNet, laneB.SecondLayerAndLengths);
            ParseTotalViaOnLane(tempList, headers, laneB.TotalViaCount);
        }

        public void ParseLaneGroups(List<string> tempList, List<string> headers, string laneNet, Dictionary<string, string> layerLengthDict)
        {
            if (!string.IsNullOrEmpty(laneNet))
            {
                headers.Add("Net Name,");
                tempList.Add(laneNet + ",");
            }
            else if (laneNet == "" && layerLengthDict.Count > 0)
            {
                tempList.Add(" " + ",");
            }
            else if (laneNet == "N/A")
            {
                tempList.Add("Lane not found... " + ",");
            }

            if (layerLengthDict.Count > 0)
            {
                //Dictionary<string, string> layerLengthDict = laneLayerAndLengths;
                // Check all possible layers to determine what is routed
                ParseLayersInDict(tempList, headers, layerLengthDict, "TOP");
                ParseLayersInDict(tempList, headers, layerLengthDict, "L1");
                ParseLayersInDict(tempList, headers, layerLengthDict, "L2");
                ParseLayersInDict(tempList, headers, layerLengthDict, "L3");
                ParseLayersInDict(tempList, headers, layerLengthDict, "L4");
                ParseLayersInDict(tempList, headers, layerLengthDict, "L5");
                ParseLayersInDict(tempList, headers, layerLengthDict, "L6");
                ParseLayersInDict(tempList, headers, layerLengthDict, "L7");
                ParseLayersInDict(tempList, headers, layerLengthDict, "L8");
                ParseLayersInDict(tempList, headers, layerLengthDict, "L9");
                ParseLayersInDict(tempList, headers, layerLengthDict, "L10");
                ParseLayersInDict(tempList, headers, layerLengthDict, "L11");
                ParseLayersInDict(tempList, headers, layerLengthDict, "L12");
                ParseLayersInDict(tempList, headers, layerLengthDict, "L13");
                ParseLayersInDict(tempList, headers, layerLengthDict, "L14");
                ParseLayersInDict(tempList, headers, layerLengthDict, "L15");
                ParseLayersInDict(tempList, headers, layerLengthDict, "L16");
                ParseLayersInDict(tempList, headers, layerLengthDict, "L17");
                ParseLayersInDict(tempList, headers, layerLengthDict, "L18");
                ParseLayersInDict(tempList, headers, layerLengthDict, "L19");
                ParseLayersInDict(tempList, headers, layerLengthDict, "L20");
                ParseLayersInDict(tempList, headers, layerLengthDict, "L21");
                ParseLayersInDict(tempList, headers, layerLengthDict, "L22");
                ParseLayersInDict(tempList, headers, layerLengthDict, "BOTTOM");
            }
        }

        public void ParseTotalViaOnLane(List<string> tempList, List<string> headers, string laneVia)
        {
            if (!string.IsNullOrEmpty(laneVia))
            {
                headers.Add("Total Vias,");
                tempList.Add(laneVia + ",");
            }
        }

        public void ParseLayersInDict(List<string> tempList, List<string> headers, Dictionary<string, string> layerLengthDict, string layerName)
        {
            if (layerLengthDict.ContainsKey(layerName) || layerLengthDict.ContainsKey(layerName + "_SIG") || layerLengthDict.ContainsKey("SIG_" + layerName))
            {
                string value = "";
                headers.Add(layerName + " Length (MILS),");
                if (layerLengthDict.TryGetValue(layerName, out value))
                {
                    tempList.Add(value + ",");
                }
                else if (layerLengthDict.TryGetValue(layerName + "_SIG", out value))
                {
                    tempList.Add(value + ",");
                }
                else if (layerLengthDict.TryGetValue("SIG_" + layerName, out value))
                {
                    tempList.Add(value + ",");
                }
                else
                {
                    tempList.Add("0,");
                }
            }
        }

        public void WriteToCSV()
        {
            string outputFile = @"\__OUTPUT__ParsedLanes_" + boardFileName + ".csv";
            using (var writer = new StreamWriter(boardDirectory + outputFile))
            {
                // Write the board file name to begin
                string boardFileNameHeader = "Board File:, " + boardFileName + ",";
                writer.Write(boardFileNameHeader);

                foreach (var row in outputList)
                {
                    foreach (var item in row)
                    {
                        writer.Write(item);
                    }
                }
            }
        }
    }
}
