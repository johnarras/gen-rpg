
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Players.Interfaces;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Characters.Entities;
using System.Threading;
using System.Dynamic;

namespace Genrpg.Shared.Interfaces
{

    public delegate void VoidDelegate();
    public delegate void ObjectDelegate(object obj);
    public delegate void StringDelegate(string s);
    public delegate void GameStateObjectDelegate(GameState gs, object obj);
    public delegate void TokenDelegate(CancellationToken token);


    public interface IRealtimeGame : IStringId
    {
    }

    public interface IStringId
    {
        string Id { get; set; }
    }

    public interface IStringOwnerId : IStringId
    {
       string OwnerId { get; set; }
    }

    public interface IId
    {
        long IdKey { get; set; }
    }

    public interface IDbId
    {
        long Id { get; set; }
    }
    public interface IName
    {
        string Name { get; set; }
    }

    public interface IInfo
    {

        long GetId();
        string ShowInfo();
    }

    public interface IIdName : IId, IName
    {

    }

    public interface IIndexedGameItem : IIdName
    {
        string Desc { get; set; }
        string Icon { get; set; }
        string Art { get; set; }
    }

    public interface INameId
    {
        string NameId { get; set; }
    }

    public interface IMusicRegion
    {
        long MusicTypeId { get; set; }
        long AmbientMusicTypeId { get; set; }
    }



    public interface ISpellHit
    {
        string UnitId { get; set; }
        DateTime LastHitTime { get; set; }
        int NumHits { get; set; }
    }


    public interface IServiceLocator
    {
        T Get<T>() where T : IService;
        object GetByName(string txt);
        void Set<T>(T t) where T : IService;
        List<Type> GetKeys();

        List<IService> GetVals();
        void Remove<T>() where T : IService;
        void Resolve(object obj);
        void ResolveSelf();
    }
}
