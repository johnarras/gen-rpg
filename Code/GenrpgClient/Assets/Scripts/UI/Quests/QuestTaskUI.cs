using UnityEngine;
using UnityEngine.UI;
using Genrpg.Shared.Quests.Entities;

public class QuestTaskUI : BaseBehaviour
{

    [SerializeField]
    private Text _taskText;

    private QuestType _qtype = null;
    private QuestTask _task = null;
    public void Init(QuestType qtype, QuestTask task)
    {
        if (qtype == null || task == null)
        {
            Destroy(gameObject);
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

        UIHelper.SetText(_taskText, _qtype.PrintTaskText(_gs, _gs.ch, _task.Index));

    }

}