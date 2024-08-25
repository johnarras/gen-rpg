using MessagePack;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.BoardGame.PlayerData
{
    [MessagePackObject]
    public class NextBoardData : IUnitData
    {
        [Key(0)] public BoardData NextBoard { get; set; }
        [Key(1)] public string Id { get; set; }
        public void AddTo(Unit unit) { }
        public void QueueDelete(IRepositoryService service)
        {
        }

        public void QueueSave(IRepositoryService repoService)
        {
        }
    }
}
