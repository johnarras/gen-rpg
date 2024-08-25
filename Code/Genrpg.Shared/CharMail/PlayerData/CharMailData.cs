using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.CharMail.Constants;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using MessagePack;
using Genrpg.Shared.DataStores.Interfaces;

namespace Genrpg.Shared.CharMail.PlayerData
{
    [MessagePackObject]
    public class CharLetter : OwnerPlayerData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public long CharLetterTypeId { get; set; }
    }


    [MessagePackObject]
    public class CharMailData : OwnerObjectList<CharLetter>, IServerOnlyData
    {
        [Key(0)] public override string Id { get; set; }
    }
    [MessagePackObject]
    public class CharMailApi : OwnerApiList<CharMailData, CharLetter> { }
    [MessagePackObject]
    public class CrafterDataLoader : OwnerDataLoader<CharMailData, CharLetter> { }


    [MessagePackObject]
    public class CrafterDataMapper : OwnerDataMapper<CharMailData, CharLetter, CharMailApi> { }
}
