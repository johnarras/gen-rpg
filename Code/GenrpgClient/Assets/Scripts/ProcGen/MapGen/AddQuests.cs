using Cysharp.Threading.Tasks;
using System.Threading;

public class AddQuests : BaseZoneGenerator
{

    protected IQuestGenService _questGenService;
    public override async UniTask Generate(CancellationToken token)
    {
        await base.Generate(token);

        _questGenService.GenerateQuests(_gs);
    }

}