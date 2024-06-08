using Cysharp.Threading.Tasks;
using System.Threading;

public interface IZoneGenerator
{
    UniTask Generate(CancellationToken token);
}
