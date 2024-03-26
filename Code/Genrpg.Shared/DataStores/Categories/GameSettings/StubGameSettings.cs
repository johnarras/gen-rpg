using MessagePack;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.DataStores.Entities;

namespace Genrpg.Shared.DataStores.Categories.GameSettings
{
    [MessagePackObject]
    public class StubGameSettings : IGameSettings
    {
        [Key(0)] public string Id { get; set; }

        public virtual void SetInternalIds() { }
        public virtual void AddTo(GameData gameData) { }
        public void ClearIndex() { }

        public virtual async Task SaveAll(IRepositoryService repo)
        {
            await Task.CompletedTask;
            return;
        }

        public virtual List<IGameSettings> GetChildren() { return new List<IGameSettings>(); }

    }
}
