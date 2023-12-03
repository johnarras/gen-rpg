using MessagePack;
namespace Genrpg.Shared.DataStores.Entities
{
    [MessagePackObject]
    public class AssetCategory
    {
        public const string DownloadAssetRootPath = "BundledAssets/";

        public const string Atlas = "atlas";
        public const string Screens = "screens";
        public const string UI = "ui";
        public const string Audio = "audio";

        public const string Monsters = "monsters";
        public const string Trees = "trees";
        public const string Bushes = "bushes";
        public const string Rocks = "rocks";
        public const string Props = "props";
        public const string TerrainTex = "terraintex";
        public const string Grass = "grass";
        public const string Magic = "magic";
        public const string Prefabs = "prefabs";



        [Key(0)] public string Name { get; set; }
        [Key(1)] public string Path { get; set; }
        [Key(2)] public bool ShouldCheckResources { get; set; }

    }
}
