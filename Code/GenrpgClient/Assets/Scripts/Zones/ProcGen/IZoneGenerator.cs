
using System.Threading;
using UnityEngine;

public interface IZoneGenerator
{
    Awaitable Generate(CancellationToken token);
}
