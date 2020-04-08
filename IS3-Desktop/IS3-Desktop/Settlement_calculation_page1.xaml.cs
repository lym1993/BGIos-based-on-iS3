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
        //用来存储沉降集合
        List<PileFoundationDto> FinalPileFoundationDtos = new List<PileFoundationDto>();
        
        public Settlement_calculation_page1()
        {
            InitializeComponent();
        }
        //沉降计算按钮
        private void Button_Click_AllPF(object sender, RoutedEventArgs e)
        {
            //清空结果集合
            FinalPileFoundationDtos.Clear();

            #region 使用Dapper扩展方法
            //设置路径
            string pt = "Data\\Z14\\Z14.mdb";
           // string pt = "Data\\Z14\\0327.mdb";
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pt);
            //开始进行计算
            using (var reader = new PileFoundationReader(path))
            {
                //读取桩基础信息
                var foundations = reader.GetPileFoundations();
                
                //按Name名称排序
                foundations.OrderBy(x => x.ID);
                //MessageBox.Show($"共读取表内桩基础数量: {foundations.Count()}个，即将开始进行沉降计算","计算提示");
                
                //建筑物地平面标高
                double HorizontalPlane =38.25;

                //新建结果集合，用来显示桩基础计算结果
                var pffinalViewCollection = new List<PileFoundationFinalView>();

                //第二页，用来显示分层总和法的计算步骤
                var strataInfoViewCollection = new List<StaraInfoFinalView>();
                //第二页索引
                int id = 0;

                //桩基础Name筛选框
                var PFName = pfName.Text;

                //桩基础第一次遍历
                //目的是计算middle和独立p0；
                foreach(var pfdto in foundations)
                {
                    //如果搜索框是空的，计算所有
                    if (PFName == "") ;
                    else if (pfdto.Name != PFName)
                        continue;

                    //按地层排序
                    //以地层标高降序排列
                    pfdto.PileFoundationStrataInfos.OrderByDescending(x => x.ElevationOfStratumBottom);

                    #region 桩顶以上土的自重Upper
                    //地平面以下，桩顶以上原土层自重应力
                    double Upper = 0;

                    //第二层，元素为天然地层，求桩顶以上自重应力
                    for (int i = 0; i < pfdto.PileFoundationStrataInfos.Count; i++)
                    {
                        //临时变量
                        double _upper = 0;
                        var temp1 = pfdto.PileFoundationStrataInfos[i];
                        //如果为首层，i-1为空
                        var temp0 = i == 0 ? null : pfdto.PileFoundationStrataInfos[i - 1];
                        //判断是否含水
                        double _gama = temp1.IsHasWater ? temp1.Gama - 10 : temp1.Gama;

                        //如果桩顶标高小于等于层底标高，计算该层土重

                        //如果是第一层土，地平面减去第一层底
                        //由于是深基础，桩顶标高必小于天然土层底标高
                        if (i == 0)
                        {
                            _upper = _gama * (HorizontalPlane - temp1.ElevationOfStratumBottom);
                        }
                        //上部土中间段，上层底减去该层底
                        //1底标高大于等于桩顶标高且大于上层底标高
                        //20200327是否要改为承台顶标高
                        else if (temp1.ElevationOfStratumBottom >= pfdto.TopOfPile)
                        {
                            _upper = _gama * (temp0.ElevationOfStratumBottom - temp1.ElevationOfStratumBottom);
                        }
                        //上部土桩顶处土层，上层底减去桩顶
                        else if (pfdto.TopOfPile >= temp1.ElevationOfStratumBottom && pfdto.TopOfPile < temp0.ElevationOfStratumBottom)
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
                    #endregion

                    #region 桩顶至桩底土的自重Middle
                    //计算桩顶至桩底土的自重应力
                    double Middle = 0;
                    for (int i = 0; i < pfdto.PileFoundationStrataInfos.Count; i++)
                    {
                        //临时变量
                        double _middle = 0;
                        var temp1 = pfdto.PileFoundationStrataInfos[i];
                        //如果是首层i-1就为空
                        var temp0 = i == 0 ? null : pfdto.PileFoundationStrataInfos[i - 1];

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
                        else if (pfdto.BaseOfPile >= temp1.ElevationOfStratumBottom &&
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
                    //输出Middle值
                    pfdto.MiddleSoilWeight = Middle;
                    #endregion

                    //基底处某点的平均附加应力值的单位是Kn/M2
                    //等于荷载加上承台重减去原Upper土重
                    double P0 = pfdto.Load + 25 * (pfdto.TopOfCushionCap - pfdto.TopOfPile) - Upper;
                    //输出由于基础上方均布荷载所产生的有效应力值
                    pfdto.AloneAdditionalStress = P0;
                }

                //桩基础第二次遍历
                //目的是扫描相邻基础，开始计算沉降
                foreach (var pfdto in foundations)
                {
                    //如果搜索框是空的，计算所有
                    if (PFName == "") ;
                    else if (pfdto.Name != PFName)
                        continue;

                    //基底处土的自重应力
                    double Lower = pfdto.MiddleSoilWeight;

                    //分层总和法计算土层压缩量
                    //新建列表，计算下层土的压缩量
                    var lowersoilcal = pfdto.PileFoundationStrataInfos;
                    //移除桩底以上土层
                    for (int i = 0; i < lowersoilcal.Count; i++)
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
                    for (int i = 0; i < lowersoilcal.Count; i++)
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
                    for (int i = 0; i < lowersoilcal.Count; i++)
                    {
                        var temp1 = lowersoilcal[i];
                        if (temp1.Thickness > flag)
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
                            for (int c = 0; c < a; c++)
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

                    //设定相邻桩基础扫描幅度
                    double Flag = 150;
                    //相邻基础集合
                    //表示会对pfdto有影响的集合
                    var AdjacentFoundations = new List<PileFoundationDto>();
                    foreach (var pfdto0 in foundations)
                    {
                        var x = pfdto.Xcoordinate;
                        var y = pfdto.Ycoordinate;
                        var x1 = pfdto0.Xcoordinate;
                        var y1 = pfdto0.Ycoordinate;
                        //a=x*x+y*y
                        var a = Math.Pow(x - x1, 2) + Math.Pow(y - y1, 2);
                        var b = Math.Pow(Flag, 2);
                        //距离小于15且不等于0；
                        if (a <= b & a != 0)
                            AdjacentFoundations.Add(pfdto0);
                    }

                    //新建一个列表，作为最终显示计算过程使用
                    //注意一定不要用之前的list进行计算，前面的list只是用来保存原始数据
                    //正确的做法是确认原list数据就是计算数据之后
                    //新建一个列表，进行插入与赋值，然后提供给datagrid显示
                    //使用old进行计算，计算结果给new
                    var calculateFinal = new List<PileFoundationStrataInfoDto>();
                    
                    for (int i = 0; i < lowersoilcal.Count; i++)
                    {
                        //old土层
                        var cal1 = lowersoilcal[i];

                        //土层距离计算点距离
                        zofbase += cal1.Thickness;

                        //new土层，计算完成后Add到final中
                        var finalsoil = new PileFoundationStrataInfoDto();

                        //new土层的上一层，也就是List的最后一个元素
                        var finalsoil0 = calculateFinal.Count == 0 ?
                            null :
                            calculateFinal.Last();

                        //此项目用来判断是否为抗拔桩
                        double temp_p0 = pfdto.AloneAdditionalStress;

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
                        finalsoil.OBJECTID += i + 1;
                        finalsoil.StratumID = cal1.StratumID;
                        //深度等于叠加厚度值
                        finalsoil.ZOfBase = zofbase;
                        //这里是土的有效重度，减去含水10的
                        finalsoil.Gama = _gama;

                        //平均附加应力系数，用来求Ai
                        //200405到这里要考虑相邻基的础影响了
                        //第一步：求本基础自身荷载影响下的平均附加应力系数
                        //区分矩形和圆形承台，矩形因为是中心角点，这一部分系数要乘以四，圆形不用乘
                        finalsoil.AverageAdditionalStressCoefficient = pfdto.Type == "Rectangle" ?
                            4*GetAverageAdditionalStressCoefficientRectangle(pfdto.L / pfdto.B, finalsoil.ZOfBase / (pfdto.B / 2)) :
                            GetAverageAdditionalStressCoefficientRound(pfdto.R, finalsoil.ZOfBase / pfdto.R);

                        //受到相邻基础影响后的新增的有效应力
                        double totalofadjacent = 0;

                        //遍历影响列表，以桩基础为单位
                        foreach(var pfdto0 in AdjacentFoundations)
                        {
                            //有效应力为负时，使用荷载数值
                            double tempP0 = 0;
                            
                            //如果相邻基础附加应力小于零，其荷载数值直接作为传过来的附加应力
                            if (pfdto0.AloneAdditionalStress <= 0)
                                tempP0 = pfdto0.Load;
                            //如果是圆形承台，等效为矩形
                            if (pfdto0.Type == "Round")
                                pfdto0.L = pfdto0.B = 2 * pfdto0.R;

                            //开始计算
                            //临时变量
                            //中心基础
                            var x = pfdto.Xcoordinate;
                            var y = pfdto.Ycoordinate;
                            //相邻基础
                            var x1 = pfdto0.Xcoordinate;
                            var y1 = pfdto0.Ycoordinate;
                            //距离基础距离
                            double n = finalsoil.ZOfBase;
                            //临时附加应力
                            double additionaltempP0 = 0;
                           
                            //长度均选择绝对值
                            var x2 = Math.Abs(x - x1);
                            var y2 = Math.Abs(y - y1);
                            //关键假设，本项目基础长边L在Y轴方向，短边B在X轴方向
                            //4个临时变量
                            double temp1 = x2 + pfdto0.B / 2;
                            double temp2 = y2 + pfdto0.L / 2;
                            double temp3 = x2 - pfdto0.B / 2;
                            double temp4 = y2 - pfdto0.L / 2;

                            //被竖着切成两块
                            if (x1>x-pfdto.B/2&&x1<x+pfdto.B/2)
                            {
                                //横竖原理相同，不同就在于L和B的选择
                                additionaltempP0 = tempP0 * (GetAdditionalStressCoeffcientRectangle(LdivideB(temp2, pfdto0.B / 2 - Math.Abs(x1 - x)), n) -
                                    GetAdditionalStressCoeffcientRectangle(LdivideB(temp4, pfdto0.B / 2 - Math.Abs(x1 - x)), n) +
                                    GetAdditionalStressCoeffcientRectangle(LdivideB(temp2, pfdto0.B / 2 + Math.Abs(x1 - x)), n) -
                                    GetAdditionalStressCoeffcientRectangle(LdivideB(temp4, pfdto0.B / 2 + Math.Abs(x1 - x)), n));
                                
                            }
                            //被横着切成两块
                            else if(y1>y-pfdto.L/2&&y1<y+pfdto.L/2)
                            {
                                //两个大矩形减去两个小的矩形之后叠加
                                additionaltempP0 = tempP0 * (GetAdditionalStressCoeffcientRectangle(LdivideB(temp1, pfdto0.L / 2 - Math.Abs(y1 - y)), n) -
                                     GetAdditionalStressCoeffcientRectangle(LdivideB(temp3, pfdto0.L / 2 - Math.Abs(y1 - y)), n) +
                                     GetAdditionalStressCoeffcientRectangle(LdivideB(temp1, pfdto0.L / 2 + Math.Abs(y1 - y)), n) -
                                     GetAdditionalStressCoeffcientRectangle(LdivideB(temp3, pfdto0.L / 2 + Math.Abs(y1 - y)), n));
                            
                            }
                            //其他情况（xy均不同轴且没有交叉点）
                            else
                            {
                                
                                //考虑L很大B很小的情况？？
                                additionaltempP0 = tempP0 * (GetAdditionalStressCoeffcientRectangle(LdivideB(temp1,temp2),n) + 
                                    GetAdditionalStressCoeffcientRectangle(LdivideB(temp3,temp4),n) -
                                    GetAdditionalStressCoeffcientRectangle(LdivideB(temp1,temp4),n) - 
                                    GetAdditionalStressCoeffcientRectangle(LdivideB(temp2,temp3),n));
                            }
                            //累加值输出
                            totalofadjacent += additionaltempP0;

                            //MessageBox.Show($"{pfdto0.Name}影响该基础的附加应力为{additionaltempP0}Kn，累计值为{totalofadjacent}Kn");

                        }

                        //如果是第一层土，自重应力和Ai求法不一样
                        if (i == 0)
                        {
                            //自重用来确定计算深度
                            finalsoil.GravityStress = pfdto.MiddleSoilWeight + finalsoil.Gama * finalsoil.Thickness;
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
                        
                        //为下一步判断附加应力值做准备
                        temp_p0 = pfdto.Load;
                        
                        //附加应力，以P0正负作为判断标准
                        //求附加应力，矩形和圆形的附加应力分情况讨论
                        if (pfdto.Type == "Rectangle")
                        {
                            //附加应力系数
                            finalsoil.AdditionnalStressCoefficnt = GetAdditionalStressCoeffcientRectangle(pfdto.L / pfdto.B, finalsoil.ZOfBase / (pfdto.B / 2));
                            //附加应力
                            finalsoil.AdditionalStress = pfdto.AloneAdditionalStress >= 0 ?
                                  pfdto.AloneAdditionalStress * finalsoil.AdditionnalStressCoefficnt+totalofadjacent :
                                  temp_p0 * finalsoil.AdditionnalStressCoefficnt+totalofadjacent;
                            //判断使用Es值还是Ers值
                            finalsoil.Esi = temp_p0 > 0 ? GetEs(finalsoil) : GetErs(finalsoil);
                            //该层沉降量
                            finalsoil.SettlementOfSoil = pfdto.AloneAdditionalStress >= 0 ?
                                    pfdto.AloneAdditionalStress * finalsoil.Ai / finalsoil.Esi :
                                    temp_p0 * finalsoil.Ai / finalsoil.Esi;
                        }
                        else if (pfdto.Type == "Round")
                        {
                            //附加应力系数
                            finalsoil.AdditionnalStressCoefficnt = GetAdditionalStressCoeffcientRound(pfdto.R, finalsoil.ZOfBase);
                            //附加应力
                            finalsoil.AdditionalStress = pfdto.AloneAdditionalStress >= 0 ?
                                  pfdto.AloneAdditionalStress * finalsoil.AdditionnalStressCoefficnt + totalofadjacent :
                                  temp_p0 * finalsoil.AdditionnalStressCoefficnt + totalofadjacent;
                            //判断使用Es值还是Ers值
                            finalsoil.Esi = temp_p0 > 0 ? GetEs(finalsoil) : GetErs(finalsoil);
                            //该层沉降
                            finalsoil.SettlementOfSoil = pfdto.AloneAdditionalStress >= 0 ?
                                pfdto.AloneAdditionalStress * finalsoil.Ai / finalsoil.Esi :
                                temp_p0 * finalsoil.Ai / finalsoil.Esi;
                        }

                        //累加数据保存与输出
                        //总的Ai
                        totalofai += finalsoil.Ai;
                        finalsoil.TotalOfAi = totalofai;

                        //以下为错误示范！！
                        //finalsoil.TotalOfAi  += finalsoil.Ai;
                        //totalofai= finalsoil.TotalOfAi;

                        totalofaiesi += finalsoil.Ai / finalsoil.Esi;
                        finalsoil.TotalOfAiEsi = totalofaiesi;

                        //土层压缩累加值
                        totalofsettlement += finalsoil.SettlementOfSoil;
                        finalsoil.TotalOfSttlement = totalofsettlement;


                        //判断是否继续计算
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

                            //第二页显示元素，建立好之后加入列表中
                            var temp0 = new StaraInfoFinalView();

                            id += 1;
                            temp0.ID = id;
                            temp0.Name = pfdto.Name;
                            temp0.ZOfBase = finalsoil.ZOfBase.ToString("#0.0");
                            temp0.Thickness = finalsoil.Thickness.ToString("#0.0");
                            temp0.StratumID = finalsoil.StratumID;
                            temp0.GravityStress = finalsoil.GravityStress.ToString("#0.0");
                            temp0.AdditionalStress = finalsoil.AdditionalStress.ToString("#0.000");
                            temp0.Esi = finalsoil.Esi.ToString("#0.0");
                            temp0.AverageAdditionalStressCoefficient = finalsoil.AverageAdditionalStressCoefficient.ToString("#0.000");
                            temp0.Ai = finalsoil.Ai.ToString("#0.000");
                            temp0.SettlementOfSoil = finalsoil.SettlementOfSoil.ToString("#0.000");
                            temp0.TotalOfAi = finalsoil.TotalOfAi.ToString("#0.000");
                            temp0.TotalOfSttlement = finalsoil.TotalOfSttlement.ToString("#0.000");
                            temp0.TotalOfAiEsi = finalsoil.TotalOfAiEsi.ToString("#0.000");

                            strataInfoViewCollection.Add(temp0);

                            //跳出循环，不在进行计算分层总和法沉降
                            break;
                        }

                        //将地层元素加入
                        calculateFinal.Add(finalsoil);

                        //第二页显示元素，建立好之后加入列表中
                        var temp = new StaraInfoFinalView();
                        id += 1;
                        temp.ID = id;
                        temp.Name = pfdto.Name;
                        temp.ZOfBase = finalsoil.ZOfBase.ToString("#0.0");
                        temp.Thickness = finalsoil.Thickness.ToString("#0.0");
                        temp.StratumID = finalsoil.StratumID;
                        temp.GravityStress = finalsoil.GravityStress.ToString("#0.0");
                        temp.AdditionalStress = finalsoil.AdditionalStress.ToString("#0.000");
                        temp.Esi = finalsoil.Esi.ToString("#0.0");
                        temp.AverageAdditionalStressCoefficient = finalsoil.AverageAdditionalStressCoefficient.ToString("#0.000");
                        temp.Ai = finalsoil.Ai.ToString("#0.000");
                        temp.SettlementOfSoil = finalsoil.SettlementOfSoil.ToString("#0.000");
                        temp.TotalOfAi = finalsoil.TotalOfAi.ToString("#0.000");
                        temp.TotalOfSttlement = finalsoil.TotalOfSttlement.ToString("#0.000");
                        temp.TotalOfAiEsi = finalsoil.TotalOfAiEsi.ToString("#0.000");

                        strataInfoViewCollection.Add(temp);
                    }

                    //显示分层总和法计算步骤，数据源为final
                    //GeneralInformation.ItemsSource = calculateFinal;

                    //计算最终沉降量
                    //压缩模量当量值
                    double AvEsi = totalofai / totalofaiesi;

                    //判断用户是否输入Posi值
                    //如果没有，就自动计算
                    double Posi = posi.Text == string.Empty ? GetEmpiricalCoefficientOfSettlementCalculation(AvEsi) : double.Parse(posi.Text);

                    //得到最终沉降
                    pfdto.FinalSettlement = pfdto.PosiE * Posi * totalofsettlement;
                    //MessageBox.Show($"桩基础{pfdto.Name}的最终沉降为{pfdto.FinalSettlement}mm");

                    //输出pfdto的值，该集合包含了沉降项目的沉降信息
                    FinalPileFoundationDtos.Add(pfdto);

                    //第一页桩基础集合页面赋值
                    var pffinalview = new PileFoundationFinalView();
                    pffinalview.ID = pffinalViewCollection.Count + 1;
                    pffinalview.Name = pfdto.Name;
                    pffinalview.Type = pfdto.Type;
                    pffinalview.FirstSettlement = totalofsettlement.ToString("#0.000");
                    pffinalview.PosiE = pfdto.PosiE.ToString("#0.000");
                    pffinalview.FinalSettlement = pfdto.FinalSettlement.ToString("#0.000");
                    pffinalview.Load = pfdto.Load.ToString("#0");
                    pffinalview.Posi = Posi.ToString("#0.000");

                    pffinalViewCollection.Add(pffinalview);
                    
                }

                
                //显示第一页桩基础沉降结果集合
                //FinalResult.ItemsSource = pilefoundationfinal;
                FinalResult.ItemsSource = pffinalViewCollection;

                //第二页分层总和法步骤
                GeneralInformation.ItemsSource = strataInfoViewCollection;
                #endregion
                //MessageBox.Show("所有计算全部完成！");

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
            //首先将结果赋值0-100
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
            //大于500的统统使用400-500的值
            else if (temp1.AdditionalStress > 500)
                value = temp1.Es400_500 == 0 ? value : temp1.Es400_500;
            return value;
        }

        //计算回弹再压缩模量Ers，输入附加应力Kn，输出Ers单位KPa
        private double GetErs(PileFoundationStrataInfoDto temp1)
        {
            return temp1.Ers ;
        }

        //平均附加应力系数矩形，没有乘以四！！！
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
            return result;

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
            //e.Row.Header = e.Row.GetIndex() + 1;
        }

        //桩基础筛选框
        private void Button_Click_PFName(object sender, RoutedEventArgs e)
        {
            this.Button_Click_AllPF(sender,e);
            //切换视角
            Steps_Of_Layered_Summation_Method.IsSelected=true;
        }

        //绘制等值线图方法
        private void Button_Click_Contour(object sender, RoutedEventArgs e)
        {
            //读取桩基础信息
            //FinalPileFoundationDtos就是
            if (FinalPileFoundationDtos.Count == 0)
                MessageBox.Show("请先计算桩基础沉降！");

            MessageBox.Show($"扫描到桩基础信息{FinalPileFoundationDtos.Count}个，即将开始绘制等值线图");

            //文博后面交给你了
            foreach(var pfdto in FinalPileFoundationDtos)
            {
                MessageBox.Show($"桩基础{pfdto.Name}的横坐标{pfdto.Xcoordinate},的纵坐标{pfdto.Ycoordinate},最终沉降为{pfdto.FinalSettlement}。");
            }
        }

        //相邻基础影响下m值的求法
        private double LdivideB(double a,double b)
        {
            var result = a > b ? (a / b) : (b / a);
            return result;
        }
    }
}

