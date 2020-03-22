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
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pt);
            using (var reader = new PileFoundationReader(path))
            {
                //读取桩基础信息
                var foundations = reader.GetPileFoundations();
                //按Name名称排序
                foundations.OrderBy(x => x.Name);
                MessageBox.Show($"共读取表内桩基础数量: {foundations.Count()}个，即将开始进行沉降计算","计算提示");

                //怎么绑定datagrid呢
                //ObservableCollection<PileFoundationDto> pfd = foundations;

                //20200322将录入结果绑定在datagrid1上面
                GeneralInformation.ItemsSource = foundations;
               

                //建筑物地平面标高
                double HorizontalPlane =38.25;

                //遍历集合，开始计算沉降
                foreach (var pfdto in foundations)
                {
                    //按地层排序
                    //以地层标高降序排列
                    pfdto.PileFoundationStrataInfos.OrderByDescending(x => x.ElevationOfStratumBottom);
                    
                    //基底平均附加应力
                    double P0;

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
                            Upper = _gama * (HorizontalPlane - temp1.ElevationOfStratumBottom);
                            //跳出本次循环
                            continue;
                        }
                        
                        //中间土层，上层底减去该层底
                        if (pfdto.TopOfPile<=temp1.ElevationOfStratumBottom)
                        {
                            _upper=_gama*(temp0.ElevationOfStratumBottom-temp1.ElevationOfStratumBottom);
                        }
                        //桩顶处土层，上层底减去桩顶
                        if(pfdto.TopOfPile>=temp1.ElevationOfStratumBottom&&pfdto.TopOfPile<temp0.ElevationOfStratumBottom)
                        {
                            _upper = _gama * (temp0.ElevationOfStratumBottom - pfdto.TopOfPile);
                        }
                        //累加得到Upper
                        Upper += _upper;
                    }

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
                        //首先分割首层，桩顶标高在该层与上层之间
                        if (pfdto.TopOfPile >= temp1.ElevationOfStratumBottom && 
                            pfdto.TopOfPile < temp0.ElevationOfStratumBottom) 
                        {
                            Middle = _gama * (pfdto.TopOfPile - temp1.ElevationOfStratumBottom);
                            //跳出本次循环
                            continue;
                        }
                        
                        //中间层，叠加即可
                        if (temp1.ElevationOfStratumBottom > pfdto.BaseOfPile &&
                            temp0.ElevationOfStratumBottom < pfdto.TopOfPile)
                        {
                            _middle = _gama * (temp0.ElevationOfStratumBottom - temp1.ElevationOfStratumBottom);
                        }
                        //桩底所在层，得到最终结果
                        if (pfdto.BaseOfPile < temp1.ElevationOfStratumBottom &&
                            pfdto.BaseOfPile > temp0.ElevationOfStratumBottom)
                        {
                            _middle = _gama * (temp0.ElevationOfStratumBottom - pfdto.BaseOfPile);
                        }
                        //累加赋值
                        Middle += _middle;
                    }

                    //基底处某点的平均附加应力单位KN/M2
                    //等于荷载加上承台重减去原Upper土重
                    P0 = pfdto.Load + 25 * (pfdto.TopOfCushionCap - pfdto.TopOfPile) - Upper;

                    //基底处土的自重应力
                    double Lower = Middle;
                    
                    //分层总和法计算土层压缩量
                    //新建列表，计算下层土的压缩量
                    var lowersoilcal = pfdto.PileFoundationStrataInfos;
                    //移除桩底以上土层
                    for (int i=0;i<lowersoilcal.Count;i++)
                    {
                        //层底标高大于等于桩底标高的土层移除
                        if (lowersoilcal[i].ElevationOfStratumBottom>=pfdto.BaseOfPile)
                            lowersoilcal.Remove(lowersoilcal[i]);
                        //小心测试时候减没了！！
                        i--;
                    }
                    //自然土分层
                    for (int i=0; i<lowersoilcal.Count; i++)
                    {
                        MessageBox.Show($"计算桩基础{lowersoilcal[i].PileFoundationID}，天然土层分层");
                        var temp1 = lowersoilcal[i];
                        var temp0 = i == 0 ? null : lowersoilcal[i - 1];
                        //天然地层厚度划分
                        //如果是首层桩顶减去层底，非首层桩底减去上层底
                        temp1.Thickness = i == 0 ? pfdto.BaseOfPile - temp1.ElevationOfStratumBottom : temp0.ElevationOfStratumBottom - temp1.ElevationOfStratumBottom;
                    }
                    //自然土层显示

                    //土分层
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
                                Thickness = flag,
                                IsHasWater = temp1.IsHasWater,
                                StratumID = temp1.StratumID,
                                Es0_100 = temp1.Es0_100,
                                Es100_200 = temp1.Es100_200,
                                Es200_300=temp1.Es200_300,
                                Es300_400=temp1.Es300_400,
                                Es400_500=temp1.Es400_500,
                                Gama=temp1.Gama
                            };
                            //尾层构造
                            PileFoundationStrataInfoDto intelligentlayer_2 = new PileFoundationStrataInfoDto
                            {
                                Thickness = b,
                                IsHasWater = temp1.IsHasWater,
                                StratumID = temp1.StratumID,
                                Es0_100 = temp1.Es0_100,
                                Es100_200 = temp1.Es100_200,
                                Es200_300 = temp1.Es200_300,
                                Es300_400 = temp1.Es300_400,
                                Es400_500 = temp1.Es400_500,
                                Gama = temp1.Gama
                            };
                            //插入新层，移除原土层
                            for(int c=0;c<a;c++)
                            {
                                lowersoilcal.Insert(i, intelligentlayer_1);
                            }
                            lowersoilcal.Insert(i + a, intelligentlayer_2);
                            //移除被分解土层
                            lowersoilcal.Remove(lowersoilcal[i + a + 1]);
                            
                        }


                    }

                    //开始计算
                    //临时变量
                    double distancefrombase = 0;
                    for (int i=0;i<lowersoilcal.Count;i++)
                    {
                        var temp1 = lowersoilcal[i];
                        var temp0 = i==0?null : lowersoilcal[i - 1];
                        var _gama=temp1.IsHasWater ? temp1.Gama - 10 : temp1.Gama;

                        
                        distancefrombase += temp1.Thickness;
                        temp1.ZOfBase = distancefrombase;
                        temp1.GravityStress = Middle + temp1.Thickness * _gama;
                        temp1.AdditionStress = Upper * GetAdditionalStressCoefficientRectangle(pfdto.L / pfdto.B / 4, distancefrombase);
                        temp1.
                    }
                       


                    
                
                    

                    
                        
                    

                           
                    

                }
                
                #endregion



            }


















        }

        private double GetAdditionalStressCoefficientRectangle(double m,double n)
        {
            double result;
            if(n==0)
            {
                result = 0.25;
            }
            else
            {
                //附加应力平均系数公式
                double temp1 = Math.Pow(1 + m * m + n * n, 0.5);
                double temp2 = Math.Pow(1 + m * m, 0.5);
                result = 1 / (2 * Math.PI) * (Math.Atan(m / (n * temp1))
                    + m / n * Math.Log((temp1 - 1) * (temp2 + 1) / ((temp1 + 1) * (temp2 - 1)), Math.E)
                    + 1 / n * Math.Log((temp1 - m) * (temp2 + m) / ((temp1 + m) * (temp2 - m)), Math.E));
            }
            return result;

        }

        private void GetAdditionalStressCoefficientRound(double r)
        {

        }

        private void GeneralInformation_LoadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }
    }
}

