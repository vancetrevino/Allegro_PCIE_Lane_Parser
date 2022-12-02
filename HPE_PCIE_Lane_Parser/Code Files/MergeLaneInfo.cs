using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Allegro_PCIE_Lane_Parser.Class_Files;

namespace Allegro_PCIE_Lane_Parser.Code_Files
{
    internal class MergeLaneInfo
    {
        private HashSet<string> connectorRefDes { get; set; }

        public MergeLaneInfo(HashSet<string> connector)
        {
            connectorRefDes = connector;
        }

        List<LaneGroup> aLaneGroupsComplete = new List<LaneGroup>();
        List<LaneGroup> bLaneGroupsComplete = new List<LaneGroup>();
        List<LaneGroup> upiTxLaneGroupsComplete = new List<LaneGroup>();
        List<LaneGroup> upiRxLaneGroupsComplete = new List<LaneGroup>();
        List<LaneGroup> otherLaneGroupsComplete = new List<LaneGroup>();

        public void MergePcieLanes(List<DiffPairLane> pcieLanesInfo, Dictionary<string, int> pinPairIndexes)
        {
            List<LaneGroup> aSideLanesCpu0 = new List<LaneGroup>();
            List<LaneGroup> aSideLanesCpu1 = new List<LaneGroup>();

            List<LaneGroup> bSideLanesCpu0 = new List<LaneGroup>();
            List<LaneGroup> bSideLanesCpu1 = new List<LaneGroup>();

            foreach (var connector in connectorRefDes)
            {
                foreach (var lane in pcieLanesInfo)
                {
                    string connPinPair = "";
                    string pinToCheck = "";

                    if (lane.FullPinPair.Contains(connector))
                    {
                        // Get which pin pair (start or end) of the lane should be stored in the new LaneGroup object
                        if (lane.PinPairStart.Contains(connector))
                        {
                            connPinPair = lane.PinPairStart;
                            pinToCheck = lane.PinPairEnd;
                        }
                        else if (lane.PinPairEnd.Contains(connector))
                        {
                            connPinPair = lane.PinPairEnd;
                            pinToCheck = lane.PinPairStart;
                        }

                        LaneGroup completeLaneGroup = new LaneGroup(connector, lane.NetName, lane.LayerAndLengths, lane.ViaCount, connPinPair);
                        string? capPinToFind = checkPinForCap(lane, pinToCheck);

                        // Check if current net is connected to a capacitor
                        if (capPinToFind != null)
                        {
                            if (pinPairIndexes.TryGetValue(capPinToFind, out int index))
                            {
                                completeLaneGroup.SecondNet = pcieLanesInfo[index].NetName;
                                completeLaneGroup.SecondLayerAndLengths = pcieLanesInfo[index].LayerAndLengths;
                                completeLaneGroup.SecondViaCount = pcieLanesInfo[index].ViaCount;
                            }
                        }

                        completeLaneGroup.CalcTotalViaCount();


                        if (lane.NetName.Contains("CPU0"))
                        {
                            if ((connPinPair.Contains(".B_A") || connPinPair.Contains(".A")) && !(connPinPair.Contains(".A_B")))
                            {
                                aSideLanesCpu0.Add(completeLaneGroup);
                            }
                            else if ((connPinPair.Contains(".A_B") || connPinPair.Contains(".B")) && !(connPinPair.Contains(".B_A")))
                            {
                                bSideLanesCpu0.Add(completeLaneGroup);
                            }
                        }
                        else if (lane.NetName.Contains("CPU1"))
                        {
                            if ((connPinPair.Contains(".B_A") || connPinPair.Contains(".A")) && !(connPinPair.Contains(".A_B")))
                            {
                                aSideLanesCpu1.Add(completeLaneGroup);
                            }
                            else if ((connPinPair.Contains(".A_B") || connPinPair.Contains(".B")) && !(connPinPair.Contains(".B_A")))
                            {
                                bSideLanesCpu1.Add(completeLaneGroup);
                            }
                        }
                    }
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
                    if (lane.NetName.Contains("CPU1_CPU0") || lane.NetName.Contains("_RT_"))
                    {
                        upiRx.Add(completeLaneGroup);
                    }
                    else if (lane.NetName.Contains("CPU0_CPU1") || lane.NetName.Contains("_TR_"))
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

        // Method to check if the other pin pair of the lane is a capacitor or not. If so, then return it's other capacitor pin
        public string? checkPinForCap(DiffPairLane diffPair, string pinToCheck)
        {
            if (pinToCheck.Contains("C"))
            {
                // Pin pair notates a capacitor, so find the end pin number of the cap and return the opposite number
                string pin = pinToCheck.Substring(0, pinToCheck.Length - 1);
                if (pinToCheck.EndsWith("1"))
                {
                    return pin + "2";
                }
                else if (pinToCheck.EndsWith("2"))
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
            List<LaneGroup> tempGroup = new List<LaneGroup>();
            HashSet<string> firstLayerHashSet = new HashSet<string>();
            HashSet<string> secondLayerHashSet = new HashSet<string>();

            for (var i = 0; i < laneGroup.Count() - 1; i++)
            {
                LaneGroup lane = laneGroup[i];
                LaneGroup nextLane = laneGroup[i + 1];

                tempGroup.Add(lane);

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
                    // In the event that the next lane is the last lane in the sequence, add the last lane to the tempGroup
                    if (nextLane == lastLane) { tempGroup.Add(lastLane); }
                    InsertMissingLayersToGroup(tempGroup, firstLayerHashSet, secondLayerHashSet);

                    currentGroupName = nextLane.GroupName;

                    // Ignore the very first iteration, and execute on all the others
                    if (i != 0)
                    {
                        tempGroup.Clear();
                        firstLayerHashSet.Clear();
                        secondLayerHashSet.Clear();
                    }
                }
            }
        }

        private void InsertMissingLayersToGroup(List<LaneGroup> tempGroup, HashSet<string> firstLayerHashSet, HashSet<string> secondLayerHashSet)
        {
            foreach (var temp in tempGroup)
            {
                foreach (var first in firstLayerHashSet)
                {
                    if (!temp.FirstLayerAndLengths.ContainsKey(first))
                    {
                        temp.FirstLayerAndLengths.Add(first, "0");
                    }
                }

                foreach (var second in secondLayerHashSet)
                {
                    if (!temp.SecondLayerAndLengths.ContainsKey(second))
                    {
                        temp.SecondLayerAndLengths.Add(second, "0");
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