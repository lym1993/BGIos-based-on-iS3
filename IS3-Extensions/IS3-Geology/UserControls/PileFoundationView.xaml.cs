using iS3.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace iS3.Geology.UserControls
{
    /// <summary>
    /// PileFoundationView.xaml 的交互逻辑
    /// </summary>
    public partial class PileFoundationView : UserControl
    {
        //初始化绘图区域
        public PileFoundationView()
        {
            InitializeComponent();
            ScaleY = 1.0;
            BH_Width = 20.0;
        }

        public double ScaleY { get; set; }
        public DGObjectsCollection Strata { get; set; }
        public PileFoundation PileFoundation { get; set; }
        public bool IsEmpty { get; set; }
        public double BH_Width { get; set; }

        public void RefreshView()
        {
            LayoutRoot.Children.Clear();
            if (PileFoundation == null || PileFoundation.Geologies == null
                || PileFoundation.Geologies.Count == 0)
            {
                IsEmpty = true;
                return;
            }
            IsEmpty = false;

            double width = BH_Width;
            Brush whiteBrush = new SolidColorBrush(Colors.White);
            Brush blueBrush = new SolidColorBrush(Colors.Blue);
            Brush blackBrush = new SolidColorBrush(Colors.Black);
            Brush redBrush = new SolidColorBrush(Colors.Red);

            // Borehole Name
            //
            TextBlock tbName = new TextBlock();
            tbName.Foreground = redBrush;
            tbName.Text = PileFoundation.Name;
            tbName.FontWeight = FontWeights.Bold;
            Canvas.SetLeft(tbName, 0);
            Canvas.SetTop(tbName, -20);
            LayoutRoot.Children.Add(tbName);

            PileFoundationGeology bhGeo0 = PileFoundation.Geologies[0];
            foreach (PileFoundationGeology pfGeo in PileFoundation.Geologies)
            {
                double top = (PileFoundation.TopOfPile - pfGeo.Base) * ScaleY;
                double height = (pfGeo.Top - pfGeo.Base) * ScaleY;
                top = Math.Abs(top);
                height = Math.Abs(height);

                // Stratum rectangle
                //
                Rectangle rec = new Rectangle();
                rec.Fill = whiteBrush;
                rec.Stroke = blueBrush;
                rec.Width = width;
                rec.Height = height;
                Canvas.SetTop(rec, top);
                Canvas.SetLeft(rec, 0);

                // Stratum name
                //
                TextBlock tbStratumName = new TextBlock();
                tbStratumName.Foreground = blueBrush;
                //如果地层数据不为空，将地层名称赋值给tbstratumname
                //若为空，使用pfgeo中的stratumid的名称
                if (Strata != null)
                {
                    Stratum stratum = Strata[pfGeo.StratumID] as Stratum;
                    tbStratumName.Text = stratum.Name;
                }
                else
                    tbStratumName.Text = pfGeo.StratumID.ToString();
                Canvas.SetLeft(tbStratumName, width);
                Canvas.SetTop(tbStratumName, top + height / 2 - 8.0);

                // Stratum base elevation
                //
                TextBlock tbBaseElevation = new TextBlock();
                tbBaseElevation.Foreground = blackBrush;
                tbBaseElevation.Text = pfGeo.Base.ToString("0.00");
                Canvas.SetLeft(tbBaseElevation, width);
                Canvas.SetTop(tbBaseElevation, top + height - 8.0);

                LayoutRoot.Children.Add(rec);
                if (height >= 10.0)
                {
                    LayoutRoot.Children.Add(tbStratumName);
                    LayoutRoot.Children.Add(tbBaseElevation);
                }

                // Stratum top elevation
                //
                if (pfGeo == bhGeo0)
                {
                    TextBlock tbTopElevation = new TextBlock();
                    tbTopElevation.Foreground = blackBrush;
                    tbTopElevation.Text = pfGeo.Top.ToString("0.00");
                    Canvas.SetLeft(tbTopElevation, width);
                    Canvas.SetTop(tbTopElevation, top - 8.0);
                    LayoutRoot.Children.Add(tbTopElevation);
                }
            }
        }
    }
}
