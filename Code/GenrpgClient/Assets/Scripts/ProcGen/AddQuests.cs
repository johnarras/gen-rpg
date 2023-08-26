using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;
using Services.ProcGen;
using System.Threading;

public class AddQuests : BaseZoneGenerator
{

    protected IQuestGenService _questGenService;
    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);

        _questGenService.GenerateQuests(gs);
    }

}