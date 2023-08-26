using MessagePack;
using Genrpg.Shared.MapObjects.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.GroundObjects.Entities
{
    [MessagePackObject]
    public class GroundObject : MapObject
    {
        public override bool IsGroundObject() { return true; }
    }
}
