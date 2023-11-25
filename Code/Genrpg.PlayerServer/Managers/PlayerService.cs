using Genrpg.PlayerServer.Entities;
using Genrpg.ServerShared.CloudComms.Requests.Entities;
using Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues;
using Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Requests;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logs.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.PlayerServer.Managers
{

    public interface IPlayerService : ISetupService
    {
        List<OnlineCharacter> GetPlayersInMap(string mapId);
        List<OnlineCharacter> GetPlayersInZone(string mapId, long zoneId);
        List<OnlineCharacter> GetPlayersInMapInstance(string mapInstanceId);
        void OnLoginUser(ServerGameState gs, LoginUser login);
        void OnLogoutUser(ServerGameState gs, LogoutUser logout);
        void OnPlayerEnterMap(ServerGameState gs, PlayerEnterMap playerEnterMap);
        void OnPlayerLeaveMap(ServerGameState gs, PlayerLeaveMap playerLeaveMap);
        void OnPlayerEnterZone(ServerGameState gs, PlayerEnterZone playerEnterZone);
        IResponse GetWhoList(ServerGameState gs, WhoListRequest request);
    }

    public class PlayerService : IPlayerService
    {
        private ConcurrentDictionary<string, LoggedInUser> _users = new ConcurrentDictionary<string, LoggedInUser>();
        private ConcurrentDictionary<string, List<OnlineCharacter>> _mapChars = new ConcurrentDictionary<string, List<OnlineCharacter>>();
        private ConcurrentDictionary<string, List<OnlineCharacter>> _mapInstanceChars = new ConcurrentDictionary<string, List<OnlineCharacter>>();
        private ConcurrentDictionary<string, List<OnlineCharacter>> _zoneChars = new ConcurrentDictionary<string, List<OnlineCharacter>>();
        private ConcurrentDictionary<string, OnlineCharacter> _onlineChars = new ConcurrentDictionary<string, OnlineCharacter>();


        private ILogSystem _logger;
        public async Task Setup(GameState gs, CancellationToken token)
        {
            _logger = gs.logger;
            await Task.CompletedTask;
        }
        protected string GetMapZoneKey(string mapId, long zoneId)
        {
            return mapId + zoneId;
        }

        public List<OnlineCharacter> GetPlayersInMap(string mapId)
        {
            return _mapChars.GetOrAdd(mapId, new List<OnlineCharacter>());
        }

        public List<OnlineCharacter> GetPlayersInZone(string mapId, long zoneId)
        {
            return _zoneChars.GetOrAdd(GetMapZoneKey(mapId, zoneId), new List<OnlineCharacter>());
        }

        public List<OnlineCharacter> GetPlayersInMapInstance(string mapInstanceId)
        {
            return _mapInstanceChars.GetOrAdd(mapInstanceId, new List<OnlineCharacter>());
        }

        public void OnLoginUser(ServerGameState gs, LoginUser login)
        {
            _logger.Message("LoginUser: " + login.Id + ": " + login.Name);
            if (_users.TryGetValue(login.Id, out LoggedInUser user))
            {
                return;
            }
            user = new LoggedInUser()
            {
                Id = login.Id,
                Name = login.Name,
                LastUpdateTime = DateTime.Now,
            };
        }

        public void OnLogoutUser(ServerGameState gs, LogoutUser logout)
        {
            _logger.Message("Logout user: " + logout.Id);
            _users.TryRemove(logout.Id, out LoggedInUser user);
        }

        protected void RemoveFromMap(OnlineCharacter currChar)
        {
            if (!string.IsNullOrEmpty(currChar.MapId))
            {
                if (_mapChars.TryGetValue(currChar.MapId, out List<OnlineCharacter> mapChars))
                {
                    mapChars.Remove(currChar);
                }
            }

            if (!string.IsNullOrEmpty(currChar.MapInstanceId))
            {
                if (_mapInstanceChars.TryGetValue(currChar.MapInstanceId, out List<OnlineCharacter> mapInstanceChars))
                {
                    mapInstanceChars.Remove(currChar);
                }
            }

            RemoveFromZone(currChar);
        }

        protected void RemoveFromZone(OnlineCharacter currChar)
        {
            if (currChar.ZoneId > 0)
            {
                if (_zoneChars.TryGetValue(GetMapZoneKey(currChar.MapId, currChar.ZoneId), out List<OnlineCharacter> zoneChars))
                {
                    _logger.Message("RemoveFromZone: " + currChar.Id);
                    zoneChars.Remove(currChar);
                }
            }
        }

        public void OnPlayerEnterMap(ServerGameState gs, PlayerEnterMap playerEnterMap)
        {
            _logger.Message("PlayerEnterMap: " + playerEnterMap.Id + " Map: " + playerEnterMap.MapId + " Instance: " +
                playerEnterMap.InstanceId);
            if (_onlineChars.TryGetValue(playerEnterMap.Id, out OnlineCharacter currChar))
            {
                RemoveFromMap(currChar);
            }

            if (currChar == null)
            {
                currChar = new OnlineCharacter()
                {
                    Id = playerEnterMap.Id,
                    Name = playerEnterMap.Name,
                    Level = playerEnterMap.Level,
                    MapId = playerEnterMap.MapId,
                    MapInstanceId = playerEnterMap.InstanceId,
                    UserId = playerEnterMap.UserId,
                };
                _onlineChars.TryAdd(currChar.Id, currChar);
            }


            List<OnlineCharacter> newMapChars = _mapChars.GetOrAdd(playerEnterMap.MapId, new List<OnlineCharacter>());
            newMapChars.Add(currChar);

            List<OnlineCharacter> newMapInstanceChars = _mapInstanceChars.GetOrAdd(playerEnterMap.InstanceId, new List<OnlineCharacter>());
            newMapInstanceChars.Add(currChar);
        }

        public void OnPlayerLeaveMap(ServerGameState gs, PlayerLeaveMap playerLeaveMap)
        {
            _logger.Message("PlayerLeaveMap: " + playerLeaveMap.Id);
            if (_onlineChars.TryGetValue(playerLeaveMap.Id, out OnlineCharacter currChar))
            {
                RemoveFromMap(currChar);
            }
        }

        public void OnPlayerEnterZone(ServerGameState gs, PlayerEnterZone playerEnterZone)
        {
            _logger.Message("AddToZone: " + playerEnterZone.Id + " to " + playerEnterZone.ZoneId);
            if (_onlineChars.TryGetValue(playerEnterZone.Id, out OnlineCharacter currChar))
            {
                RemoveFromZone(currChar);
                currChar.ZoneId = playerEnterZone.ZoneId;
                List<OnlineCharacter> zoneChars = _zoneChars.GetOrAdd(GetMapZoneKey(currChar.MapId, currChar.ZoneId), new List<OnlineCharacter>());
                zoneChars.Add(currChar);
            }
        }

        public IResponse GetWhoList(ServerGameState gs, WhoListRequest request)
        {
            List<OnlineCharacter> onlineChars = _onlineChars.Values.ToList();

            WhoListResponse response = new WhoListResponse();

            foreach (OnlineCharacter onlineChar in onlineChars)
            {
                response.Chars.Add(new WhoListChar()
                {
                    Id = onlineChar.Id,
                    Name = onlineChar.Name,
                    Level = onlineChar.Level,
                    ZoneName = onlineChar.ZoneId.ToString(),
                });
            }
            return response;
        }
    }
}
