
using System.Threading.Tasks;
using System.Threading;
using Assets.Scripts.MapTerrain;
using UnityEngine; // Needed

public class SetBasicTerrainTextures : BaseZoneGenerator
{
    public class MaterialChosen
    {
        public int TextureTypeId;
        public int SplatmapIndex;
        public int ZoneTypeId;

        public Texture2D RegTexture;
        public Texture2D NormTexture;

    }

    public override async Task Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);

        // Set up splat prototypes based on the things given in the zone data, if any
        // and then fall back to the defaults if that fails.
        // And if all of those fail and anything loads, use the thing that loaded
        // for all channels to try to deal with errors.
        TerrainLayer[] layers = new TerrainLayer[MapConstants.MaxTerrainIndex];

        for (int s = 0; s < layers.Length; s++)
        {

            layers[s] = MapGenData.CreateTerrainLayer(gs.md.GetBasicTerrain(gs, s));
        }

        for (int gx = 0; gx < gs.map.BlockCount; gx++)
        {
            for (int gy = 0; gy < gs.map.BlockCount; gy++)
            {

                TerrainPatchData patch = gs.md.GetTerrainPatch(gs, gx, gy);
                if (patch == null)
                {
                    continue;
                }
                TerrainData tdata = patch.terrainData as TerrainData;
                if (tdata != null)
                {
                    tdata.terrainLayers = layers;
                }
            }
        }
    }
}
	
