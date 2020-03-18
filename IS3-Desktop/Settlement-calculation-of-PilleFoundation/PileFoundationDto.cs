using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settlement_calculation_of_PilleFoundation
{
    public class PileFoundationDto
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public double L { get; set; }

        public double R { get; set; }

        public string B { get; set; }

        public double Xcoordinate { get; set; }

        public string Ycoordinate { get; set; }

        public double TopOfCushionCap { get; set; }

        public double PileLength { get; set; }

        public double TopOfPile { get; set; }

        public double BaseOfPile { get; set; }

        public double DiameterOfPile { get; set; }

        public double NumberOfPile { get; set; }

        public double DistanceOfPile { get; set; }

        public double PilesOfB { get; set; }

        public List<PileFoundationStrataInfoDto> PileFoundationStrataInfos { get; set; } = new List<PileFoundationStrataInfoDto>();
    }
}
