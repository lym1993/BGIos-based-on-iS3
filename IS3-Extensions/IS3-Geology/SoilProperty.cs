﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data;

using iS3.Core;
using iS3.Core.Serialization;
using iS3.Geology.Serialization;

namespace iS3.Geology
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

    public class StratumSection : DGObject
    { 

        //定义里程
        public double? StartMileage { get; set; }
        public double? EndMileage { get; set; }

        public StratumSection()
        {
        }

        public StratumSection(DataRow rawData)
            :base(rawData)
        {
        }

        public override bool LoadObjs(DGObjects objs, DbContext dbContext)
        {
            GeologyDGObjectLoader loader2 = new GeologyDGObjectLoader(dbContext);
            bool success = loader2.LoadStratumSections(objs);
            return success;
        }
    }

    //定义土壤性质类
    public class SoilProperty : DGObject
    {
        //定义土层ID
        public int StratumID { get; set; }
        //定义土层截面ID
        public int? StratumSectionID { get; set; }
        //土壤静态性质
        public SoilStaticProperty StaticProp { get; set; }
        //土壤动态性质
        public SoilDynamicProperty DynamicProp { get; set; }

        // Use StratumSectionID:Name as the key
        public override string key
        {
            get
            {
                return StratumSectionID.ToString() + ":" + Name;
            }
        }

        public SoilProperty()
        {
            StaticProp = new SoilStaticProperty();
            DynamicProp = new SoilDynamicProperty();
        }

        public SoilProperty(DataRow rawData)
            :base(rawData)
        {
            StaticProp = new SoilStaticProperty();
            DynamicProp = new SoilDynamicProperty();
        }

        public override bool LoadObjs(DGObjects objs, DbContext dbContext)
        {
            GeologyDGObjectLoader loader2 = new GeologyDGObjectLoader(dbContext);
            bool success = loader2.LoadSoilProperties(objs);
            return success;
        }
    }

    public class SoilStaticProperty
    {
        //含水量
        public double? w { get; set; }
        //重度
        public double? gama { get; set; }
        public double? c { get; set; }
        public double? fai { get; set; }
        public double? cuu { get; set; }
        public double? faiuu { get; set; }
        public double? Cs { get; set; }         // swelling index
        public double? qu { get; set; }
        public double? K0 { get; set; }
        public double? Kv { get; set; }
        public double? Kh { get; set; }
        public double? e { get; set; }
        public double? av { get; set; }
        public double? Cu { get; set; }         // coefficient of uniformity

        public double? G { get; set; }
        public double? Sr { get; set; }
        public double? ccq { get; set; }
        public double? faicq { get; set; }
        public double? c_s { get; set; }
        public double? fais { get; set; }
        public double? a01_02 { get; set; }
        public double? Es01_02 { get; set; }
        public double? c_u { get; set; }
        public double? faiu { get; set; }
        public double? ccu { get; set; }
        public double? faicu { get; set; }
        public double? cprime { get; set; }
        public double? faiprime { get; set; }
        public double? E015_0025 { get; set; }
        public double? E02_0025 { get; set; }
        public double? E04_0025 { get; set; }
    }

    public class SoilDynamicProperty
    {
        public double? G0 { get; set; }
        public double? ar { get; set; }
        public double? br { get; set; }
    }
}