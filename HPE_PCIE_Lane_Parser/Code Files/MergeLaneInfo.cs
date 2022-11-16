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

                        if (connPinPair.Contains(".A") && lane.NetName.Contains("CPU0"))
                        {
                            aSideLanesCpu0.Add(completeLaneGroup);
                        }
                        else if (connPinPair.Contains(".A") && lane.NetName.Contains("CPU1"))
                        {
                            aSideLanesCpu1.Add(completeLaneGroup);
                        }
                        else if (connPinPair.Contains(".B") && lane.NetName.Contains("CPU0"))
                        {
                            bSideLanesCpu0.Add(completeLaneGroup);
                        }
                        else if (connPinPair.Contains(".B") && lane.NetName.Contains("CPU1"))
                        {
                            bSideLanesCpu1.Add(completeLaneGroup);
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
            foreach (var lane in otherLanesInfo)
            {

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

        public List<LaneGroup> GetASideGroup()
        {
            return aLaneGroupsComplete;
        }
        
        public List<LaneGroup> GetBSideGroup()
        {
            return bLaneGroupsComplete;
        }
    }
}
