using System.Threading.Tasks;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Messages;
using System.Threading;

public interface IMapObjectLoader : ISetupDictionaryItem<long>
{
    Task Load(UnityGameState gs, OnSpawn message, MapObject loadedObject, CancellationToken token);
}