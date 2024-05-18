using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Core.Interfaces
{
    public interface IInjectOnLoad<T> where T : IInjectable
    {
    }
}
