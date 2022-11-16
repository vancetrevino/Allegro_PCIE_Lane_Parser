using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allegro_PCIE_Lane_Parser.Class_Files
{
    // Class to categorize all diff pair lanes
    internal class DiffPairLane
    {
        public string NetName { get; set; }
        public string FullPinPair { get; set; }
        public string PinPairStart { get; set; }
        public string PinPairEnd { get; set; }
        public string TotalLength { get; set; }
        public Dictionary<string, string> LayerAndLengths { get; set; }
        public string ViaCount { get; set; }

        public DiffPairLane(string netName, string fullPinPair, string totalLength)
        {
            NetName = netName;
            FullPinPair = fullPinPair;
            TotalLength = totalLength;

            PinPairStart = SplitPinPair()[0];
            PinPairEnd = SplitPinPair()[1];

            LayerAndLengths = new Dictionary<string, string>();
            ViaCount = "0";
        }

        public string[] SplitPinPair()
        {
            string[] splitPairs = FullPinPair.Split(":");

            return new string[] { splitPairs[0], splitPairs[1] };
        }

        public string FindNewConnectors()
        {
            string firstRefDes = PinPairStart.Split(".")[0];
            string secondRefDes = PinPairEnd.Split(".")[0];

            if (firstRefDes.Contains('J'))
            {
                return firstRefDes;
            }
            else if (secondRefDes.Contains('J'))
            {
                return secondRefDes;
            }
            else
            {
                return null;
            }
        }
    }
}
