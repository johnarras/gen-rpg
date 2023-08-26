using MessagePack;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Currencies.Entities
{
    [MessagePackObject]
    public class CurrencyStatus : IStatusItem, IId
    {
        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public long Quantity { get; set; }
    }


}
