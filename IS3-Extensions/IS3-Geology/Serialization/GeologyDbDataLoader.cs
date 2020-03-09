using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.Common;
using System.Data.Odbc;

using iS3.Core;
using iS3.Core.Serialization;
using iS3.Geology;

namespace iS3.Geology.Serialization
{
    #region Copyright Notice
    //************************  Notice  **********************************
    //** This file is part of iS3
    //**
    //** Copyright (c) 2015 Tongji University iS3 Team. All rights reserved.
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

    #endregion
    //加载地质数据的类
    //继承与DbDataLoader类
    public class GeologyDbDataLoader : DbDataLoader
    {
        //iS3地质信息序列化，用来读取各项信息
        public GeologyDbDataLoader(DbContext dbContext)
            : base(dbContext)
        { }

        //以下为新代码
        // 200303读取PileFoundation信息
        // 重载3
        public bool ReadPileFoundation(DGObjects objs, string tableNameSQL,
            List<int> objsIDs)
        {
            string conditionSQL = WhereSQL(objsIDs);

            return ReadPileFoundation(objs, tableNameSQL, conditionSQL, null);
        }

        //读取PileFoundation的方法，返回bool值
        //重载4
        public bool ReadPileFoundation(DGObjects objs, string tableNameSQL,
            string conditionSQL, string orderSQL)
        {
            try
            {
                //首先读取PileFoundation
                //接着用第二种方法加载PileFoundationGeologies也就是桩基础属性
                _ReadPileFoundation(objs, tableNameSQL, conditionSQL,
                   orderSQL);
                _ReadPileFoundationGeologies2(objs);

               
            }
            //报错模块，弹出错误信息
            catch(DbException ex)
            {
                string str = ex.ToString();
                ErrorReport.Report(str);
                return false;
            }
            return true;
        }

        void _ReadPileFoundation(
            DGObjects objs,
            string tableNameSQL,
            string conditionSQL,
            string orderSQL)
        {
            ReadRawData(objs, tableNameSQL, orderSQL, conditionSQL);
            DataTable table = objs.rawDataSet.Tables[0];
            //以行为单位对数据进行枚举
            foreach (DataRow row in table.Rows)
            {
                //如果此列名称不是ID，掠过
                if (IsDbNull(row, "ID"))
                    continue;

                //实例化PileFoundation类
                //为实例赋予读取得数值
                PileFoundation PF = new PileFoundation(row);
                //基础编号、名字、形状
                PF.ID = ReadInt(row, "ID").Value;
                PF.Name = ReadString(row, "Name");
                PF.shape = ReadShape(row);

                //以下调用PileFoundation类中数据结构
                //基础形状，矩形、圆形
                //矩形基础长边L、短边B、圆形基础半径R
                //基础顶标高、基础底标高、桩底标高
                PF.Type = ReadString(row, "Type");
                PF.LOfRectangularBearingPlatform = ReadDouble(row, "L").Value;
                PF.BOfRectangularBearingPlatform = ReadDouble(row, "B").Value;
                PF.ROfRoundBearPlatform = ReadDouble(row, "R").Value;

                PF.TopOfCushionCap = ReadDouble(row, "TopOfCushionCap").Value;
                PF.TopOfPile = ReadDouble(row, "TopOfPile").Value;
                PF.BaseOfPile = ReadDouble(row, "BaseOfPile").Value;
                PF.Xcoordinate = ReadDouble(row, "Xcoordinate").Value;
                PF.Ycoordinate = ReadDouble(row, "Ycoordinate").Value;
                PF.PileLength = ReadDouble(row, "PileLength").Value;

                //将PF实例设置为objs对象中PF实例的key
                objs[PF.key] = PF;
            }
        }

