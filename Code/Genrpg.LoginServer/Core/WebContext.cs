using Genrpg.LoginServer.Maps;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Interfaces;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing.Text;
using System.Linq;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.Core
{
    public class WebContext : ServerGameState
    {
        public User user { get; set; }
        
        public MyRandom rand { get; set; } = new MyRandom();

        public List<IWebResult> Results { get; set; } = new List<IWebResult>();

        protected IRepositoryService _repoService;
        public WebContext(IServerConfig config) : base(config)
        {

        }
        public WebContext(IServerConfig config, IServiceLocator locIn) : base(config)
        {
            loc = locIn;
            rand = new MyRandom();
            _repoService = locIn.Get<IRepositoryService>(); 
        }

        private List<IStringId> _docsSeen = new List<IStringId>();
        protected Dictionary<string, IUnitData> _unitData = new Dictionary<string, IUnitData>();

        public List<IStringId> GetAllData() { return _docsSeen.ToList(); }  
        
        public void Add<T>(T doc) where T : class, IStringId, new()
        {
            _docsSeen.Add(doc);
        }

        public async Task<T> GetAsync<T>(long idkey) where T : class, IUnitData, IStringOwnerId, IId, new()
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
            string fullKey = typeof(T).Name.ToLower() + "-" + ownerId.ToLower() + "-" + idkey;

            if (_unitData.ContainsKey(fullKey))
            {
                return (T)_unitData[fullKey];
            }
            List<T> items = await _repoService.Search<T>(x => x.OwnerId == ownerId && x.IdKey == idkey);

            if (items.Count > 1)
            {
                throw new Exception($"Duplicate player data Item for a given OwnerId {ownerId} and IdKey {idkey})");
            }

            T item = items.FirstOrDefault();
            if (item == null)
            {
                item = new T() { Id = HashUtils.NewGuid(), OwnerId = ownerId, IdKey = idkey };
                _unitData.Add(fullKey, item);
                _docsSeen.Add(item);
            }
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
                    return default(T);
                }               
            }

            string fullKey = typeof(T).Name.ToLower() + "-" + id.ToLower();

            if (_unitData.ContainsKey(fullKey))
            {
                return (T)_unitData[fullKey];
            }

            T item = await _repoService.Load<T>(id);

            if (item == null)
            {
                item = new T() { Id = id };
                _unitData.Add(fullKey, item);
                _docsSeen.Add(item);
            }
            return item;
        }
    }
}
