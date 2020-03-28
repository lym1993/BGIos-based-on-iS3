using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using iS3.Core;
using iS3.Core.Serialization;
using iS3.Geology;
using System.IO;
using Settlement_calculation_of_PilleFoundation;
using System.Collections.ObjectModel;

namespace iS3.Desktop
{
    /// <summary>
    /// Settlement_calculation_page1.xaml 的交互逻辑
    /// </summary>
    public partial class Settlement_calculation_page1 : Window
    {
        //200312读取ReadPileFoundation后的objs
        //先声明一个全局变量，用于传出数据
        

        public Settlement_calculation_page1()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            //PileFoundation pf1 = new PileFoundation();
            //pf1.PileFoundationCalculate();
            
            #region 使用Dapper扩展方法
            //设置路径
            string pt = "Data\\Z14\\Z14.mdb";
           // string pt = "Data\\Z14\\0327.mdb";
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pt);
            using (var reader = new PileFoundationReader(path))
            {
                //读取桩基础信息
                var foundations = reader.GetPileFoundations();
                //按Name名称排序
                foundations.OrderBy(x => x.ID);
                MessageBox.Show($"共读取表内桩基础数量: {foundations.Count()}个，即将开始进行沉降计算","计算提示");

                //怎么绑定datagrid呢
                //ObservableCollection<PileFoundationDto> pfd = foundations;

                //20200322将录入结果绑定在datagrid1上面
                //GeneralInformation.ItemsSource = foundations;
              
                //建筑物地平面标高
                double HorizontalPlane =38.25;

                //新建结果集合，用来显示桩基础计算结果
                var pilefoundationfinal = new List<PileFoundationDto>();

                //遍历集合，开始计算沉降
                foreach (var pfdto in foundations)
                {
                    //测试代码
                    //if (pfdto.Name != "CZ-1")
                       // continue;
                    //按地层排序
                    //以地层标高降序排列
                    pfdto.PileFoundationStrataInfos.OrderByDescending(x => x.ElevationOfStratumBottom);
                    
                    //地平面以下，桩顶以上原土层自重应力
                    double Upper = 0;
                    for (int i=0;i<pfdto.PileFoundationStrataInfos.Count;i++)
                    {
                        //临时变量
                        double _upper=0;
                        var temp1 = pfdto.PileFoundationStrataInfos[i];
                        //如果为首层，i-1为空
                        var temp0 =i==0?null: pfdto.PileFoundationStrataInfos[i - 1];
                        //判断是否含水
                        double _gama = temp1.IsHasWater ? temp1.Gama - 10 : temp1.Gama;

                        //如果桩顶标高小于等于层底标高，计算该层土重

                        //如果是第一层土，地平面减去第一层底
                        //由于是深基础，桩顶标高必小于天然土层底标高
                        if(i==0)
                        {
                            _upper = _gama * (HorizontalPlane - temp1.ElevationOfStratumBottom);
                        }
                        //上部土中间段，上层底减去该层底
                        //1底标高大于等于桩顶标高且大于上层底标高
                        //20200327是否要改为承台顶标高
                        else if (temp1.ElevationOfStratumBottom>=pfdto.TopOfPile)
                        {
                            _upper=_gama*(temp0.ElevationOfStratumBottom-temp1.ElevationOfStratumBottom);
                        }
                        //上部土桩顶处土层，上层底减去桩顶
                        else if(pfdto.TopOfPile>=temp1.ElevationOfStratumBottom&&pfdto.TopOfPile<temp0.ElevationOfStratumBottom)
                        {
                            _upper = _gama * (temp0.ElevationOfStratumBottom - pfdto.TopOfPile);
                            //剩下土层不用计算了
                            Upper += _upper;
                            break;
                        }
                        //累加得到Upper
                        Upper += _upper;
                    }
                    //MessageBox.Show($"被挖掉的土自重为{Upper}KPa");

                    //计算桩顶至桩底土的自重应力
                    double Middle = 0;
                    for (int i = 0; i < pfdto.PileFoundationStrataInfos.Count; i++)
                    {
                        //临时变量
                        double _middle = 0;
                        var temp1 = pfdto.PileFoundationStrataInfos[i];
                        //如果是首层i-1就为空
                        var temp0 =i==0?null:pfdto.PileFoundationStrataInfos[i - 1];

                        //判断是否含水
                        double _gama = temp1.IsHasWater ? temp1.Gama - 10 : temp1.Gama;

                        //如果桩顶标高小于层底标高，继续遍历
                        if (pfdto.TopOfPile < temp1.ElevationOfStratumBottom)
                            continue;

                        //首先分割首层，桩顶标高在该层与上层之间
                        else if (pfdto.TopOfPile >= temp1.ElevationOfStratumBottom &&
                            pfdto.TopOfPile < temp0.ElevationOfStratumBottom)
                        {
                            Middle = _gama * (pfdto.TopOfPile - temp1.ElevationOfStratumBottom);
                            //跳出本次循环
                            continue;
                        }

                        //中间层，叠加即可
                        else if (temp1.ElevationOfStratumBottom > pfdto.BaseOfPile &&
                            temp0.ElevationOfStratumBottom < pfdto.TopOfPile)
                        {
                            _middle = _gama * (temp0.ElevationOfStratumBottom - temp1.ElevationOfStratumBottom);
                        }
                        //桩底所在层，得到最终结果
                        //桩底标高大于该层底，小于上层底
                        else if (pfdto.BaseOfPile >=temp1.ElevationOfStratumBottom &&
                            pfdto.BaseOfPile < temp0.ElevationOfStratumBottom)
                        {
                            _middle = _gama * (temp0.ElevationOfStratumBottom - pfdto.BaseOfPile);

                            //只算一次即可，结束循环
                            Middle += _middle;
                            break;
                        }
                        //累加赋值
                        Middle += _middle;
                    }

                    //MessageBox.Show($"中间自重应力计算完成，数值为{Middle}KPa");

                    //基底处某点的平均附加应力单位KN/M2
                    //等于荷载加上承台重减去原Upper土重
                    double P0 = pfdto.Load + 25 * (pfdto.TopOfCushionCap - pfdto.TopOfPile) - Upper;

                    

                    //基底处土的自重应力
                    double Lower = Middle;
                    
                    //分层总和法计算土层压缩量
                    //新建列表，计算下层土的压缩量
                    var lowersoilcal = pfdto.PileFoundationStrataInfos;
                    //移除桩底以上土层
                    for (int i=0;i<lowersoilcal.Count;i++)
                    {
                        //层底标高大于等于桩底标高的土层移除
                        if (lowersoilcal[i].ElevationOfStratumBottom >= pfdto.BaseOfPile)
                        {
                            lowersoilcal.Remove(lowersoilcal[i]);
                            //小心测试时候减没了！！
                            --i;
                        }
                    }
                    //MessageBox.Show("移除土层完成");
                    //自然土厚度分层
                    for (int i=0; i<lowersoilcal.Count; i++)
                    {
                        
                        var temp1 = lowersoilcal[i];
                        var temp0 = i == 0 ? null : lowersoilcal[i - 1];
                        //天然地层厚度划分
                        //如果是首层桩底减去层底，非首层桩底减去上层底
                        temp1.Thickness = i == 0 ? pfdto.BaseOfPile - temp1.ElevationOfStratumBottom : temp0.ElevationOfStratumBottom - temp1.ElevationOfStratumBottom;
                    }
                    //自然土层显示
                    //MessageBox.Show("自然土分层完成");
                    //人工土分层
                    //土层厚度标志
                    double flag = 0.5;
                    for (int i=0;i<lowersoilcal.Count;i++)
                    {
                        var temp1 = lowersoilcal[i];
                        if(temp1.Thickness>flag)
                        {
                            int a = 0;
                            double b = temp1.Thickness;
                            do
                            {
                                //b等于flag的情况已经搞定
                                b -= flag;
                                a += 1;
                            }
                            while (b > flag);

                            //分层开始，标准化构造
                            PileFoundationStrataInfoDto intelligentlayer_1 = new PileFoundationStrataInfoDto
                            {
                                PileFoundationID = temp1.PileFoundationID,
                                Thickness = flag,
                                IsHasWater = temp1.IsHasWater,
                                StratumID = temp1.StratumID,
                                Es0_100 = temp1.Es0_100,
                                Es100_200 = temp1.Es100_200,
                                Es200_300 = temp1.Es200_300,
                                Es300_400 = temp1.Es300_400,
                                Es400_500 = temp1.Es400_500,
                                Ers = temp1.Ers,
                                Gama = temp1.Gama
                            };
                            //新建后插入新层
                            for (int c=0;c<a;c++)
                            {
                                lowersoilcal.Insert(i, intelligentlayer_1);
                                
                            }
                            //新建与插入尾层
                            PileFoundationStrataInfoDto intelligentlayer_2 = new PileFoundationStrataInfoDto
                            {
                                PileFoundationID = temp1.PileFoundationID,
                                Thickness = b,
                                IsHasWater = temp1.IsHasWater,
                                StratumID = temp1.StratumID,
                                Es0_100 = temp1.Es0_100,
                                Es100_200 = temp1.Es100_200,
                                Es200_300 = temp1.Es200_300,
                                Es300_400 = temp1.Es300_400,
                                Es400_500 = temp1.Es400_500,
                                Ers = temp1.Ers,
                                Gama = temp1.Gama
                            };
                            lowersoilcal.Insert(i + a, intelligentlayer_2);
                            //移除被分解土层
                            lowersoilcal.Remove(lowersoilcal[i + a + 1]);
                        }
                        
                    }
                    //MessageBox.Show("人工划分土完成");

                    //分层总和法计算沉降
                    //临时变量还得有
                    double zofbase = 0;
                    double totalofai = 0;
                    double totalofaiesi = 0;
                    double totalofsettlement = 0;
                    

                    //新建一个列表，作为最终显示计算过程使用
                    //注意一定不要用之前的list进行计算，前面的list只是用来保存原始数据
                    //正确的做法是确认原list数据就是计算数据之后
                    //新建一个列表，进行插入与赋值，然后提供给datagrid显示
                    //使用old进行计算，计算结果给new
                    var calculateFinal = new List<PileFoundationStrataInfoDto>();

                    for (int i=0;i<lowersoilcal.Count;i++)
                    {
                        //old土层
                        var cal1 = lowersoilcal[i];

                        //土层距离计算点距离
                        zofbase += cal1.Thickness;

                        //new土层，计算完成后Add到final中
                        var finalsoil = new PileFoundationStrataInfoDto();

                        //new土层的上一层，List的最后一个元素
                        var finalsoil0 = calculateFinal.Count == 0 ?
                            null :
                            calculateFinal.Last();

                        //此项目用来判断是否为抗拔桩
                        double temp_p0 = P0;

                        //判断是否含水，含水减去10
                        var _gama = cal1.IsHasWater ? cal1.Gama - 10 : cal1.Gama;
                       

                        //先给相同属性赋值
                        finalsoil.PileFoundationID = cal1.PileFoundationID;
                        finalsoil.Thickness = cal1.Thickness;
                        finalsoil.Gama = cal1.Gama;
                        finalsoil.Es0_100 = cal1.Es0_100;
                        finalsoil.Es100_200 = cal1.Es100_200;
                        finalsoil.Es200_300 = cal1.Es200_300;
                        finalsoil.Es300_400 = cal1.Es300_400;
                        finalsoil.Es400_500 = cal1.Es400_500;

                        //finalsoil.Es500_600 = cal1.Es500_600;
                        finalsoil.IsHasWater = cal1.IsHasWater;
                        finalsoil.Ers = cal1.Ers;
                        finalsoil.IsHasWater = cal1.IsHasWater;
                        finalsoil.OBJECTID += i+1;
                        finalsoil.StratumID = cal1.StratumID;
                        //深度等于叠加厚度值
                        finalsoil.ZOfBase = zofbase;
                        //这里是土的有效重度，减去含水10的
                        finalsoil.Gama = _gama;

                       //平均附加应力系数，用来求Ai
                       //区分矩形和圆形承台
                        finalsoil.AverageAdditionalStressCoefficient = pfdto.Type == "Rectangle" ?
                            GetAverageAdditionalStressCoefficientRectangle(pfdto.L / pfdto.B, finalsoil.ZOfBase / (pfdto.B / 2)) :
                            GetAverageAdditionalStressCoefficientRound(pfdto.R, finalsoil.ZOfBase / pfdto.R);

                        //如果是第一层土，自重应力和Ai求法不一样
                        if (i == 0)
                        {
                            //自重用来确定计算深度
                            finalsoil.GravityStress = Middle + finalsoil.Gama * finalsoil.Thickness;
                            //Ai为第i层土附加应力系数沿土层厚度的积分值，可近似按分块面积计算
                            finalsoil.Ai = finalsoil.ZOfBase * finalsoil.AverageAdditionalStressCoefficient;
                            
                        }
                        //非第一层土
                        else
                        {
                            //自重应力等于上层自重加上该层层重
                            finalsoil.GravityStress = finalsoil0.GravityStress + finalsoil.Thickness * finalsoil.Gama;
                            //Ai为与上层土的差值
                            finalsoil.Ai = finalsoil.ZOfBase * finalsoil.AverageAdditionalStressCoefficient -
                                finalsoil0.ZOfBase * finalsoil0.AverageAdditionalStressCoefficient;
                        }

                        //判断使用Es值还是Ers值
                        finalsoil.Esi = temp_p0 > 0 ? GetEs(finalsoil) : GetErs(finalsoil);

                        //为下一步判断附加应力值做准备
                        temp_p0 = pfdto.Load;


                        //附加应力，以P0正负作为判断标准
                        //求附加应力，矩形和圆形的附加应力分情况讨论
                        if (pfdto.Type == "Rectangle")
                        {
                            //附加应力系数
                            finalsoil.AdditionnalStressCoefficnt = GetAdditionalStressCoeffcientRectangle(pfdto.L / pfdto.B, finalsoil.ZOfBase / (pfdto.B / 2));
                            //附加应力
                            finalsoil.AdditionalStress = P0 >= 0 ?
                                  P0 *  finalsoil.AdditionnalStressCoefficnt :
                                  temp_p0 *  finalsoil.AdditionnalStressCoefficnt;
                            //该层沉降量
                            finalsoil.SettlementOfSoil = P0 >= 0 ?
                                    P0 * finalsoil.Ai / finalsoil.Esi :
                                    temp_p0 * finalsoil.Ai / finalsoil.Esi;
                        }
                        else if (pfdto.Type == "Round")
                        {
                            //附加应力系数
                            finalsoil.AdditionnalStressCoefficnt = GetAdditionalStressCoeffcientRound(pfdto.R, finalsoil.ZOfBase);
                            //附加应力
                            finalsoil.AdditionalStress = P0 >= 0 ?
                                  P0 * finalsoil.AdditionnalStressCoefficnt :
                                  temp_p0 * finalsoil.AdditionnalStressCoefficnt;
                            //该层沉降
                            finalsoil.SettlementOfSoil = P0 >= 0 ?
                                P0 * finalsoil.Ai / finalsoil.Esi :
                                temp_p0 * finalsoil.Ai / finalsoil.Esi;
                        }

                        //累加数据保存与输出
                        //总的Ai
                        totalofai += finalsoil.Ai;
                        finalsoil.TotalOfAi = totalofai;

                        //以下为错误示范！！
                        //finalsoil.TotalOfAi  += finalsoil.Ai;
                        //totalofai= finalsoil.TotalOfAi;
                        
                        totalofaiesi += finalsoil.Ai / finalsoil.Esi ;
                        finalsoil.TotalOfAiEsi = totalofaiesi;

                        //土层压缩累加值
                        totalofsettlement += finalsoil.SettlementOfSoil;
                        finalsoil.TotalOfSttlement = totalofsettlement;
                        

                        //是否继续计算
                        //0.2自重应力大于等于附加应力，计算最后一次跳出循环
                        if (0.2 * finalsoil.GravityStress >= finalsoil.AdditionalStress)
                        {
                            //输出结果
                            //MessageBox.Show($"当前是天然层第{finalsoil.StratumID}层，0.2倍的自重应力{finalsoil.GravityStress * 0.2}>=附加应力{finalsoil.AdditionalStress}，" +
                                //$"分层总和法下该层沉降量为{finalsoil.TotalOfSttlement}mm，先不计算后面了");

                            //加入计算的尾层
                            calculateFinal.Add(finalsoil);
                            //传出分层总和沉降值
                            pfdto.FirstSettlement = finalsoil.TotalOfSttlement;
                            //因为这次是加入新List，不用删除土层元素了
                            //跳出循环，不在进行计算分层总和法沉降
                            break;
                        }

                        //将地层元素加入
                        calculateFinal.Add(finalsoil);
                    }
                    
                    //显示分层总和法计算步骤，数据源为final
                    GeneralInformation.ItemsSource = calculateFinal;

                    //计算最终沉降量
                    //压缩模量当量值
                    double AvEsi = totalofai / totalofaiesi;

                    //判断用户是否输入Posi值
                    //如果没有，就自动计算
                    double Posi =posi==null?
                        double.Parse(posi.Text):
                        GetEmpiricalCoefficientOfSettlementCalculation(AvEsi);
                    
                    //得到最终沉降
                    pfdto.FinalSettlement = pfdto.PosiE * Posi * totalofsettlement;
                    //MessageBox.Show($"桩基础{pfdto.Name}的最终沉降为{pfdto.FinalSettlement}mm");
                    //桩基础集合页面赋值
                    var pffinal = new PileFoundationDto();
                    pffinal.Xcoordinate = pfdto.Xcoordinate;
                    pffinal.Ycoordinate = pfdto.Ycoordinate;
                    pffinal.Name = pfdto.Name;
                    pffinal.FirstSettlement = totalofsettlement;
                    pffinal.FinalSettlement = pfdto.FinalSettlement;
                    pffinal.PosiE = pfdto.PosiE;
                    pffinal.Type = pfdto.Type;
                    pffinal.Load = pfdto.Load;
                    //加入List
                    pilefoundationfinal.Add(pffinal);

                }

                //显示桩基础沉降结果集合
                FinalResult.ItemsSource = pilefoundationfinal;
                #endregion

                MessageBox.Show("计算完成！");

            }

        }
        //沉降计算经验系数
        private double GetEmpiricalCoefficientOfSettlementCalculation(double avEsi)
        {
            //线性插值
            //Y=(X-X1)(Y2-Y1)/(X2-X1)+Y1
            double value = 0;
            if (avEsi >= 50)
                value = 0.4;
            else if (avEsi <= 10)
                value = 1.2;
            else if (avEsi > 10 && avEsi <= 15)
                value = (avEsi - 10) * (0.9 - 1.2) / (15 - 10) + 1.2;
            else if (avEsi > 15 && avEsi <= 20)
                value = (avEsi - 15) * (0.65 - 0.9) / (20 - 15) + 0.9;
            else if (avEsi > 20 && avEsi <= 35)
                value = (avEsi - 20) * (0.5 - 0.65) / (35 - 20) + 0.65;
            else if (avEsi > 35 && avEsi <= 50)
                value = (avEsi - 35) * (0.4 - 0.5) / (50 - 35) + 0.5;
            return value;
        }

        //计算压缩模量Es，输入附加应力Kn,输出Es单位KPa
        private double GetEs(PileFoundationStrataInfoDto temp1)
        {
            double value = 0;
            //首先赋值
            value = temp1.Es0_100;

            //判断附加应力在Es曲线的哪一段，如果该段不为空替换value
            if (temp1.AdditionalStress >= 100 && temp1.AdditionalStress < 200)
                value = temp1.Es100_200 == 0 ? value : temp1.Es100_200;
            else if (temp1.AdditionalStress >= 200 && temp1.AdditionalStress < 300)
                value = temp1.Es200_300 == 0 ? value : temp1.Es200_300;
            else if (temp1.AdditionalStress >= 300 && temp1.AdditionalStress < 400)
                value = temp1.Es300_400 == 0 ? value : temp1.Es300_400;
            else if (temp1.AdditionalStress >= 400 && temp1.AdditionalStress < 500)
                value = temp1.Es400_500 == 0 ? value : temp1.Es400_500;
            else if (temp1.AdditionalStress > 500)
                MessageBox.Show($"该分层附加应力值大于500，设置600的表吧！");
            return value;
        }

        //计算回弹再压缩模量Ers，输入附加应力Kn，输出Ers单位KPa
        private double GetErs(PileFoundationStrataInfoDto temp1)
        {
            return temp1.Ers ;
        }

        //平均附加应力系数矩形
        private double GetAverageAdditionalStressCoefficientRectangle(double m,double n)
        {
            double result;
            if(n==0)
            {
                result = 0.25;
            }
            else
            {
                //矩形附加应力平均系数公式
                double temp1 = Math.Pow(1 + m * m + n * n, 0.5);
                double temp2 = Math.Pow(1 + m * m, 0.5);
                result = 1 / (2 * Math.PI) * (Math.Atan(m / (n * temp1))
                    + m / n * Math.Log((temp1 - 1) * (temp2 + 1) / ((temp1 + 1) * (temp2 - 1)), Math.E)
                    + 1 / n * Math.Log((temp1 - m) * (temp2 + m) / ((temp1 + m) * (temp2 - m)), Math.E));
            }
            return 4*result;

        }

        //平均附加应力系数圆形
        private double GetAverageAdditionalStressCoefficientRound(double r, double z)
        {
            //z等于0的情况还没写
            double result;

            result = 1 + 2 * (r / z) - (1 + 2 * Math.Pow((r / z), 2)) / Math.Pow(1 + Math.Pow((r / z), 2), 0.5);

            return result;
        }

        //附加应力系数矩形
        private double GetAdditionalStressCoeffcientRectangle(double m,double n)
        {
            double result;
            if (n == 0)
            {
                result = 0.25;
            }
            else
            {
                double temp1 = m * m + n * n;
                double temp2 = n * n + 1;
                result =(1/(2*Math.PI))* (Math.Asin(m / (Math.Pow(temp1 * temp2, 0.5))) + 
                    m * n / (Math.Pow(temp1 + 1, 0.5)) * (1 / temp1 + 1 / temp2));
            }

            return 4*result;
        }

        //附加应力系数圆形
        private double GetAdditionalStressCoeffcientRound(double r,double z)
        {
            double result;
            result = 1 - 1 / (Math.Pow(1 + Math.Pow(r / z, 2), 3 / 2));
            return result;
        }
        
        //DataGrid生成新的行时候加入Index
        private void GeneralInformation_LoadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e)
        {
            
        }
    }
}