        void _ReadPileFoundationGeologies2(DGObjects objs)
        {
            //通过rawdata读取数据，速度要比linq更快
            //如果录入的表数量少于两个，跳出方法
            if (objs.rawDataSet.Tables.Count <= 1)
                return;

            // method2: index the stratra info
            // 步骤2 插入地层信息
            //在这里插入的是表2，也就是PileFoundationStrataInfo表
            DataTable dt = objs.rawDataSet.Tables[1];

            //新建地层字典用来存放由PileFoundationGeology组成的列表
            Dictionary<int, List<PileFoundationGeology>> strata_dict =
                new Dictionary<int, List<PileFoundationGeology>>();

            // put the strata information into the dictionary
            // 将地层信息放入字典中
            foreach (DataRow row in dt.Rows)
            {
                //检查表格完整性
                //20200307进行了改动，在access表中添加了新的字段，
                if (IsDbNull(row, "StratumID") || IsDbNull(row, "ElevationOfStratumBottom"))
                {
                    string error = string.Format(
                        "Data table [{0}] error: [StratumID] or [ElevationOfStratumBottom] can't be null, [PileFoundationID] = {1}."
                        + Environment.NewLine
                        + "This record is ignore. Checking data is strongly recommended.",
                        dt.TableName, row["PileFoundation"]);
                    ErrorReport.Report(error);
                    continue;
                }
                //新建PFID字段，存放桩基础ID信息
                int PFID = ReadInt(row, "PileFoundationID").Value;

                //初始化geo列表，用来存储桩基础地质信息
                List<PileFoundationGeology> geo = null;
                //如果strata字典中包含Key值也就是PFID，将该键对应的值赋予geo实例
                if (strata_dict.ContainsKey(PFID))
                    geo = strata_dict[PFID];
                else
                {
                    geo = new List<PileFoundationGeology>();
                    strata_dict[PFID] = geo;
                }

                //实例化PFGeology，给桩基础下地层的性质赋值
                PileFoundationGeology pfg = new PileFoundationGeology();
                pfg.StratumID = ReadInt(row, "StratumID").Value;
                //地层底高程
                pfg.Base = ReadDouble(row, "ElevationOfStratumBottom").Value;
                //地层土壤重度
                pfg.Gama = ReadDouble(row, "Gama").Value;
                //地层土弹性模量Es
                pfg.Es0_100 = ReadDouble(row, "Es0_100").Value;
                pfg.Es100_200 = ReadDouble(row, "Es100_200").Value;
                pfg.Es200_300 = ReadDouble(row, "Es200_300").Value;
                pfg.Es300_400 = ReadDouble(row, "Es300_400").Value;
                pfg.Es400_500 = ReadDouble(row, "Es400_500").Value;

                //在geo实例中加入新的项目
                geo.Add(pfg);
            }

            // sort the pilefoundation geology
            // 桩基础地质分类
            // 这一句没看明白
            foreach (var geo in strata_dict.Values)
            {
                //匿名函数么？
                geo.Sort((x, y) => x.StratumID.CompareTo(y.StratumID));
            }

            // add the geology to pilefoundation
            // 将geo实例绑定到桩基础实例
            foreach (PileFoundation pf in objs.values)
            {
                //新建有关PFG的列表geo
                List<PileFoundationGeology> geo = null;
                if (strata_dict.ContainsKey(pf.ID))
                    geo = strata_dict[pf.ID];
                else
                    continue;

                double topofpile = pf.TopOfPile;
                foreach (var x in geo)
                {
                    //桩顶标高赋给TOP属性
                    x.Top = topofpile;
                    //
                    topofpile = x.Base;
                     //对PF实例中的geologies属性添加元素
                    pf.Geologies.Add(x);
                }
            }
        }



        //20200308读取桩基础所有数据,返回一个objs对象
        public void ReadPileFoundationInformation(DGObjects objspf,string tableNameSQL,
            string conditionSQL, string orderSQL)
        {
            try
            {
                //首先读取PileFoundation
                //接着用第二种方法加载PileFoundationGeologies也就是桩基础属性
                _ReadPileFoundationInformation(objspf, tableNameSQL, conditionSQL,
                   orderSQL);
                _ReadPileFoundationInformationGeo(objspf);
                

            }
            //报错模块，弹出错误信息
            catch (DbException ex)
            {
                string str = ex.ToString();
                ErrorReport.Report(str);
                
            }
        }

