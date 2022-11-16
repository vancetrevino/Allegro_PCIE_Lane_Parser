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
        private string mlb_directory { get; set; }
        private string mlb_file { get; set; }
        private List<LaneGroup> aLaneGroupComplete { get; set; }
        private List<LaneGroup> bLaneGroupComplete { get; set; }

        public PrintToCSV (string mlb_directory, string mlb_file, List<LaneGroup> aLaneGroupComplete, List<LaneGroup> bLaneGroupComplete)
        {
            this.mlb_directory = mlb_directory;
            this.mlb_file = mlb_file;
            this.aLaneGroupComplete = aLaneGroupComplete;
            this.bLaneGroupComplete = bLaneGroupComplete;
        }

        private string outputFile = @"\__OUTPUT__ParsedLanes.csv";
        private List<List<string>> outputList = new List<List<string>>();

        public void LaneGroupsOrdering()
        {
            string oldRefDes = "";

            for (var i = 0; i < aLaneGroupComplete.Count; i++)
            {
                List<string> tempList = new List<string>();
                List<string> headers = new List<string>();

                var lanesA = aLaneGroupComplete[i];
                var lanesB = bLaneGroupComplete[i];
                
                // Do a series of check to determine what needs to be written to the CSV file
                if (!string.IsNullOrEmpty(lanesA.GroupName))
                {
                    headers.Add("\n Ref Des,");
                    tempList.Add(lanesA.GroupName + ",");
                }

                ParseLaneGroups(tempList, headers, lanesA.SecondNet, lanesA.SecondLayerAndLengths);
                ParseLaneGroups(tempList, headers, lanesA.FirstNet, lanesA.FirstLayerAndLengths);
                ParseTotalViaOnLane(tempList, headers, lanesA.TotalViaCount);
                
                headers.Add(",");
                tempList.Add(",");

                ParseLaneGroups(tempList, headers, lanesB.SecondNet, lanesB.SecondLayerAndLengths);
                ParseLaneGroups(tempList, headers, lanesB.FirstNet, lanesB.FirstLayerAndLengths);
                ParseTotalViaOnLane(tempList, headers, lanesB.TotalViaCount);

                headers.Add("\n");
                tempList.Add("\n");

                if (oldRefDes != lanesA.GroupName)
                {
                    oldRefDes = lanesA.GroupName;
                    outputList.Add(headers);
                }

                outputList.Add(tempList);
            }
        }

        public void WriteToCSV()
        {
            using (var writer = new StreamWriter(mlb_directory + outputFile))
            {
                // Write the board file name to begin
                string boardFileNameHeader = "Board File:, " + mlb_file + ",";
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

        public void ParseLaneGroups(List<string> tempList, List<string> headers, string laneNet, Dictionary<string, string> laneLayerAndLengths)
        {
            if (!string.IsNullOrEmpty(laneNet))
            {
                headers.Add("Net Name,");
                tempList.Add(laneNet + ",");
            }

            if (laneLayerAndLengths.Count > 0)
            {
                Dictionary<string, string> layerLengthDict = laneLayerAndLengths;
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
            if (layerLengthDict.ContainsKey(layerName) || layerLengthDict.ContainsKey(layerName + "_SIG"))
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
            }
        }
    }
}
