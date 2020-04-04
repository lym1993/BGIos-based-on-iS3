using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settlement_calculation_of_PilleFoundation
{
    public class StaraInfoFinalView
    {
        //用来显示分层总和法下的计算步骤
        //以下为隐藏属性
        public int ID { get; set; }
        //土层厚度
        public string Thickness {get ;set; }
        //桩端距离
        public string ZOfBase { get; set; }
        //层底处自重应力
        public string GravityStress { get; set; }
        //层底处附加应力
        public string AdditionalStress { get; set; }
        //平均附加应力系数
        public string AverageAdditionalStressCoefficient { get; set; }
        //附加应力系数
        public string AdditionnalStressCoefficnt { get; set; }

        public string StratumID { get; set; }
        //i层土压缩模量Es、回弹压缩模量Ers
        public string Esi { get; set; }
        //第i层土附加应力系数沿土层厚度积分值
        public string Ai { get; set; }
        //该层压缩量
        public string SettlementOfSoil { get; set; }
        //总积分值
        public string TotalOfAi { get; set; }
        //积分除以压缩模量累加值
        public string TotalOfAiEsi { get; set; }
        //沉降累加值
        public string TotalOfSttlement { get; set; }
        //Name
        public string Name { get; set; }
    }
}
