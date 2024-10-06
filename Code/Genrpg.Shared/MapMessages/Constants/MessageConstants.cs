using MessagePack;
using Genrpg.Shared.MapServer.Constants;
using System;
using System.Collections.Generic;
using System.Text;

    [MessagePackObject]
public class MessageConstants
{
    public const float DefaultGridDistance = SharedMapConstants.TerrainPatchSize*75/100;

    public const float DefaultUpdatePosDistance = DefaultGridDistance;

    public const float DelayedMessageTimeGranularity = 0.01f;

}
