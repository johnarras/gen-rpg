using Genrpg.ServerShared.Core;
using Genrpg.Shared.AI.Entities;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.Factions.Constants;
using Genrpg.Shared.Factions.Entities;
using Genrpg.Shared.Input.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.SpellCrafting.Services;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.PlayerData
{
    public class PlayerDataService : IPlayerDataService
    {

        ISharedSpellCraftService _spellCraftingService = null;

        private Dictionary<Type, IUnitDataLoader> _loaderObjects = null;

        public async Task Setup(GameState gs, CancellationToken token)
        {
            List<IndexConfig> configs = new List<IndexConfig>();
            configs.Add(new IndexConfig() { Ascending = true, MemberName = "UserId" });
            await gs.repo.CreateIndex<Character>(configs);

            List<Type> loadTypes = ReflectionUtils.GetTypesImplementing(typeof(IUnitDataLoader));

            Dictionary<Type, IUnitDataLoader> newList = new Dictionary<Type, IUnitDataLoader>();
            foreach (Type lt in loadTypes)
            {
                if (Activator.CreateInstance(lt) is IUnitDataLoader newLoader)
                {
                    newList[newLoader.GetServerType()] = newLoader;
                    await newLoader.Setup(gs);
                }
            }
            _loaderObjects = newList;
        }

        public Dictionary<Type,IUnitDataLoader> GetLoaders()
        {
            return _loaderObjects;
        }

        public IUnitDataLoader GetLoader<T>() where T : IUnitData
        {
            if (_loaderObjects.TryGetValue(typeof(T), out IUnitDataLoader loader))
            {
                return loader;
            }
            return null;
        }

        public void SavePlayerData(Character ch, IRepositorySystem repoSystem, bool saveClean)
        {
            ch?.SaveAll(repoSystem, saveClean);
        }

        public async Task<List<IUnitData>> MapToClientApi(List<IUnitData> serverDataList)
        {
            List<IUnitData> retval = new List<IUnitData>();

            foreach (IUnitData serverData in serverDataList)
            {
                if (_loaderObjects.TryGetValue(serverData.GetType(), out IUnitDataLoader loader))
                {
                    if (loader.SendToClient())
                    {
                        retval.Add(loader.MapToAPI(serverData));
                    }
                }
            }
            await Task.CompletedTask;
            return retval;
        }

        public async Task<List<IUnitData>> LoadPlayerData(ServerGameState gs, Character ch)
        {
            List<Task<IUnitData>> allTasks = new List<Task<IUnitData>>();
            foreach (IUnitDataLoader loader in _loaderObjects.Values)
            {
                allTasks.Add(LoadOrCreateData(loader, gs.repo, ch));
            }

            IUnitData[] allData = await Task.WhenAll(allTasks.ToList());

            foreach (IUnitData data in allData)
            {
                data.AddTo(ch);
            }

            UpdateOnLoad(gs, ch);
            return allData.ToList();
        }

        protected async Task<IUnitData> LoadOrCreateData(IUnitDataLoader loader, IRepositorySystem repoSystem, Character ch)
        {
            IUnitData newData = await loader.LoadData(repoSystem, ch);
            if (newData == null)
            {
                newData = loader.Create(ch);
            }
            return newData;
        }

        protected void UpdateOnLoad(ServerGameState gs, Character ch)
        {
            ch.FactionTypeId = FactionTypes.Player;
            ch.BaseSpeed = gs.data.GetGameData<AISettings>(ch).BaseUnitSpeed;
            ch.Speed = ch.BaseSpeed;
            ch.RemoveFlag(UnitFlags.Evading);
            ch.EntityTypeId = EntityTypes.Unit;
            ch.EntityId = 1;

            SpellData spellData = ch.Get<SpellData>();
            for (int i = 1; i <= 3; i++)
            {
                Spell mySpell = spellData.Get(i);
                if (mySpell == null)
                {
                    Spell newSpell = SerializationUtils.ConvertType<SpellType, Spell>(gs.data.GetGameData<SpellTypeSettings>(ch).GetSpellType(i));
                    newSpell.Id = HashUtils.NewGuid();
                    newSpell.OwnerId = ch.Id;

                    spellData.Add(newSpell);
                    gs.repo.QueueSave(newSpell);
                }

                ActionInputData adata = ch.Get<ActionInputData>();

                ActionInput ai = adata.GetData().FirstOrDefault(x => x.SpellId == i);
                if (ai == null)
                {
                    adata.SetInput(i, i);
                }
                else
                {
                    ai.Index = i;
                }
                ch.SetDirty(true);
                adata.SetDirty(true);
            }

            foreach (Spell spell in spellData.GetData())
            {
                _spellCraftingService.ValidateSpellData(gs, ch, spell);
            }

        }

        public async Task<List<CharacterStub>> LoadCharacterStubs(ServerGameState gs, string userId)
        {
            // TODO: projection in the repo itself
            List<Character> chars = await gs.repo.Search<Character>(x => x.UserId == userId);

            List<CharacterStub> stubs = new List<CharacterStub>();
            foreach (Character ch in chars)
            {
                stubs.Add(new CharacterStub()
                {
                    Id = ch.Id,
                    Name = ch.Name,
                    Level = ch.Level,
                });
            }

            return stubs;
        }

    }
}
