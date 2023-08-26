using MessagePack;
using Genrpg.Shared.Audio.Entities;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.GameDatas;
using Genrpg.Shared.Zones.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.ProcGen.Entities
{
    [MessagePackObject]
    public class ProcGenSettings : BaseGameData
    {
        public override void Set(GameData gameData) { gameData.Set(this); }
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public List<ZoneType> ZoneTypes { get; set; }
        [Key(2)] public List<PlantType> PlantTypes { get; set; }
        [Key(3)] public List<RockType> RockTypes { get; set; }
        [Key(4)] public List<TreeType> TreeTypes { get; set; }
        [Key(5)] public List<TextureType> TextureTypes { get; set; }
        [Key(6)] public List<WeatherType> WeatherTypes { get; set; }
        [Key(7)] public List<BridgeType> BridgeTypes { get; set; }
        [Key(8)] public List<FenceType> FenceTypes { get; set; }
        [Key(9)] public List<ClutterType> ClutterTypes { get; set; }
        [Key(10)] public List<GroundObjType> GroundObjects { get; set; }
        [Key(11)] public List<MusicType> MusicTypes { get; set; }
        [Key(12)] public List<TextureChannel> TextureChannels { get; set; }

        public ZoneType GetZoneType(long idkey) { return _lookup.Get<ZoneType>(idkey); }
        public PlantType GetPlantType(long idkey) { return _lookup.Get<PlantType>(idkey); }
        public RockType GetRockType(long idkey) { return _lookup.Get<RockType>(idkey); }
        public TreeType GetTreeType(long idkey) { return _lookup.Get<TreeType>(idkey); }
        public TextureType GetTextureType(long idkey) { return _lookup.Get<TextureType>(idkey); }
        public WeatherType GetWeatherType(long idkey) { return _lookup.Get<WeatherType>(idkey); }
        public BridgeType GetBridgeType(long idkey) { return _lookup.Get<BridgeType>(idkey); }
        public FenceType GetFenceType(long idkey) { return _lookup.Get<FenceType>(idkey); }
        public ClutterType GetClutterType(long idkey) { return _lookup.Get<ClutterType>(idkey); }
        public GroundObjType GetGroundObjType(long idkey) { return _lookup.Get<GroundObjType>(idkey); }
        public MusicType GetMusicType(long idkey) { return _lookup.Get<MusicType>(idkey); }
        public TextureChannel GetTextureChannel(long idkey) { return _lookup.Get<TextureChannel>(idkey); }

        [Key(13)] public float TallChance { get; set; } = 0.5f;
        [Key(14)] public float TreeDirtRadius { get; set; } = 9.0f;

    }
}
