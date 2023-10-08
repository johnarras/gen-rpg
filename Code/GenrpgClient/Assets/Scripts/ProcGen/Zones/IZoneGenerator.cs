using System.Threading.Tasks;
using System.Threading;

public interface IZoneGenerator
{
    Task Generate(UnityGameState gs, CancellationToken token);
}
