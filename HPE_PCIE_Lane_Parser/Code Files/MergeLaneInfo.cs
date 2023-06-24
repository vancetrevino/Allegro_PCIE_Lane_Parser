using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Allegro_PCIE_Lane_Parser.Class_Files;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Allegro_PCIE_Lane_Parser.Code_Files
{
    internal class MergeLaneInfo
    {
        private TextBlock boardTextBlock;

        public MergeLaneInfo(TextBlock boardOutputTextBlock)
        {
            boardTextBlock = boardOutputTextBlock;
        }

        List<LaneGroup> aLaneGroupsComplete = new List<LaneGroup>();
        List<LaneGroup> bLaneGroupsComplete = new List<LaneGroup>();
        List<LaneGroup> upiTxLaneGroupsComplete = new List<LaneGroup>();
        List<LaneGroup> upiRxLaneGroupsComplete = new List<LaneGroup>();
        List<LaneGroup> otherLaneGroupsComplete = new List<LaneGroup>();

        public void MergePcieLanes(List<DiffPairLane> pcieLanesInfo, Dictionary<string, int> pinPairIndexes, string[] mainConnRefDes, HashSet<string> connectorRefDesHash, string boardType)
        {
            List<LaneGroup> aSideLanesCpu0 = new List<LaneGroup>();
            List<LaneGroup> aSideLanesCpu1 = new List<LaneGroup>();

            List<LaneGroup> bSideLanesCpu0 = new List<LaneGroup>();
            List<LaneGroup> bSideLanesCpu1 = new List<LaneGroup>();

            foreach (var lane in pcieLanesInfo)
            {
                string connPinPair = "";
                string pinToCheck = "";

                // Checking if either the start/end pin pair contains the CPU ref des, or if two connectors are connected together. 
                // Redundancy checks are in place to get all connectors/devices that PCIE lanes (P5E*) are connected to. 
                if ((mainConnRefDes.Any(lane.PinPairStart.StartsWith)) || (mainConnRefDes.Any(lane.PinPairEnd.StartsWith)) || (lane.PinPairStart.Contains('J') && lane.PinPairEnd.Contains('J')))
                {
                    // Check for dual connector first, then check to find main connector (aka CPU or goldfinger) 
                    if (lane.PinPairStart.Contains('J') && lane.PinPairEnd.Contains('J'))
                    {
                        // Dual connector case needs to create two separate LaneGroup objects to sort. 
                        connPinPair = lane.PinPairStart;
                        pinToCheck = lane.PinPairStart;
                        ParseLaneIntoLaneGroupAndMerge(pcieLanesInfo, lane, pinPairIndexes, mainConnRefDes,
                                                ref aSideLanesCpu0, ref aSideLanesCpu1, ref bSideLanesCpu0, ref bSideLanesCpu1,
                                                boardType, connPinPair, pinToCheck);

                        connPinPair = lane.PinPairEnd;
                        pinToCheck = lane.PinPairEnd;
                    }
                    else if (mainConnRefDes.Any(lane.PinPairStart.StartsWith))
                    {
                        connPinPair = lane.PinPairEnd;
                        pinToCheck = lane.PinPairEnd;
                    }
                    else if (mainConnRefDes.Any(lane.PinPairEnd.StartsWith))
                    {
                        connPinPair = lane.PinPairStart;
                        pinToCheck = lane.PinPairStart;
                    }

                    // For all other cases, just create a single LaneGroup object to sort. 
                    ParseLaneIntoLaneGroupAndMerge(pcieLanesInfo, lane, pinPairIndexes, mainConnRefDes,
                                                ref aSideLanesCpu0, ref aSideLanesCpu1, ref bSideLanesCpu0, ref bSideLanesCpu1,
                                                boardType, connPinPair, pinToCheck);
                }
            }

            var aLanesInOrderCpu0 = aSideLanesCpu0.OrderBy(lanes => lanes.PinPair);
            var aLanesInOrderCpu1 = aSideLanesCpu1.OrderBy(lanes => lanes.PinPair);
            var bLanesInOrderCpu0 = bSideLanesCpu0.OrderBy(lanes => lanes.PinPair);
            var bLanesInOrderCpu1 = bSideLanesCpu1.OrderBy(lanes => lanes.PinPair);

            aLaneGroupsComplete.AddRange(aLanesInOrderCpu0);
            aLaneGroupsComplete.AddRange(aLanesInOrderCpu1);

            bLaneGroupsComplete.AddRange(bLanesInOrderCpu0);
            bLaneGroupsComplete.AddRange(bLanesInOrderCpu1);
        }

        public void ParseLaneIntoLaneGroupAndMerge(List<DiffPairLane> pcieLanesInfo, DiffPairLane pcieLane, Dictionary<string, int> pinPairIndexes, string[] mainConnRefDes,
                                                ref List<LaneGroup> aSideLanesCpu0, ref List<LaneGroup> aSideLanesCpu1, ref List<LaneGroup> bSideLanesCpu0, ref List<LaneGroup> bSideLanesCpu1,
                                                string boardType, string connPinPair, string pinToCheck)
        {
            string startingNet = "";
            Dictionary<string, string> startingLayersandLengths = new Dictionary<string, string>();
            string startingViaCount = "0";

            string endingNet = "";
            Dictionary<string, string> endingLayersandLengths = new Dictionary<string, string>();
            string endinggViaCount = "0";
            string? capPinToFind = checkPinForCap(pcieLane, pinToCheck);

            // Check if current net is connected to a capacitor
            if (capPinToFind != null)
            {
                if (pinPairIndexes.TryGetValue(capPinToFind, out int index))
                {
                    startingNet = pcieLane.NetName;
                    startingLayersandLengths = pcieLane.LayerAndLengths;
                    startingViaCount = pcieLane.ViaCount;

                    endingNet = pcieLanesInfo[index].NetName;
                    endingLayersandLengths = pcieLanesInfo[index].LayerAndLengths;
                    endinggViaCount = pcieLanesInfo[index].ViaCount;

                    if (pcieLanesInfo[index].PinPairStart.Contains("C"))
                    {
                        connPinPair = pcieLanesInfo[index].PinPairEnd;
                    }
                    else if (boardType != "riser")
                    {
                        connPinPair = pcieLanesInfo[index].PinPairStart;
                    }
                }
            }
            else
            {
                startingNet = pcieLane.NetName;
                startingLayersandLengths = pcieLane.LayerAndLengths;
                startingViaCount = pcieLane.ViaCount;
            }

            string connector = connPinPair.Split('.')[0];

            // Prepend extra 0 (zeros) to the pin pair A/B side of the connector.
            if (connPinPair.Contains('J'))
            {
                string connectorPin = connPinPair.Split('.')[1];

                //Regex re = new Regex(@"([A-B]+)(\d+)");
                Regex re = new Regex(@"(\w+?)(\d+)");
                //Regex re = new Regex(@"(.+?[A-B])(\d+)");
                Match result = re.Match(connectorPin);

                string connectorPinLetter = result.Groups[1].Value;
                string connectorPinNumber = result.Groups[2].Value;

                while (connectorPinNumber.Length < 5)
                {
                    connectorPinNumber = connectorPinNumber.PadLeft(connectorPinNumber.Length + 1, '0');
                }

                connPinPair = connector + "." + connectorPinLetter + connectorPinNumber;
            }

            LaneGroup completeLaneGroup = new LaneGroup(connector, startingNet, startingLayersandLengths, startingViaCount,
                                                        connPinPair, endingNet, endingLayersandLengths, endinggViaCount);

            completeLaneGroup.CalcTotalViaCount();

            // Sort each lane into its respective CPU identifier. If no CPU, then sort into CPU0 by default
            if (pcieLane.NetName.Contains("CPU1"))
            {
                SortLaneByConnectorSide(connPinPair, ref aSideLanesCpu1, ref bSideLanesCpu1, ref completeLaneGroup);
            }
            else
            {
                SortLaneByConnectorSide(connPinPair, ref aSideLanesCpu0, ref bSideLanesCpu0, ref completeLaneGroup);
            }
        }


        public void SortLaneByConnectorSide(string connPinPair, ref List<LaneGroup>  aSideLanes, ref List<LaneGroup>  bSideLanes, ref LaneGroup completeLaneGroup)
        {
            if ((connPinPair.Contains(".B_A") || connPinPair.Contains(".A")) && !(connPinPair.Contains(".A_B")))
            {
                aSideLanes.Add(completeLaneGroup);
            }
            else if ((connPinPair.Contains(".A_B") || connPinPair.Contains(".B")) && !(connPinPair.Contains(".B_A")))
            {
                bSideLanes.Add(completeLaneGroup);
            }
        }
        

        public void MergeOtherLanes(string groupType, List<DiffPairLane> otherLanesInfo)
        {
            List<LaneGroup> upiTx = new List<LaneGroup>();
            List<LaneGroup> upiRx = new List<LaneGroup>();
            List<LaneGroup> otherLanes = new List<LaneGroup>();

            foreach (var lane in otherLanesInfo)
            {
                LaneGroup completeLaneGroup = new LaneGroup(groupType, lane.NetName, lane.LayerAndLengths, lane.ViaCount, lane.FullPinPair);

                // If current lane's net name contains <##>, then split it and re-order the naming to allow for correct ordering
                if (lane.NetName.Contains("_DP<") || lane.NetName.Contains("_DN<"))
                {
                    string[] polaritySeparators = new String[] { "_DP<", "_DN<" };
                    char[] bracketSeparators = new char[] { '<', '>' };
                    string netNamePrefix = lane.NetName.Split(polaritySeparators, StringSplitOptions.RemoveEmptyEntries)[0];
                    string[] netSplit = lane.NetName.Split(bracketSeparators, StringSplitOptions.RemoveEmptyEntries);
                    string netPolarity = netSplit[0].Split('_').Last();
                    string netNumber = netSplit.Last();

                    while (netNumber.Length < 5)
                    {
                        netNumber = netNumber.PadLeft(netNumber.Length + 1, '0');
                    }

                    completeLaneGroup.NetToOrderBy = netNamePrefix + "_" + netNumber + netPolarity;
                }
                else
                {
                    completeLaneGroup.NetToOrderBy = lane.NetName;
                }

                completeLaneGroup.CalcTotalViaCount();

                // For UPI lane sorting
                if (groupType == "UPI")
                {
                    if (lane.NetName.Contains("CPU1_CPU0") || lane.NetName.Contains("_RT_") || lane.NetName.Contains("_RX_"))
                    {
                        upiRx.Add(completeLaneGroup);
                    }
                    else if (lane.NetName.Contains("CPU0_CPU1") || lane.NetName.Contains("_TR_") || lane.NetName.Contains("_TX_"))
                    {
                        upiTx.Add(completeLaneGroup);
                    }
                }
                // If not a UPI lane, then add to the other lane group List
                else
                {
                    otherLanes.Add(completeLaneGroup);
                }
            }

            if (upiTx.Count != 0 || upiRx.Count != 0)
            {
                var upiTxLanesInOrder = upiTx.OrderBy(upi => upi.NetToOrderBy);
                var upiRxLanesInOrder = upiRx.OrderBy(upi => upi.NetToOrderBy);

                upiTxLaneGroupsComplete.AddRange(upiTxLanesInOrder);
                upiRxLaneGroupsComplete.AddRange(upiRxLanesInOrder);
            }
            else if (otherLanes.Count != 0)
            {
                var otherLanesInOrder = otherLanes.OrderBy(lane => lane.NetToOrderBy);

                otherLaneGroupsComplete.AddRange(otherLanesInOrder);
            }
        }

        // Check to see if the current lane an inline capacitor or not. If so, then return it's other capacitor pin
        public string? checkPinForCap(DiffPairLane diffPair, string pinToCheck)
        {
            // Check either the pin pair for a capacitor or the netname for the standard capacitor notation
            if (pinToCheck.Contains("C") || diffPair.NetName.Contains("_C_"))
            {
                // Pin pair notates a capacitor, so find the end pin number of the cap and return the opposite number
                string pin = pinToCheck.Substring(0, pinToCheck.Length - 1);
                if (pinToCheck.EndsWith(".1"))
                {
                    return pin + "2";
                }
                else if (pinToCheck.EndsWith(".2"))
                {
                    return pin + "1";
                }
            }
            return null;
        }  

        public void CheckGroupsForMissingLayers(List<LaneGroup> laneGroup)
        {
            string? currentGroupName = "";
            var lastLane = laneGroup.Last();
            List<LaneGroup> tempLaneGroup = new List<LaneGroup>();
            HashSet<string> firstLayerHashSet = new HashSet<string>();
            HashSet<string> secondLayerHashSet = new HashSet<string>();

            for (var i = 0; i < laneGroup.Count() - 1; i++)
            {
                LaneGroup lane = laneGroup[i];
                LaneGroup nextLane = laneGroup[i + 1];

                tempLaneGroup.Add(lane);

                foreach (var layer in lane.FirstLayerAndLengths)
                {
                    firstLayerHashSet.Add(layer.Key);
                }

                foreach (var layer in lane.SecondLayerAndLengths)
                {
                    secondLayerHashSet.Add(layer.Key);
                }

                if (nextLane.GroupName != currentGroupName || nextLane == lastLane)
                {
                    // In the event that the next lane is the last lane in the sequence, add the last lane to the tempLaneGroup
                    if (nextLane == lastLane) { tempLaneGroup.Add(lastLane); }
                    InsertMissingLayersToGroup(tempLaneGroup, firstLayerHashSet, secondLayerHashSet);

                    currentGroupName = nextLane.GroupName;

                    // Do not clear on the very first iteration, and execute a clear on all the others
                    if (i != 0)
                    {
                        tempLaneGroup.Clear();
                        firstLayerHashSet.Clear();
                        secondLayerHashSet.Clear();
                    }
                }
            }
        }

        private void InsertMissingLayersToGroup(List<LaneGroup> tempLaneGroup, HashSet<string> firstLayerHashSet, HashSet<string> secondLayerHashSet)
        {
            foreach (var lane in tempLaneGroup)
            {
                foreach (var firstLayer in firstLayerHashSet)
                {
                    if (!lane.FirstLayerAndLengths.ContainsKey(firstLayer))
                    {
                        lane.FirstLayerAndLengths.Add(firstLayer, "0");
                    }
                }

                foreach (var secondLayer in secondLayerHashSet)
                {
                    if (!lane.SecondLayerAndLengths.ContainsKey(secondLayer))
                    {
                        lane.SecondLayerAndLengths.Add(secondLayer, "0");
                        
                        if (secondLayerHashSet.Count > 0 && lane.SecondNet == "")
                        {
                            lane.SecondNet = "-";
                        }
                    }
                }
            }
        }

        public List<LaneGroup> GetASideGroup()
        {
            return aLaneGroupsComplete;
        }
        
        public List<LaneGroup> GetBSideGroup()
        {
            return bLaneGroupsComplete;
        }

        public List<LaneGroup> GetUpiRxGroup()
        {
            return upiRxLaneGroupsComplete;
        }

        public List<LaneGroup> GetUpiTxGroup()
        {
            return upiTxLaneGroupsComplete;
        }

        public List<LaneGroup> GetOtherLaneGroup()
        {
            return otherLaneGroupsComplete;
        }
    }
}




//else if (connectorRefDesHash.Any(lane.PinPairStart.Contains))
//{
//    connPinPair = lane.PinPairEnd;
//    pinToCheck = lane.PinPairEnd;
//}
//else if (connectorRefDesHash.Any(lane.PinPairEnd.Contains))
//{
//    connPinPair = lane.PinPairStart;
//    pinToCheck = lane.PinPairStart;
//}

// If the board type is a riser, then override the connPinPair 
//if (boardType == "riser" && mainConnRefDes.Any(lane.PinPairStart.StartsWith))
//{
//    connPinPair = lane.PinPairStart;
//}
//else if (boardType == "riser" && mainConnRefDes.Any(lane.PinPairEnd.StartsWith))
//{
//    connPinPair = lane.PinPairEnd;
//}