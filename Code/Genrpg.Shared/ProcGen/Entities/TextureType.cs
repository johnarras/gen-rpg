using MessagePack;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.ProcGen.Entities
{
    [MessagePackObject]
    public class TextureType : IIndexedGameItem
    {
        public const int None = 0;
        public const int Grass = 1;
        public const int Dirt = 2;
        public const int Rock = 3;
        public const int Road = 4;
        public const int Sand = 5;
        public const int Forest = 6;
        public const int Swamp = 7;
        public const int Cracked = 8;
        public const int WastelandRock = 9;
        public const int SandRock = 10;
        public const int Wasteland = 11;
        public const int Path = 12;
        public const int Water = 13;
        public const int Hills = 14;


        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string Desc { get; set; }
        [Key(3)] public string Icon { get; set; }

        [Key(4)] public string Art { get; set; }

        [Key(5)] public float Size { get; set; }

        [Key(6)] public long ParentTextureTypeId { get; set; }


        public TextureType()
        {
        }
    }
}
