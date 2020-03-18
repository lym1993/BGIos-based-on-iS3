using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settlement_calculation_of_PilleFoundation
{
    public class PileFoundationStrataInfoDto
    {
        public long OBJECTID { get; set; }

        public long PileFoundationID { get; set; }

        public double StratumID { get; set; }

        public double ElevationOfStratumBottom { get; set; }

        public double Gama { get; set; }

        public double Es0_100 { get; set; }

        public double Es100_200 { get; set; }

        public double Es200_300 { get; set; }

        public double Es300_400 { get; set; }

        public double Es400_500 { get; set; }

    }
}
