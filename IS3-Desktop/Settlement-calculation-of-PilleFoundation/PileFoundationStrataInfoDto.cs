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

        //回弹再压缩模量
        public double Ers { get; set; }

        //以下为隐藏属性
        //土层厚度
        public double Thickness { get; set; }
        //桩端距离
        public double ZOfBase { get; set; }
        //层底处自重应力
        public double GravityStress { get; set; }
        //层底处附加应力
        public double AdditionalStress { get; set; }
        //平均附加应力系数
        public double AverageAdditionalStressCoefficient { get; set; }
        //附加应力系数
        public double AdditionnalStressCoefficnt { get; set; }
        
        //i层土压缩模量Es、回弹压缩模量Ers
        public double Esi { get; set; }
        //第i层土附加应力系数沿土层厚度积分值
        public double Ai { get; set; }
        //该层压缩量
        public double SettlementOfSoil { get; set; }
        //总积分值
        public double TotalOfAi { get; set; }
        //积分除以压缩模量累加值
        public double TotalOfAiEsi { get; set; }
        //沉降累加值
        public double TotalOfSttlement { get; set; }
    }
}
