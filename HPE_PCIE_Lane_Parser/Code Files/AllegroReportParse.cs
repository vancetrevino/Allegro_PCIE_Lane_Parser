using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allegro_PCIE_Lane_Parser.Code_Files
{
    internal class AllegroReportParse
    {
        private Dictionary<string, Dictionary<string, string>> netNameDict = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, string> componentPinPairDict = new Dictionary<string, string>();



        private string currentBrdPath { get; set; }

        public AllegroReportParse(string path)
        {
            currentBrdPath = path;
        }


        public void ParsePinPairReport()
        {

        }
    }
}