        void _ReadPileFoundationInformation(
            DGObjects objspf,
            string tableNameSQL,
            string conditionSQL,
            string orderSQL)
        {
            ReadRawData(objspf, tableNameSQL, orderSQL, conditionSQL);
            DataTable table = objspf.rawDataSet.Tables[0];
            //以行为单位对数据进行枚举
            foreach (DataRow row in table.Rows)
            {
                //如果此列名称不是ID，掠过
                if (IsDbNull(row, "ID"))
                    continue;

                //实例化PileFoundation类
                //为实例赋予读取得数值
                PileFoundation PF = new PileFoundation(row);
                //基础编号、名字、形状
                PF.ID = ReadInt(row, "ID").Value;
                PF.Name = ReadString(row, "Name");
                PF.shape = ReadShape(row);

                //以下调用PileFoundation类中数据结构
                //基础形状，矩形、圆形
                //矩形基础长边L、短边B、圆形基础半径R
                //基础顶标高、基础底标高、桩底标高
                PF.Type = ReadString(row, "Type");
                PF.LOfRectangularBearingPlatform = ReadDouble(row, "L").Value;
                PF.BOfRectangularBearingPlatform = ReadDouble(row, "B").Value;
                PF.ROfRoundBearPlatform = ReadDouble(row, "R").Value;

                PF.TopOfCushionCap = ReadDouble(row, "TopOfCushionCap").Value;
                PF.TopOfPile = ReadDouble(row, "TopOfPile").Value;
                PF.BaseOfPile = ReadDouble(row, "BaseOfPile").Value;
                PF.Xcoordinate = ReadDouble(row, "Xcoordinate").Value;
                PF.Ycoordinate = ReadDouble(row, "Ycoordinate").Value;
                PF.PileLength = ReadDouble(row, "PileLength").Value;

                //将PF实例设置为objs对象中PF实例的key
                objspf[PF.key] = PF;
            }
            
        }

        void  _ReadPileFoundationInformationGeo(DGObjects objspf)
        {
            

            // method2: index the stratra info
            // 步骤2 插入地层信息
            //在这里插入的是表2，也就是PileFoundationStrataInfo表
            DataTable dt = objspf.rawDataSet.Tables[1];

            //新建地层字典用来存放由PileFoundationGeology组成的列表
            Dictionary<int, List<PileFoundationGeology>> strata_dict =
                new Dictionary<int, List<PileFoundationGeology>>();

            // put the strata information into the dictionary
            // 将地层信息放入字典中
            foreach (DataRow row in dt.Rows)
            {
                //检查表格完整性
                //20200307进行了改动，在access表中添加了新的字段，
                if (IsDbNull(row, "StratumID") || IsDbNull(row, "ElevationOfStratumBottom"))
                {
                    string error = string.Format(
                        "Data table [{0}] error: [StratumID] or [ElevationOfStratumBottom] can't be null, [PileFoundationID] = {1}."
                        + Environment.NewLine
                        + "This record is ignore. Checking data is strongly recommended.",
                        dt.TableName, row["PileFoundation"]);
                    ErrorReport.Report(error);
                    continue;
                }
                //新建PFID字段，存放桩基础ID信息
                int PFID = ReadInt(row, "PileFoundationID").Value;

                //初始化geo列表，用来存储桩基础地质信息
                List<PileFoundationGeology> geo = null;
                //如果strata字典中包含Key值也就是PFID，将该键对应的值赋予geo实例
                if (strata_dict.ContainsKey(PFID))
                    geo = strata_dict[PFID];
                else
                {
                    geo = new List<PileFoundationGeology>();
                    strata_dict[PFID] = geo;
                }

                //实例化PFGeology，给桩基础下地层的性质赋值
                PileFoundationGeology pfg = new PileFoundationGeology();
                pfg.StratumID = ReadInt(row, "StratumID").Value;
                //地层底高程
                pfg.Base = ReadDouble(row, "ElevationOfStratumBottom").Value;
                //地层土壤重度
                pfg.Gama = ReadDouble(row, "Gama").Value;
                //地层土弹性模量Es
                pfg.Es0_100 = ReadDouble(row, "Es0_100").Value;
                pfg.Es100_200 = ReadDouble(row, "Es100_200").Value;
                pfg.Es200_300 = ReadDouble(row, "Es200_300").Value;
                pfg.Es300_400 = ReadDouble(row, "Es300_400").Value;
                pfg.Es400_500 = ReadDouble(row, "Es400_500").Value;

                //在geo实例中加入新的项目
                geo.Add(pfg);
            }

            // sort the pilefoundation geology
            // 桩基础地质分类
            // 这一句没看明白
            foreach (var geo in strata_dict.Values)
            {
                //匿名函数么？
                geo.Sort((x, y) => x.StratumID.CompareTo(y.StratumID));
            }

            // add the geology to pilefoundation
            // 将geo实例绑定到桩基础实例
            foreach (PileFoundation pf in objspf.values)
            {
                //新建有关PFG的列表geo
                List<PileFoundationGeology> geo = null;
                if (strata_dict.ContainsKey(pf.ID))
                    geo = strata_dict[pf.ID];
                else
                    continue;

                double topofpile = pf.TopOfPile;
                foreach (var x in geo)
                {
                    //桩顶标高赋给TOP属性
                    x.Top = topofpile;
                    //
                    topofpile = x.Base;
                    //对PF实例中的geologies属性添加元素
                    pf.Geologies.Add(x);
                }
            }
            
            
        }

