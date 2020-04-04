using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settlement_calculation_of_PilleFoundation
{
    public class PileFoundationFinalView
    {
        //桩基础沉降结果最终显示类
        //都是String类型，保留小数点后三位
        public int ID { get; set; }
        //上部荷载
        public string Load { get; set; }

        //桩基沉降等效系数
        public string PosiE { get; set; }

        //沉降经验系数
        public string Posi { get; set; }

        //分层总和法计算的沉降量
        public string FirstSettlement { get; set; }

        //该桩基最终沉降量
        public string FinalSettlement { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

    }
}
