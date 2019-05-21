//************************  Notice  **********************************
//** This file is part of iS3
//**
//** Copyright (c) 2018 Tongji University iS3 Team. All rights reserved.
//**
//** This library is free software; you can redistribute it and/or
//** modify it under the terms of the GNU Lesser General Public
//** License as published by the Free Software Foundation; either
//** version 3 of the License, or (at your option) any later version.
//**
//** This library is distributed in the hope that it will be useful,
//** but WITHOUT ANY WARRANTY; without even the implied warranty of
//** MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//** Lesser General Public License for more details.
//**
//** In addition, as a special exception,  that plugins developed for iS3,
//** are allowed to remain closed sourced and can be distributed under any license .
//** These rights are included in the file LGPL_EXCEPTION.txt in this package.
//**
//**************************************************************************

//** This program is designed for configing data files for iS3.Desktop (standalone version).
//** The data files usually reside in the iS3-Desktop-Stand-alone\Output\Data directory,
//** including:
//**          ProjectList.xml
//**          <ProjectName>.xml
//**          <ProjectName>\*.*
//**
//** This program depends on .NET Framework 4.5
//
//** This program depends on the following library (apart from .NET library):
//**          Xceed.Wpf.Toolkit
//**          iS3.Core
//**          iS3.ArcGIS
//**          iS3.Unity.Webplayer
//**          ArcGIS Runtime SDK for .NET 10.2.5
//

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using iS3.Core;

namespace iS3.Config
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        string iS3Path = "";
        string dataPath = "";

        string projID = "";
        double projLocX = 0;
        double projLocY = 0;

        public App()
        {
            Startup += App_Startup;
        }

        //定义App_Stratup监听事件
        private void App_Startup(object sender, StartupEventArgs e)
        {
            GdbHelper.Initialize();

            // Load resources from ResourceDictionary.xaml
            // 从ResourceDictionary.xaml中加载资源
            ResourceDictionary dict = (ResourceDictionary)Application.LoadComponent(
                new Uri("/iS3.Config;Component/ResourceDictionary.xaml", System.UriKind.Relative));
            this.Resources.MergedDictionaries.Add(dict);

            // open a background window that start the configuration
            // 打开一个后台窗口开始进行配置
            Window backgroundWnd = new Window();
            this.MainWindow = backgroundWnd;

            //startconfig赋给bool值ok
            bool ok = StartConfig();
            if (ok)
            {
                // do something
            }
            else
            {

            }

            Shutdown();
        }

        //bool变量 开始配置
        bool StartConfig()
        {
            bool? success;

            // Preparation Step 1 - Config path to iS3 and data directory
            // 准备环节第一步 配置iS3路径与data路径
            ConfPathWindow mainWnd = new ConfPathWindow();
            mainWnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            success = mainWnd.ShowDialog();
            if (success == null || success.Value == false)
            {
                return false;
            }
            iS3Path = mainWnd.ExePath;
            dataPath = mainWnd.DataPath;

            // Preparation Step 2 - Config projects
            // 准备环节第二步 配置项目
            string projListFile = dataPath + "\\ProjectList.xml";
            ProjectList projList = ConfigCore.LoadProjectList(projListFile);
            ProjectsWindow projsWnd = new ProjectsWindow(projList);
            projsWnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            success = projsWnd.ShowDialog();
            if (success == null || success.Value == false)
            {
                return false;
            }
            projID = projsWnd.ProjID;
            projLocX = projsWnd.ProjLocX;
            projLocY = projsWnd.ProjLocY;

            // Step 1 - Config project general definition
            // 配置环节第一步 配置项目整体描述
            ProjectDefinition projDef = ConfigCore.LoadProjectDefinition(dataPath, projID);
            if (projDef == null)
                projDef = ConfigCore.CreateProjectDefinition(dataPath, projID);
            ProjGnrDefWindow projGnrDefWnd = new ProjGnrDefWindow(projDef);
            projGnrDefWnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            success = projGnrDefWnd.ShowDialog();
            if (success == null || success.Value == false)
            {
                return false;
            }

            // Step 2 - Config engineering maps definition of the project
            // 配置环节第二步 配置地图
            ProjEMapDefWindow projEMapsDefWnd = new ProjEMapDefWindow(projDef);
            projEMapsDefWnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            success = projEMapsDefWnd.ShowDialog();
            if (success == null || success.Value == false)
            {
                return false;
            }

            // Config 3D map
            //      Note: Because there is nothing to configure for 3D, add "preview 3D model" in DomainDefWindow
            //
            //Proj3DViewDefWindow proj3DViewDefWnd = new Proj3DViewDefWindow(projDef);
            //proj3DViewDefWnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //success = proj3DViewDefWnd.ShowDialog();
            //if (success == null || success.Value == false)
            //{
            //    return false;
            //}

            // Step 3 - Config domains of the project
            // 配置项目定义域
            List<EMapLayers> eMapLayersList = projEMapsDefWnd.EMapLayersList;
            Project prj = ConfigCore.LoadProject(dataPath, projID);
            DomainDefWindow domainDefWnd = new DomainDefWindow(projDef, prj, eMapLayersList);
            domainDefWnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            success = domainDefWnd.ShowDialog();
            if (success == null || success.Value == false)
            {
                return false;
            }

            // Step 4 - Config project tree
            // 配置项目树
            ProjTreeDefWindow prjTreeDefWnd = new ProjTreeDefWindow(prj);
            prjTreeDefWnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            success = prjTreeDefWnd.ShowDialog();
            if (success == null || success.Value == false)
            {
                return false;
            }

            // Write ProjectList.xml
            // 
            ConfigCore.WriteProjectList(projListFile, projsWnd.ProjectList);
            
            // Write <projectID>.xml
            //
            ConfigCore.WriteProject(dataPath, projID, projDef, prj);

            // Write <projectID>.py
            //
            ConfigCore.WriteViewsDef(iS3Path, projID, projDef);

            string format = 
                "Congratulations!\r\n" +
                "The following files has been generated successfully.\r\n" +
                "    {0}\\ProjectList.xml\r\n" +
                "    {1}\\{2}.xml\r\n" +
                "    {3}\\Data\\{4}\\{5}.py\r\n" +
                "The {6} project is ready to use in iS3.";

            string str = string.Format(format, dataPath, dataPath, projID, iS3Path, projID, projID, projID);
            MessageBox.Show(str, "Success", MessageBoxButton.OK);

            return true;
        }
    }
}
