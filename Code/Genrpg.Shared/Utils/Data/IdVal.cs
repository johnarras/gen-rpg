using MessagePack;
namespace Genrpg.Shared.Utils.Data
{
    [MessagePackObject]
    public class IdVal
    {
        [Key(0)] public int Id { get; set; }
        [Key(1)] public int Val { get; set; }
    }
}
