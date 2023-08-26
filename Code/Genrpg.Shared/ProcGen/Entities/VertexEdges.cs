using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.ProcGen.Entities
{
    [MessagePackObject]
    public class VertexEdges
    {
        [Key(0)] public int Id { get; set; }
        [Key(1)] public int PosId { get; set; }
        [Key(2)] public int X { get; set; }

        [Key(3)] public int Y { get; set; }

        [Key(4)] public List<int> AdjacentVerts { get; set; }

        public VertexEdges()
        {
            AdjacentVerts = new List<int>();
        }
    }
}
