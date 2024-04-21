using Genrpg.Shared.Quests.Entities;
using Genrpg.Shared.Quests.WorldData;

public class QuestTaskUI : BaseBehaviour
{
    public GText TaskText;

    private QuestType _qtype = null;
    private QuestTask _task = null;

    public void Init(QuestType qtype, QuestTask task)
    {
        if (qtype == null || task == null)
        {
            GEntityUtils.Destroy(entity);
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

        _uIInitializable.SetText(TaskText, _qtype.PrintTaskText(_gs, _gs.ch, _gameData, _task.Index));

    }

}