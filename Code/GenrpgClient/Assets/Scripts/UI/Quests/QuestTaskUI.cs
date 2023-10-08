using Genrpg.Shared.Quests.Entities;

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

        UIHelper.SetText(TaskText, _qtype.PrintTaskText(_gs, _gs.ch, _task.Index));

    }

}