using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Utils;
using MongoDB.Driver;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Users.PlayerData;
using System.Runtime.InteropServices;
using Genrpg.Shared.GameSettings.PlayerData;

namespace Genrpg.RequestServer.Core
{
    public class WebContext : ServerGameState
    {
        public User user { get; set; } = null!;

        public MyRandom rand { get; set; } = new MyRandom();

        public List<IWebResult> Results { get; set; } = new List<IWebResult>();

        protected IRepositoryService _repoService = null!;

        // IFilteredObject code  

        public WebContext(IServerConfig config) : base(config)
        {

        }
        public WebContext(IServerConfig config, IServiceLocator locIn) : base(config)
        {
            loc = locIn;
            rand = new MyRandom();
            _repoService = locIn.Get<IRepositoryService>();
        }

        public async Task<User> LoadUser(string userId)
        {
            if (user == null)
            {
                user = await _repoService.Load<User>(userId);
                Set(user);
            }
            return user;
        }

        protected Dictionary<string, IUnitData> _unitData = new Dictionary<string, IUnitData>();

        public List<IUnitData> GetAllData() { return _unitData.Values.ToList(); }

        public void Set<T>(T doc) where T : class, IUnitData, new()
        {
            string id = doc.Id;
            if (doc is IId iid)
            {
                id = iid.IdKey.ToString();
            }
            _unitData.Add(GetFullKey<T>(id), doc);
        }

        string GetFullKey<T>(object idObj) where T : class, IUnitData, new()
        {
            return (typeof(T).Name + idObj.ToString()).ToLower();
        }

        public async Task<T> GetAsync<T>(long idkey) where T : class, IOwnerQuantityChild, new()
        {
            string ownerId = null;
            if (typeof(IUserData).IsAssignableFrom(typeof(T)))
            {
                ownerId = user.Id;
            }
            else if (!string.IsNullOrEmpty(user.CurrCharId))
            {
                ownerId = user.CurrCharId;
            }
            string fullKey = GetFullKey<T>(idkey.ToString());

            if (_unitData.ContainsKey(fullKey))
            {
                return (T)_unitData[fullKey];
            }
            List<T> items = await _repoService.Search<T>(x => x.OwnerId == ownerId && x.IdKey == idkey);

            if (items.Count > 1)
            {
                throw new Exception($"Duplicate player data Item for a given OwnerId {ownerId} and IdKey {idkey})");
            }

            T item = items.FirstOrDefault()!;
            if (item == null)
            {
                item = new T() { Id = HashUtils.NewGuid(), OwnerId = ownerId, IdKey = idkey };
            }

            Set(item);
            return item;
        }

        public async Task<T> GetAsync<T>(string id = null) where T : class, IUnitData, new()
        {
            if (string.IsNullOrEmpty(id))
            {
                if (typeof(IUserData).IsAssignableFrom(typeof(T)))
                {
                    id = user.Id;
                }
                else if (!string.IsNullOrEmpty(user.CurrCharId))
                {
                    id = user.CurrCharId;
                }
                else
                {
                    return default;
                }
            }

            string fullKey = GetFullKey<T>(id);

            if (_unitData.ContainsKey(fullKey))
            {
                return (T)_unitData[fullKey];
            }

            T item = await _repoService.Load<T>(id);

            if (item == null)
            {
                item = new T() { Id = id };
            }
            Set(item);
            return item;
        }
    }
}