        // 原代码
        // Read boreholes
        // 读取钻孔数据
        public bool ReadBoreholes(DGObjects objs, string tableNameSQL,
            List<int> objsIDs)
        {
            string conditionSQL = WhereSQL(objsIDs);

            return ReadBoreholes(objs, tableNameSQL, conditionSQL, null);
        }

        public bool ReadBoreholes(DGObjects objs, string tableNameSQL,
            string conditionSQL, string orderSQL)
        {
            try
            {
                _ReadBoreholes(objs, tableNameSQL, conditionSQL, 
                    orderSQL);
                _ReadBoreholeGeologies2(objs);
            }
            catch (DbException ex)
            {
                string str = ex.ToString();
                ErrorReport.Report(str);
                return false;
            }
            return true;
        }

        void _ReadBoreholes(
            DGObjects objs,
            string tableNameSQL,
            string conditionSQL,
            string orderSQL)
        {
            ReadRawData(objs, tableNameSQL, orderSQL, conditionSQL);
            DataTable table = objs.rawDataSet.Tables[0];
            foreach (DataRow row in table.Rows)
            {
                if (IsDbNull(row, "ID"))
                    continue;

                Borehole bh = new Borehole(row);
                bh.ID = ReadInt(row, "ID").Value;
                bh.Name = ReadString(row, "Name");
                bh.FullName = ReadString(row, "FullName");
                bh.Description = ReadString(row, "Description");
                bh.shape = ReadShape(row);

                bh.Type = ReadString(row, "BoreholeType");
                bh.Top = ReadDouble(row, "TopElevation").Value;
                bh.Base = bh.Top - ReadDouble(row, "BoreholeLength").Value;
                bh.Mileage = ReadDouble(row, "Mileage");

                objs[bh.key] = bh;
            }
        }

