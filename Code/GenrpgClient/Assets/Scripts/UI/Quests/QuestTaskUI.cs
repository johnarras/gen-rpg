using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.Quests.Entities;
using Genrpg.Shared.Quests.WorldData;

public class QuestTaskUI : BaseBehaviour
{

    protected IMapProvider _mapProvider;

    public GText TaskText;

    private QuestType _qtype = null;
    private QuestTask _task = null;

    public void Init(QuestType qtype, QuestTask task)
    {
        if (qtype == null || task == null)
        {
            _clientEntityService.Destroy(entity);
            return;
        }

        _qtype = qtype;
        _task = task;

        ShowStatus();
    }

    public void ShowStatus()
    {
        if (_qtype == null || _task == null)
        {
            return;
        }

        _uiService.SetText(TaskText, _qtype.PrintTaskText(_rand, _gs.ch, _gameData, _mapProvider, _task.Index));

    }

}