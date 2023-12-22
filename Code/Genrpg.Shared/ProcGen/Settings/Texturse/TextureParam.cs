using MessagePack;
namespace Genrpg.Shared.ProcGen.Settings.Texturse
{
    [MessagePackObject]
    public class TextureParam
    {
        [Key(0)] public string Name { get; set; }
        [Key(1)] public float MinVal { get; set; }
        [Key(2)] public float MaxVal { get; set; }
        [Key(3)] public bool IsInteger { get; set; }
    }
}
