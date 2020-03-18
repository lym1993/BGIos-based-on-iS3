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
            #region
            //点击计算按钮后，开始读取access表中数据
            //option为0时，采用默认得odbc读取方法
            //option为1时，采用oledb方法读取
            //先设置运行环境
            //string definitionFile = "PileFoundationTest.xml";
            //DbContext dbContext = new DbContext("Data\\PileFoundationTest\\PileFoundationTest.mdb", 0);
            //DbContext dbContext = new DbContext("Data\\Z14\\Z14.mdb", 0);
            ////定义DGObjectsDefinition的各项属性
            //DGObjectsDefinition def = new DGObjectsDefinition();
            //def.DefNamesSQL = null;
            //def.Name = "AllPileFoundations";
            //def.Type = "PileFoundation";
            //def.TableNameSQL = "PileFoundation,PileFoundationStrataInfo";
            //def.OrderSQL = "ID,ID";

            ////Load方法
            ////实例化objs
            //DGObjects objs = new DGObjects(def);
            //bool success = objs.load(dbContext);

            //DGObjects objcal = objs;

            //foreach (var value in objs.values)
            //{    
            //    MessageBox.Show("ok");
            //}

            //objs的rawdataset属性
            //objs.rawDataSet = new System.Data.DataSet();

            //DGObject objhelper = ObjectHelper.CreateDGObjectFromSubclassName(def.Type);
            //objhelper.LoadObjs(objs, dbContext);
            //objs.buildIDIndex();
            //objs.buildRowViewIndex();     
            //objscal.count是桩基础的数量，进行循环
            #endregion 

            //设置路径
            string pt = "Data\\Z14\\Z14.mdb";
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pt);
            using (var reader = new PileFoundationReader(path))
            {
                var foundations = reader.GetPileFoundations();
                MessageBox.Show($"Foundations count : {foundations.Count()}");
            }
           

















        }





















    }
}

