using MessagePack;
namespace Genrpg.Shared.Utils.Data
{
    [MessagePackObject]
    public class IdLongVal
    {
        [Key(0)] public int Id { get; set; }
        [Key(1)] public long Val { get; set; }
    }
}
