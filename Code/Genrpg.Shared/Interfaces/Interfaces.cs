
using System;
using System.Collections.Generic;
using Genrpg.Shared.Core.Entities;
using System.Threading;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.PlayerData;
using System.Threading.Tasks;

namespace Genrpg.Shared.Interfaces
{

    public delegate void VoidDelegate();
    public delegate void ObjectDelegate(object obj);
    public delegate void StringDelegate(string s);
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

    public interface IMapOwnerId : IStringOwnerId
    {
        string MapId { get; set; }
    }

    public interface IOwnerQuantityChild : IStringOwnerId, IChildUnitData, IId
    {
        long Quantity { get; set; }
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

    public interface IExtraDescItem
    {
        string GetExtraDesc(IGameData gameData);
    }

    public interface IOrderedItem
    {
        long GetOrder();
    }

    public interface IVariationIndexedGameItem : IIndexedGameItem
    {
        int VariationCount { get; set; }
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
        T Get<T>() where T : IInjectable;
        void Set<T>(T t) where T : IInjectable;
        List<Type> GetKeys();

        List<IInjectable> GetVals();
        void Resolve(object obj);
        void StoreDictionaryItem(object obj);
        Task InitializeDictionaryItems(CancellationToken token);
        void ResolveSelf();      
    }
}
