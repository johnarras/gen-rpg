using MessagePack;
using Genrpg.Shared.Characters.Entities;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Login.Interfaces;

namespace Genrpg.Shared.Login.Messages.DeleteChar
{
    [MessagePackObject]
    public class DeleteCharResult : ILoginResult
    {
        [Key(0)] public List<CharacterStub> AllCharacters { get; set; }
    }
}
