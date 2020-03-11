using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iS3.Core;
using System.Data;
using iS3.Core.Serialization;
using iS3.Geology.Serialization;
using System.Windows;
using iS3.Geology.UserControls;

namespace iS3.Geology
{
    //参照BoreholeGeology
    //代表桩基础的地质性质（各层标高、天然地层等等）
    public class PileFoundationGeology
    {
        //地层顶标高
        public double Top { get; set; }
        //地层底标高
        public double Base { get; set; }     
        //天然地层编号
        public int StratumID { get; set; }
        //土壤重度gama
        public double Gama { get; set; }
        //土壤压缩模量Es
        public double Es0_100 { get; set; }
        public double Es100_200 { get; set; }
        public double Es200_300 { get; set; }
        public double Es300_400 { get; set; }
        public double Es400_500 { get; set; }
        
    }

    //PileFoundation类，代表桩基础的基本性质
    public class PileFoundation:DGObject
    {
        //ID和Name、Shape继承自DGObject，因而不再定义

        //桩基础形状
        public string Type { get; set; }
        //矩形承台长边l
        public double LOfRectangularBearingPlatform { get; set; }
        //矩形承台短边b
        public double BOfRectangularBearingPlatform { get; set; }
        //圆形承台半径
        public double ROfRoundBearPlatform { get; set; }
        //承台顶标高
        public double TopOfCushionCap { get; set; }
        //桩顶标高OR承台底部标高
        public double TopOfPile { get; set; }
        //桩底部标高
        public double BaseOfPile { get; set; }
       
        //桩基础横坐标
        public double Xcoordinate { get; set; }
        //桩基础纵坐标
        public double Ycoordinate { get; set; }
        //桩长
        public double PileLength { get; set; }
        //承台底桩径
        public double DiameterOfPile { get; set; }
        //承台下总桩数
        public int NumberOfPile { get; set; }
        //矩形桩基础短边桩数量
        public int PilesOfB { get; set; }
        //桩间距离
        public double DistanceOfPile { get; set; }

        //针对桩基础地质情况集合的列表
        public List<PileFoundationGeology> Geologies { get; set; }

        //PileFoundation的构造函数,构造新列表赋值给Geologies列表
        public PileFoundation()
        {
            Geologies = new List<PileFoundationGeology>();
        }
        //含database参数的构造函数
        public PileFoundation(DataRow rawData):base(rawData)
        {
            Geologies = new List<PileFoundationGeology>();
        }

        public override bool LoadObjs(DGObjects objs, DbContext dbContext)
        {
            //GDGOLoader中没有定义LoadPileFoundation方法，重新去定义一下
            GeologyDGObjectLoader loader = new GeologyDGObjectLoader(dbContext);
            bool success = loader.LoadPileFoundation(objs);
            return success;
        }


        //现在用不上
        //ToString覆写方法
        public override string ToString()
        {
            string str = base.ToString();

            //桩底标高、桩顶标高，基础形状
            string str1 = string.Format(
                ", TopOfPile={0}, BaseOfPile={1},  Type={2}, Geo=",
                TopOfPile, BaseOfPile,  Type);
            str += str1;

            foreach (var geo in Geologies)
            {
                str += geo.StratumID + ",";
            }

            return str;
        }

        //200311参照Borehole格式写表格视图
        public override List<DataView> tableViews(IEnumerable<DGObject> objs)
        {
            List<DataView> dataViews = base.tableViews(objs);

            if (parent.rawDataSet.Tables.Count > 1)
            {
                DataTable table = parent.rawDataSet.Tables[1];
                string filter = idFilter(objs);
                DataView view = new DataView(table, filter, "[PileFoundationID]",
                    DataViewRowState.CurrentRows);
                dataViews.Add(view);
            }

            return dataViews;
        }

        //字符串加工
        //没什么用
        string idFilter(IEnumerable<DGObject> objs)
        {
            string sql = "PileFoundatonID in (";
            foreach (var obj in objs)
            {
                sql += obj.ID.ToString();
                sql += ",";
            }
            sql += ")";
            return sql;
        }

        //这是wpf显示控件图表视图
        //如果需要右下角显示图像，改这里
        public override List<FrameworkElement> chartViews(
            IEnumerable<DGObject> objs, double width, double height)
        {
            List<FrameworkElement> charts = new List<FrameworkElement>();

            List<PileFoundation> pfs = new List<PileFoundation>();
            foreach (PileFoundation pf in objs)
            {
                if (pf != null && pf.Geologies.Count > 0)
                    pfs.Add(pf);
            }

            Domain geologyDomain = Globals.project.getDomain(DomainType.Geology);
            DGObjectsCollection strata = geologyDomain.getObjects("Stratum");


            //实例化新的钻孔显示界面，这里尚未改动
            BoreholeCollectionView bhsView = new BoreholeCollectionView();
            bhsView.Name = "Geology";
            // 这一句有问题，先不改了  bhsView.Boreholes = pfs;
            bhsView.Strata = strata;
            bhsView.ViewerHeight = height;
            bhsView.RefreshView();
            bhsView.UpdateLayout();
            charts.Add(bhsView);

            return charts;
        }
    }
}
