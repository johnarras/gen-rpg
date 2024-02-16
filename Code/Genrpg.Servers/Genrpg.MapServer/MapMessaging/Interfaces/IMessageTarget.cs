using Genrpg.Shared.MapObjects.Entities;
using System;
using System.Collections.Generic;
using System.Text;


// This is used for things that need to receive messages but are not really in the map.
public interface IMessageTarget
{
    MapObject GetMessageTarget();
}