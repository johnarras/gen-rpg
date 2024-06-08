using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.ProcGen.RandomNumbers
{
    public class ClientRandom : MyRandom, IClientRandom
    {
        public ClientRandom():base() { }

        public ClientRandom(long seed) : base(seed) { }
    }
}
