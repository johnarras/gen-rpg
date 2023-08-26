using MessagePack;
using Genrpg.Shared.Characters.Entities;
using System.Collections.Generic;
using Genrpg.Shared.Login.Interfaces;

namespace Genrpg.Shared.Login.Messages.CreateChar
{
    [MessagePackObject]
    public class CreateCharResult : ILoginResult
    { 
        [Key(0)] public Character NewChar { get; set; }
        [Key(1)] public List<CharacterStub> AllCharacters { get; set; }
    }
}
