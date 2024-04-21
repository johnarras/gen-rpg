
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using System.Threading;
using System.Threading.Tasks;
public interface INoiseService : IInitializable
{
    float[,] Generate(GameState gs, double pers, double freq, double amp,
                                int octaves, long seed, int width, int height, double lacunarity = 2.0f);
}

public class NoiseService : INoiseService
{
    public async Task Initialize(GameState gs, CancellationToken token)
    {
        await Task.CompletedTask;
    }

    public float[,] Generate(GameState gs, double pers, double freq, double amp,
                                int octaves, long seed, int width, int height, double lacunarity = 2.0f)
    {
        NoiseGen perlinNoise = new NoiseGen();
        return perlinNoise.Generate(gs, pers, freq, amp, octaves, seed, width, height, (float)lacunarity);
    }
}