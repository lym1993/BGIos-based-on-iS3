using iS3.Geology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settlement_calculation_of_PilleFoundation
{
    //该项目用来计算桩基础沉降，需要用到access数据库中的两个表
    //dbo_PileFoundation 和 dbo_PileFoundationStrataInfo
    //点击沉降计算按钮后首先读取database文件，赋值到桩基计算类中，之后开始进行计算
    //定义PileFoundationCalculate类，继承PileFoundation类
    public class PileFoundationCalculate:PileFoundation
    {
        public double distancefromxx { get; set; }

    }
}
