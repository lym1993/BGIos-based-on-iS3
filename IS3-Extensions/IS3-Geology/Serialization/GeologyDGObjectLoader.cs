﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using iS3.Core;
using iS3.Core.Serialization;

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
    //加载地质对象的类
    public class GeologyDGObjectLoader
    {
        protected GeologyDbDataLoader _dbLoader;

        public GeologyDGObjectLoader(DbContext dbContext)
        {
            _dbLoader = new GeologyDbDataLoader(dbContext);
        }

        //200303加入LoadPileFoundation方法
        //该方法用来在iS3系统中加载PileFoundation生成的objs
        public bool LoadPileFoundation(DGObjects objs)
        {
            DGObjectsDefinition def = objs.definition;
            if (def == null)
                return false;
            bool success = _dbLoader.ReadPileFoundation(objs,
                def.TableNameSQL, def.ConditionSQL, def.OrderSQL);
            return success;
        }

        //20200308加入LoadPileFoundationInformation方法
        //该方法用来计算PileFoundatin沉降数据
        public void LoadPileFoundationInformation(DGObjects objspf)
        {
            DGObjectsDefinition def = objspf.definition;
            _dbLoader.ReadPileFoundationInformation(objspf, def.TableNameSQL,
            def.ConditionSQL, def.OrderSQL);
                       
        }

        public bool LoadBoreholes(DGObjects objs)
        {
            DGObjectsDefinition def = objs.definition;
            if (def == null)
                return false;
            bool success = _dbLoader.ReadBoreholes(objs,
                def.TableNameSQL, def.ConditionSQL, def.OrderSQL);
            return success;
        }

        public bool LoadStrata(DGObjects objs)
        {
            DGObjectsDefinition def = objs.definition;
            if (def == null)
                return false;
            bool success = _dbLoader.ReadStrata(objs,
                def.TableNameSQL, def.ConditionSQL, def.OrderSQL);
            return success;
        }

        public bool LoadSoilProperties(DGObjects objs)
        {
            DGObjectsDefinition def = objs.definition;
            if (def == null)
                return false;
            bool success = _dbLoader.ReadSoilProperties(objs,
                def.TableNameSQL, def.ConditionSQL, def.OrderSQL);
            return success;
        }

        public bool LoadStratumSections(DGObjects objs)
        {
            DGObjectsDefinition def = objs.definition;
            if (def == null)
                return false;
            bool success = _dbLoader.ReadStratumSections(objs,
                def.TableNameSQL, def.ConditionSQL, def.OrderSQL);
            return success;
        }

        public bool LoadRiverWaters(DGObjects objs)
        {
            DGObjectsDefinition def = objs.definition;
            if (def == null)
                return false;
            bool success = _dbLoader.ReadRiverWaters(objs,
                def.TableNameSQL, def.ConditionSQL, def.OrderSQL);
            return success;
        }

        public bool LoadPhreaticWaters(DGObjects objs)
        {
            DGObjectsDefinition def = objs.definition;
            if (def == null)
                return false;
            bool success = _dbLoader.ReadPhreaticWaters(objs,
                def.TableNameSQL, def.ConditionSQL, def.OrderSQL);
            return success;
        }

        public bool LoadConfinedWaters(DGObjects objs)
        {
            DGObjectsDefinition def = objs.definition;
            if (def == null)
                return false;
            bool success = _dbLoader.ReadConfinedWaters(objs,
                def.TableNameSQL, def.ConditionSQL, def.OrderSQL);
            return success;
        }

        public bool LoadWaterProperties(DGObjects objs)
        {
            DGObjectsDefinition def = objs.definition;
            if (def == null)
                return false;
            bool success = _dbLoader.ReadWaterProperties(objs,
                def.TableNameSQL, def.ConditionSQL, def.OrderSQL);
            return success;
        }
    }
}
