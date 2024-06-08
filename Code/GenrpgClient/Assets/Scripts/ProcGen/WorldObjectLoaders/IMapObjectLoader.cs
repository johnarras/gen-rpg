using Cysharp.Threading.Tasks;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Messages;
using System.Threading;

public interface IMapObjectLoader : ISetupDictionaryItem<long>
{
    UniTask Load(OnSpawn message, MapObject loadedObject, CancellationToken token);
}