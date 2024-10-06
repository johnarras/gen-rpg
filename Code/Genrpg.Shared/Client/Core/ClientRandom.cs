using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Client.Core
{
    public interface IClientRandom : IRandom, IInjectable
    {
    }
    public class ClientRandom : MyRandom, IClientRandom
    {
        public ClientRandom() : base() { }

        public ClientRandom(long seed) : base(seed) { }
    }
}
