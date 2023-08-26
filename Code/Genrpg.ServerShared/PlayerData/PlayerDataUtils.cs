using Genrpg.ServerShared.Core;
using Genrpg.Shared.AI.Entities;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Factions.Entities;
using Genrpg.Shared.Input.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.PlayerData
{
    public class PlayerDataUtils
    {
        private static Dictionary<Type, IUnitDataLoader> _loaderObjects = null;

        private static async Task SetupLoaders(ServerGameState gs)
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

        public static Dictionary<Type,IUnitDataLoader> GetLoaders()
        {
            return _loaderObjects;
        }

        public static void SavePlayerData(Character ch, IRepositorySystem repoSystem, bool saveClean)
        {
            ch?.SaveAll(repoSystem, saveClean);
        }

        public static async Task<List<IUnitData>> MapToClientApi(List<IUnitData> serverDataList)
        {
            List<IUnitData> retval = new List<IUnitData>();

            foreach (IUnitData serverData in serverDataList)
            {
                if (_loaderObjects.TryGetValue(serverData.GetType(), out IUnitDataLoader loader))
                {
                    if (loader.ShouldSendToClient())
                    {
                        retval.Add(loader.MapToAPI(serverData));
                    }
                }
            }
            await Task.CompletedTask;
            return retval;
        }

        public static async Task<List<IUnitData>> LoadPlayerData(ServerGameState gs, Character ch)
        {
            await SetupLoaders(gs);
            List<IUnitData> retval = new List<IUnitData>();
            foreach (IUnitDataLoader loader in _loaderObjects.Values)
            {
                IUnitData newData = await loader.LoadData(gs.repo, ch);
                if (newData == null)
                {
                    newData = loader.Create(ch);
                }
                newData.AddTo(ch);
                retval.Add(newData);
            }
            UpdateOnLoad(gs, ch);
            return retval;
        }

        protected static void UpdateOnLoad(ServerGameState gs, Character ch)
        {
            ch.FactionTypeId = FactionType.Player;
            ch.BaseSpeed = gs.data.GetGameData<AISettings>().BaseUnitSpeed;
            ch.Speed = ch.BaseSpeed;
            ch.RemoveFlag(UnitFlags.Evading);
            ch.EntityTypeId = EntityType.Unit;
            ch.EntityId = 1;

            SpellData spellData = ch.Get<SpellData>();
            for (int i = 1; i <= 3; i++)
            {
                Spell mySpell = spellData.Get(i);
                if (mySpell == null)
                {
                    spellData.Add(gs.data.GetGameData<SpellSettings>().GetSpell(i));
                }

                ActionInputData adata = ch.Get<ActionInputData>();

                ActionInput ai = adata.Data.FirstOrDefault(x => x.SpellId == i);
                if (ai == null)
                {
                    adata.Data.Add(new ActionInput() { Index = i, SpellId = i });
                }
                else
                {
                    ai.Index = i;
                }
                ch.SetDirty(true);
                adata.SetDirty(true);
            }
        }

        public static TClient SimpleClientConvert<TServer, TClient>(TServer serverObject)
        {
            string txt = SerializationUtils.Serialize(serverObject);
            return SerializationUtils.Deserialize<TClient>(txt);
        }

        public static async Task<List<CharacterStub>> LoadCharacterStubs(ServerGameState gs, string userId)
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
