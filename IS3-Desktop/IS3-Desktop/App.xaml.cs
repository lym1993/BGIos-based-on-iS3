﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Media;
using System.Reflection;

using iS3.Core;
using iS3.Core.Serialization;
using iS3.ArcGIS.Graphics;
using iS3.ArcGIS.Geometry;

// For test
using iS3.Core.Graphics;
using iS3.Core.Geometry;



namespace iS3.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        MainFrame _mainFrame;
        string _sysDataDir = @"..\..\Data";

        static IS3GraphicEngine _graphicEngine = new IS3GraphicEngine();
        static IS3GeometryEngine _geometryEngine = new IS3GeometryEngine();

        public MainFrame MainFrame
        {
            get { return _mainFrame; }
            set { _mainFrame = value; }
        }
        public Project Project
        {
            get
            {
                if (_mainFrame != null)
                    return _mainFrame.prj;
                else
                    return null;
            }
        }
        public string SysDataDir { get { return _sysDataDir; } }

        public App()
        {
            Startup += App_Startup;
            Exit += App_Exit;
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            // Before initializing the ArcGIS Runtime first 
            // set the ArcGIS Runtime license by providing the license string 
            // obtained from the License Viewer tool.
            //ArcGISRuntime.SetLicense("Place the License String in here");

            // Initialize the ArcGIS Runtime before any components are created.
            try
            {
                string exeLocation = Assembly.GetExecutingAssembly().Location;
                string exePath = System.IO.Path.GetDirectoryName(exeLocation);
                DirectoryInfo di = System.IO.Directory.GetParent(exePath);
                string rootPath = di.FullName+ "\\Output";
                string dataPath = rootPath + "\\Data";
                string tilePath = dataPath + "\\TPKs";
                //确定根目录
                Runtime.rootPath = rootPath;
                //确定数据目录
                Runtime.dataPath = dataPath;
                //确定切片文件目录
                Runtime.tilePath = tilePath;
                //确定配置文件目录
                Runtime.configurationPath = rootPath + "\\config\\DBconfig.xml";

                //ArcGISRuntime.Initialize();
                //arcgisruntime初始化
                Runtime.initializeEngines(_graphicEngine, _geometryEngine);
                Globals.application = this;
                Globals.mainthreadID = Thread.CurrentThread.ManagedThreadId;
                //test();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

                // Exit application
                this.Shutdown();
            }
        }

        void App_Exit(object sender, ExitEventArgs e)
        {
        }

        void test()
        {
            IPointCollection pc = Runtime.geometryEngine.newPointCollection();
            IMapPoint p1 = Runtime.geometryEngine.newMapPoint(0, 0);
            IMapPoint p2 = Runtime.geometryEngine.newMapPoint(0, 100);
            IMapPoint p3 = Runtime.geometryEngine.newMapPoint(100, 0);
            pc.Add(p1);
            pc.Add(p2);
            pc.Add(p3);

            IGraphicCollection gc = Runtime.graphicEngine.newGraphicCollection();
            IGraphic g1 = Runtime.graphicEngine.newLine(p1, p2);
            IGraphic g2 = Runtime.graphicEngine.newLine(p1, p3);
            gc.Add(g1);
            gc.Add(g2);

            IEnvelope env = GraphicsUtil.GetGraphicsEnvelope(gc);

        }
    }

}
