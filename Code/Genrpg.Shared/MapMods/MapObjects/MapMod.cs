﻿using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.MapObjects.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.MapMods.MapObjects
{
    public class MapMod : MapObject
    {
        public MapMod(IRepositoryService repositoryService) : base(repositoryService) { }
    }
}
