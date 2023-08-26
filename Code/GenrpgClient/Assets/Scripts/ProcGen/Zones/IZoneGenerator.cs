using Cysharp.Threading.Tasks;
using Genrpg.Shared.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public interface IZoneGenerator
{
    UniTask Generate(UnityGameState gs, CancellationToken token);
}
