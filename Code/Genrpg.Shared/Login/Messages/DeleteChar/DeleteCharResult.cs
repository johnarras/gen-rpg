using MessagePack;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.Characters.PlayerData;

namespace Genrpg.Shared.Login.Messages.DeleteChar
{
    [MessagePackObject]
    public class DeleteCharResult : ILoginResult
    {
        [Key(0)] public List<CharacterStub> AllCharacters { get; set; }
    }
}
