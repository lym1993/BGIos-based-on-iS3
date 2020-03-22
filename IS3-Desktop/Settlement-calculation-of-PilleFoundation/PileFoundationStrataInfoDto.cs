using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settlement_calculation_of_PilleFoundation
{
    public class PileFoundationStrataInfoDto
    {
        //录入属性
        //新增土层是否含水词条
        public bool IsHasWater { get; set; }

        public long OBJECTID { get; set; }

        public long PileFoundationID { get; set; }

        //200322 地层由double更换为string
        public string StratumID { get; set; }

        public double ElevationOfStratumBottom { get; set; }

        public double Gama { get; set; }

        public double Es0_100 { get; set; }

        public double Es100_200 { get; set; }

        public double Es200_300 { get; set; }

        public double Es300_400 { get; set; }

        public double Es400_500 { get; set; }

        //以下为隐藏属性
        //土层厚度
        public double Thickness { get; set; }
        //桩端距离
        public double ZOfBase { get; set; }
        //层顶自重应力
        public double GravityStress { get; set; }
        //层顶附加应力
        public double AdditionStress { get; set; }
        //附加应力系数
        public double AdditionalStressCoefficient { get; set; }
        //该层总应力
        public double TotalStress { get; set; }
        //压缩模量Es
        public double CompressionModulus { get; set; }
        //该层压缩量
        public double SettlementOfSoil { get; set; }


    }
}
