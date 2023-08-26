using MessagePack;
using Genrpg.Shared.MapServer.Constants;
using System;
using System.Collections.Generic;
using System.Text;

    [MessagePackObject]
public class MessageConstants
{
    public const float DefaultGridDistance = SharedMapConstants.TerrainPatchSize*40/100;


    public const float DelayedMessageTimeGranularity = 0.01f;

}
