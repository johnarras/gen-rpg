
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Messages;
using System.Threading;
using UnityEngine;

public interface IMapObjectLoader : ISetupDictionaryItem<long>
{
    Awaitable Load(OnSpawn message, MapObject loadedObject, CancellationToken token);
}