        void _ReadBoreholeGeologies(DGObjects objs)
        {
            // 方法1，使用linq，使用了where from select等linq方法
            // method1: maybe very slow because linq is slow.
            DataTable dt = objs.rawDataSet.Tables[1];
            foreach (Borehole bh in objs.values)
            {
                var rows = from row in dt.AsEnumerable()
                           where (int)row["BoreholeID"] == bh.ID
                           orderby row["ElevationOfStratumBottom"] descending
                           select row;

                double top = bh.Top;
                foreach (DataRow x in rows)
                {
                    //if (x["StratumID"].GetType() == typeof(System.DBNull)
                    //    || x["ElevationOfStratumBottom"].GetType() == typeof(System.DBNull))
                    if (IsDbNull(x, "StratumID") || IsDbNull(x, "ElevationOfStratumBottom"))
                    {
                        string error = string.Format(
                            "Data table [{0}] error: [StratumID] or [ElevationOfStratumBottom] can't be null, [BoreholeID] = {1}."
                            + Environment.NewLine
                            + "This record is ignore. Checking data is strongly recommended.",
                            dt.TableName, x["BoreholeID"]);
                        ErrorReport.Report(error);
                        continue;
                    }
                    BoreholeGeology bg = new BoreholeGeology();
                    bg.StratumID = ReadInt(x, "StratumID").Value;
                    bg.Top = top;
                    bg.Base = ReadDouble(x, "ElevationOfStratumBottom").Value;

                    top = bg.Base;
                    bh.Geologies.Add(bg);
                }
            }
        }

        void _ReadBoreholeGeologies2(DGObjects objs)
        {
            if (objs.rawDataSet.Tables.Count <= 1)
                return;

            // method2: index the stratra info
            DataTable dt = objs.rawDataSet.Tables[1];
            Dictionary<int, List<BoreholeGeology>> strata_dict =
                new Dictionary<int, List<BoreholeGeology>>();

            // put the strata information into the dictionary
            foreach (DataRow row in dt.Rows)
            {
                if (IsDbNull(row, "StratumID") || IsDbNull(row, "ElevationOfStratumBottom"))
                {
                    string error = string.Format(
                        "Data table [{0}] error: [StratumID] or [ElevationOfStratumBottom] can't be null, [BoreholeID] = {1}."
                        + Environment.NewLine
                        + "This record is ignore. Checking data is strongly recommended.",
                        dt.TableName, row["BoreholeID"]);
                    ErrorReport.Report(error);
                    continue;
                }

                int bhID = ReadInt(row, "BoreholeID").Value;
                List<BoreholeGeology> geo = null;
                if (strata_dict.ContainsKey(bhID))
                    geo = strata_dict[bhID];
                else
                {
                    geo = new List<BoreholeGeology>();
                    strata_dict[bhID] = geo;
                }
                BoreholeGeology bg = new BoreholeGeology();
                bg.StratumID = ReadInt(row, "StratumID").Value;
                bg.Base = ReadDouble(row, "ElevationOfStratumBottom").Value;
                geo.Add(bg);
            }

            // sort the borehole geology
            foreach (var geo in strata_dict.Values)
            {
                geo.Sort((x,y) => x.StratumID.CompareTo(y.StratumID));
            }

            // add the geology to borehole
            foreach (Borehole bh in objs.values)
            {
                List<BoreholeGeology> geo = null;
                if (strata_dict.ContainsKey(bh.ID))
                    geo = strata_dict[bh.ID];
                else
                    continue;

                double top = bh.Top;
                foreach (var x in geo)
                {
                    x.Top = top;
                    top = x.Base;
                    bh.Geologies.Add(x);
                }
            }
        }

        // Read strata
        // 读取地层
        public bool ReadStrata(
            DGObjects objs,
            string tableNameSQL,
            string conditionSQL,
            string orderSQL)
        {
            try
            {
                _ReadStrata(objs, tableNameSQL, conditionSQL,
                    orderSQL);
            }
            catch (DbException ex)
            {
                string str = ex.ToString();
                ErrorReport.Report(str);
                return false;
            }
            return true;
        }
        void _ReadStrata(
            DGObjects objs,
            string tableNameSQL,
            string conditionSQL,
            string orderSQL)
        {
            ReadRawData(objs, tableNameSQL, orderSQL, conditionSQL);
            DataTable table = objs.rawDataSet.Tables[0];
            foreach (DataRow row in table.Rows)
            {
                if (IsDbNull(row, "ID"))
                    continue;

                Stratum st = new Stratum(row);
                st.Name = ReadString(row, "Name");
                st.FullName = ReadString(row, "FullName");
                st.Description = ReadString(row, "Description");

                st.ID = ReadInt(row, "ID").Value;
                st.GeologyAge = ReadString(row, "GeologicalAge");
                st.FormationType = ReadString(row, "FormationType");
                st.Compaction = ReadString(row, "Compaction");
                st.ElevationRange = ReadString(row, "ElevationOfStratumBottom");
                st.ThicknessRange = ReadString(row, "Thickness");

                st.shape = ReadShape(row);

                objs[st.key] = st;
            }
        }

