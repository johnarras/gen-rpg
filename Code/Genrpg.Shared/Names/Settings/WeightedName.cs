using MessagePack;
namespace Genrpg.Shared.Names.Settings
{
    [MessagePackObject]
    public class WeightedName
    {
        [Key(0)] public float Weight { get; set; }
        [Key(1)] public bool Ignore { get; set; }
        [Key(2)] public string Name { get; set; }
        [Key(3)] public string Desc { get; set; }

        public WeightedName()
        {
            Weight = 1000;
            Ignore = false;
            Name = "";
            Desc = "";
        }
    }
}
