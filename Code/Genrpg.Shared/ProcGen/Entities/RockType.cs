using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils.Data;

namespace Genrpg.Shared.ProcGen.Entities
{



    /// <summary>
    /// Plants found on the ground used in Unity's grass terrain generator
    /// </summary>

    [MessagePackObject]
    public class RockType : IIndexedGameItem
    {
        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string Desc { get; set; }
        [Key(3)] public string Icon { get; set; }

        [Key(4)] public string Art { get; set; }

        [Key(5)] public float ChanceScale { get; set; }

        [Key(6)] public int MaxPerZone { get; set; }

        [Key(7)] public MyColorF BaseColor { get; set; }

        [Key(8)] public int MaxIndex { get; set; }

        public RockType()
        {
            ChanceScale = 1.0f;

            MaxPerZone = 0;
            BaseColor = new MyColorF();

            MaxIndex = 1;
        }

    }
}
