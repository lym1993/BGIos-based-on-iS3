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
using iS3.Core;

namespace iS3.Geology.UserControls
{
    /// <summary>
    /// PileFoundationCollectionView.xaml 的交互逻辑
    /// </summary>
    public partial class PileFoundationCollectionView : UserControl
    {
        public PileFoundationCollectionView()
        {
            InitializeComponent();
            ScaleX = 1.0;
            ScaleY = 1.0;
        }

        public double ScaleX { get; set; }
        public double ScaleY { get; set; }
        public double ViewerHeight { get; set; }
        public double Top { get; set; }
        public double Base { get; set; }
        public double TotalLength { get { return Top - Base; } }
        public DGObjectsCollection Strata { get; set; }
        public List<PileFoundation> PileFoundations { get; set; }

        double x_space = 70;
        double y_margin = 20;
        double bh_width = 10;

        public void RefreshView()
        {
            if (PileFoundations == null || PileFoundations.Count == 0)
                return;

            //搜寻桩基础顶部与底部
            SearchBoreholesTopAndBase();
            ScaleY = (ViewerHeight - 80) / TotalLength;

            LayoutRoot.Children.Clear();
            LayoutRoot.Width = 0.0;
            LayoutRoot.Height = ViewerHeight;

            int i = 0;
            Brush blackBrush = new SolidColorBrush(Colors.Black);
            Polyline pline = new Polyline();
            pline.Stroke = blackBrush;

            foreach (PileFoundation pf  in PileFoundations)
            {
                PileFoundationView bhView = new PileFoundationView();
                bhView.BH_Width = bh_width;
                bhView.ScaleY = ScaleY;
                bhView.Strata = Strata;
                bhView.PileFoundation = pf;
                bhView.RefreshView();
                if (bhView.IsEmpty)
                    continue;

                double bhTop = pf.TopOfPile;
                double y = (Top - bhTop) * ScaleY;
                TranslateTransform translate = new TranslateTransform();
                translate.X = i * x_space * ScaleX;
                translate.Y = y + y_margin;
                bhView.RenderTransform = translate;

                LayoutRoot.Children.Add(bhView);
                LayoutRoot.Width += x_space * ScaleX;

                
               
                i++;
            }

            if (pline.Points.Count >= 2)
                LayoutRoot.Children.Add(pline);
        }

        void SearchBoreholesTopAndBase()
        {
            Top = PileFoundations[0].TopOfPile;
            Base = PileFoundations[0].BaseOfPile;

            foreach (PileFoundation pf in PileFoundations)
            {
                if (pf.TopOfPile > Top)
                    Top = pf.TopOfPile;
                if (pf.BaseOfPile < Base)
                    Base = pf.BaseOfPile;
            }
        }

        
    }
}
