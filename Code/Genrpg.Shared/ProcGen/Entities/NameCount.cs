using MessagePack;
namespace Genrpg.Shared.ProcGen.Entities
{
    [MessagePackObject]
    public class NameCount
    {
        [Key(0)] public string Name { get; set; }
        [Key(1)] public int Count { get; set; }
    }
}
