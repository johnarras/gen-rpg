using System.Threading.Tasks;
using System.Threading;

public class AddQuests : BaseZoneGenerator
{

    protected IQuestGenService _questGenService;
    public override async Task Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);

        _questGenService.GenerateQuests(gs);
    }

}