        // Read Soil properties
        // 读取土壤性质
        public bool ReadSoilProperties(
            DGObjects objs,
            string tableNameSQL,
            string conditionSQL,
            string orderSQL)
        {
            try
            {
                _ReadSoilProperties(objs, tableNameSQL, conditionSQL,
                    orderSQL);
            }
            catch (DbException ex)
            {
                string str = ex.ToString();
                ErrorReport.Report(str);
                return false;
            }
            return true;
        }
        void _ReadSoilProperties(
            DGObjects objs,
            string tableNameSQL,
            string conditionSQL,
            string orderSQL)
        {
            ReadRawData(objs, tableNameSQL, orderSQL, conditionSQL);
            DataTable table = objs.rawDataSet.Tables[0];
            foreach (DataRow reader in table.Rows)
            {
                if (IsDbNull(reader, "ID"))
                    continue;

                SoilProperty soilProp = new SoilProperty(reader);
                soilProp.ID = ReadInt(reader, "ID").Value;
                soilProp.Name = ReadString(reader, "Name");
                soilProp.StratumID = ReadInt(reader, "StratumID").Value;
                soilProp.StratumSectionID = ReadInt(reader, "StratumSectionID");

                //含水量
                soilProp.StaticProp.w = ReadDouble(reader, "w");
                //重度
                soilProp.StaticProp.gama = ReadDouble(reader, "gama");
                soilProp.StaticProp.c = ReadDouble(reader, "c");
                soilProp.StaticProp.fai = ReadDouble(reader, "fai");
                soilProp.StaticProp.cuu = ReadDouble(reader, "cuu");
                soilProp.StaticProp.faiuu = ReadDouble(reader, "faiuu");
                soilProp.StaticProp.Cs = ReadDouble(reader, "Cs");
                soilProp.StaticProp.qu = ReadDouble(reader, "qu");
                soilProp.StaticProp.K0 = ReadDouble(reader, "K0");
                soilProp.StaticProp.Kv = ReadDouble(reader, "Kv");
                soilProp.StaticProp.Kh = ReadDouble(reader, "Kh");
                //孔隙比?
                soilProp.StaticProp.e = ReadDouble(reader, "e");
                soilProp.StaticProp.av = ReadDouble(reader, "av");
                soilProp.StaticProp.Cu = ReadDouble(reader, "Cu");

                soilProp.StaticProp.G = ReadDouble(reader, "G");
                soilProp.StaticProp.Sr = ReadDouble(reader, "Sr");
                soilProp.StaticProp.ccq = ReadDouble(reader, "ccq");
                soilProp.StaticProp.faicq = ReadDouble(reader, "faicq");
                soilProp.StaticProp.c_s = ReadDouble(reader, "c_s");
                soilProp.StaticProp.fais = ReadDouble(reader, "fais");
                soilProp.StaticProp.a01_02 = ReadDouble(reader, "a01_02");
                soilProp.StaticProp.Es01_02 = ReadDouble(reader, "Es01_02");
                soilProp.StaticProp.ccu = ReadDouble(reader, "ccu");
                soilProp.StaticProp.faicu = ReadDouble(reader, "faicu");
                soilProp.StaticProp.cprime = ReadDouble(reader, "cprime");
                soilProp.StaticProp.faiprime = ReadDouble(reader, "faiprime");
                soilProp.StaticProp.E015_0025 = ReadDouble(reader, "E015_0025");
                soilProp.StaticProp.E02_0025 = ReadDouble(reader, "E02_0025");
                soilProp.StaticProp.E04_0025 = ReadDouble(reader, "E04_0025");

                SoilDynamicProperty dynamicProp = new SoilDynamicProperty();
                soilProp.DynamicProp.G0 = ReadDouble(reader, "G0");
                soilProp.DynamicProp.ar = ReadDouble(reader, "ar");
                soilProp.DynamicProp.br = ReadDouble(reader, "br");

                objs[soilProp.key] = soilProp;
            }
        }

