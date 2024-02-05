using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allegro_PCIE_Lane_Parser.Class_Files
{
    internal class LaneGroup
    {
        public string GroupName { get; set; }
        public string FirstNet { get; set; }
        public Dictionary<string, string> FirstLayerAndLengths { get; set; }
        public string FirstViaCount { get; set; }
        public string PinPair { get; set; }
        public string SecondNet { get; set; }
        public Dictionary<string, string> SecondLayerAndLengths { get; set; }
        public string SecondViaCount { get; set; }
        public string TotalViaCount { get; set; }

        // String used to order by Net Name
        public string NetToOrderBy { get; set; }

        public LaneGroup(string groupName, string firstNet, Dictionary<string, string> firstLayerAndLengths, string firstViaCount, 
                        string pinPair, string secondNet, Dictionary<string, string> secondLayerAndLengths, string secondViaCount)
        {
            GroupName = groupName;
            FirstNet = firstNet;
            FirstLayerAndLengths = firstLayerAndLengths;
            FirstViaCount = firstViaCount;
            PinPair = pinPair;

            SecondNet = secondNet;
            SecondLayerAndLengths = secondLayerAndLengths;
            SecondViaCount = secondViaCount;
            TotalViaCount = "0";

            // This purely for UPI/CLK/USB net name ordering
            NetToOrderBy = "";
        }

        public LaneGroup(string groupName, string firstNet, Dictionary<string, string> firstLayerAndLengths, string firstViaCount, string pinPair)
        {
            GroupName = groupName;
            FirstNet = firstNet;
            FirstLayerAndLengths = firstLayerAndLengths;
            FirstViaCount = firstViaCount;
            PinPair = pinPair;

            SecondNet = "";
            SecondLayerAndLengths = new Dictionary<string, string>();
            SecondViaCount = "0";
            TotalViaCount = "0";

            NetToOrderBy = "";
        }

        public LaneGroup()
        {
            GroupName = ""; ;
            FirstNet = ""; ;
            FirstLayerAndLengths = new Dictionary<string, string>();
            FirstViaCount = ""; ;
            PinPair = ""; ;

            SecondNet = ""; ;
            SecondLayerAndLengths = new Dictionary<string, string>();
            SecondViaCount = "";
            TotalViaCount = "";

            NetToOrderBy = "";
        }

        public void CalcTotalViaCount()
        {
            int firstViaToInt = Int32.Parse(FirstViaCount);
            int secondViaToInt = Int32.Parse(SecondViaCount);

            TotalViaCount = (firstViaToInt + secondViaToInt).ToString();
        }
    }
}
