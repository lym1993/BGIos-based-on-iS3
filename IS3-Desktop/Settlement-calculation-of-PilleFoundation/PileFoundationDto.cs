using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settlement_calculation_of_PilleFoundation
{
    public class PileFoundationDto
    {
        //桩基础ID
        public  int ID { get; set; }
        //上部荷载
        public double Load { get; set; }

        //桩基沉降等效系数
        public double PosiE { get; set; }
        
        //分层总和法计算的沉降量
        public double FirstSettlement { get; set; }

        //该桩基沉降量
        public double FinalSettlement { get; set; }
        
        public string Name { get; set; }

        public string Type { get; set; }

        public double L { get; set; }

        public double R { get; set; }

        public double B { get; set; }

        public double Xcoordinate { get; set; }

        public double Ycoordinate { get; set; }

        public double TopOfCushionCap { get; set; }

        public double PileLength { get; set; }

        public double TopOfPile { get; set; }

        public double BaseOfPile { get; set; }

        public double DiameterOfPile { get; set; }

        public double NumberOfPile { get; set; }

        public double DistanceOfPile { get; set; }

        public double PilesOfB { get; set; }

        public List<PileFoundationStrataInfoDto> PileFoundationStrataInfos { get; set; } = new List<PileFoundationStrataInfoDto>();


        #region
        //计算矩形桩基础PosiE的值
        //public double GetPosiE()
        //{
        //    double _posiE;
        //    double _C0;
        //    double _C1;
        //    double _C2;

        //    //矩形承台短边桩数量
        //    double nb = Math.Pow((this.NumberOfPile*B/L), 0.5);

        //    //距径比C0、长径比C1、基础长款比C2

        //    return _posiE;

        //}
        #endregion

        //以下为计算方法

    }
}