        // Read StratumSections
        //
        public bool ReadStratumSections(
            DGObjects objs,
            string tableNameSQL,
            string conditionSQL,
            string orderSQL)
        {
            try
            {
                _ReadStratumSections(objs, tableNameSQL,
                    conditionSQL, orderSQL);
            }
            catch (DbException ex)
            {
                string str = ex.ToString();
                ErrorReport.Report(str);
                return false;
            }
            return true;
        }
        public void _ReadStratumSections(
            DGObjects objs,
            string tableNameSQL,
            string conditionSQL,
            string orderSQL)
        {
            ReadRawData(objs, tableNameSQL, orderSQL, conditionSQL);
            DataTable table = objs.rawDataSet.Tables[0];
            foreach (DataRow reader in table.Rows)
            {
                if (IsDbNull(reader, "ID"))
                    continue;
                StratumSection sec = new StratumSection(reader);
                sec.ID = ReadInt(reader, "ID").Value;
                sec.Name = ReadString(reader, "Name");
                sec.StartMileage = ReadDouble(reader, "StartMileage");
                sec.EndMileage = ReadDouble(reader, "EndMileage");
                objs[sec.key] = sec;
            }
        }

        // Read River Water
        //
        public bool ReadRiverWaters(
            DGObjects objs,
            string tableNameSQL,
            string conditionSQL,
            string orderSQL)
        {
            try
            {
                _ReadRiverWaters(objs, tableNameSQL,
                    conditionSQL, orderSQL);
            }
            catch (DbException ex)
            {
                string str = ex.ToString();
                ErrorReport.Report(str);
                return false;
            }
            return true;
        }
        void _ReadRiverWaters(
            DGObjects objs,
            string tableNameSQL,
            string conditionSQL,
            string orderSQL)
        {
            ReadRawData(objs, tableNameSQL, orderSQL, conditionSQL);
            DataTable table = objs.rawDataSet.Tables[0];
            foreach (DataRow reader in table.Rows)
            {
                if (IsDbNull(reader, "ID"))
                    continue;
                RiverWater rw = new RiverWater(reader);
                rw.ID = ReadInt(reader, "ID").Value;
                rw.ObservationLocation = ReadString(reader, "ObservationLocation");
                rw.HighestTidalLevel = ReadDouble(reader, "HighestTidalLevel");
                rw.HighestTidalLevelDate = ReadDateTime(reader, "HighestTidalLevelDate");
                rw.LowestTidalLevel = ReadDouble(reader, "LowestTidalLevel");
                rw.LowestTidalLevelDate = ReadDateTime(reader, "LowestTidalLevelDate");
                rw.AvHighTidalLevel = ReadDouble(reader, "AvHighTidalLevel");
                rw.AvLowTidalLevel = ReadDouble(reader, "AvLowTidalLevel");
                rw.AvTidalRange = ReadDouble(reader, "AvTidalRange");
                rw.DurationOfRise = ReadTimeSpan(reader, "DurationOfRise").ToString();
                rw.DurationOfFall = ReadTimeSpan(reader, "DurationOfFall").ToString();
                objs[rw.key] = rw;
            }
        }

        // Read River Water
        //
        public bool ReadPhreaticWaters(
            DGObjects objs,
            string tableNameSQL,
            string conditionSQL,
            string orderSQL)
        {
            try
            {
                _ReadPhreaticWaters(objs, tableNameSQL,
                    conditionSQL, orderSQL);
            }
            catch (DbException ex)
            {
                string str = ex.ToString();
                ErrorReport.Report(str);
                return false;
            }
            return true;
        }
        void _ReadPhreaticWaters(
            DGObjects objs,
            string tableNameSQL,
            string conditionSQL,
            string orderSQL)
        {
            ReadRawData(objs, tableNameSQL, orderSQL, conditionSQL);
            DataTable table = objs.rawDataSet.Tables[0];
            foreach (DataRow reader in table.Rows)
            {
                if (IsDbNull(reader, "ID"))
                    continue;
                PhreaticWater pw = new PhreaticWater(reader);
                pw.ID = ReadInt(reader, "ID").Value;
                pw.SiteName = ReadString(reader, "SiteName");
                pw.AvBuriedDepth = ReadDouble(reader, "AvBuriedDepth");
                pw.AvElevation = ReadDouble(reader, "AvElevation");
                objs[pw.key] = pw;
            }
        }

        // Read Confined Water
        //
        public bool ReadConfinedWaters(
            DGObjects objs,
            string tableNameSQL,
            string conditionSQL,
            string orderSQL)
        {
            try
            {
                _ReadConfinedWaters(objs, tableNameSQL,
                    conditionSQL, orderSQL);
            }
            catch (DbException ex)
            {
                string str = ex.ToString();
                ErrorReport.Report(str);
                return false;
            }
            return true;
        }
        void _ReadConfinedWaters(
            DGObjects objs,
            string tableNameSQL,
            string conditionSQL,
            string orderSQL)
        {
            ReadRawData(objs, tableNameSQL, orderSQL, conditionSQL);
            DataTable table = objs.rawDataSet.Tables[0];
            foreach (DataRow reader in table.Rows)
            {
                if (IsDbNull(reader, "ID"))
                    continue;
                ConfinedWater cw = new ConfinedWater(reader);
                cw.ID = ReadInt(reader, "ID").Value;
                cw.BoreholeName = ReadString(reader, "BoreholeName");
                cw.SiteName = ReadString(reader, "SiteName");
                cw.TopElevation = ReadDouble(reader, "TopElevation");
                cw.ObservationDepth = ReadDouble(reader, "ObservationDepth");
                cw.StratumName = ReadString(reader, "StatumName");
                cw.Layer = ReadInt(reader, "Layer");
                cw.WaterTable = ReadDouble(reader, "WaterTable");
                cw.ObservationDate = ReadDateTime(reader, "ObservationDate");
                objs[cw.key] = cw;
            }
        }

        // Read Water Properties
        //
        public bool ReadWaterProperties(
            DGObjects objs,
            string tableNameSQL,
            string conditionSQL,
            string orderSQL)
        {
            try
            {
                _ReadWaterProperties(objs, tableNameSQL,
                    conditionSQL, orderSQL);
            }
            catch (DbException ex)
            {
                string str = ex.ToString();
                ErrorReport.Report(str);
                return false;
            }
            return true;
        }
        void _ReadWaterProperties(
            DGObjects objs,
            string tableNameSQL,
            string conditionSQL,
            string orderSQL)
        {
            ReadRawData(objs, tableNameSQL, orderSQL, conditionSQL);
            DataTable table = objs.rawDataSet.Tables[0];
            foreach (DataRow reader in table.Rows)
            {
                if (IsDbNull(reader, "ID"))
                    continue;
                WaterProperty wp = new WaterProperty(reader);
                wp.ID = ReadInt(reader, "ID").Value;
                wp.BoreholeName = ReadString(reader, "BoreholeName");
                wp.Cl = ReadDouble(reader, "Cl");
                wp.SO4 = ReadDouble(reader, "SO4");
                wp.Mg = ReadDouble(reader, "Mg");
                wp.NH = ReadDouble(reader, "NH");
                wp.pH = ReadDouble(reader, "pH");
                wp.CO2 = ReadDouble(reader, "CO2");
                wp.Corrosion = ReadString(reader, "Corrosion");
                objs[wp.key] = wp;
            }
        }

    }
}
