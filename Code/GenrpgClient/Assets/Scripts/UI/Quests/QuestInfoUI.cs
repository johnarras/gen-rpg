using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using Genrpg.Shared.Core.Entities;


using Services;
using UnityEngine.UI;
using TMPro;
using ClientEvents;
using Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Quests.Entities;
using Genrpg.Shared.Quests.Services;
using Genrpg.Shared.NPCs.Entities;
using Genrpg.Shared.Currencies.Entities;
using System.Threading;

public class QuestInfoUI : BaseBehaviour
{

    protected ISharedQuestService _questService;

    public const string ScreenTaskPrefab = "QuestScreenTaskRow";
    public const string HUDTaskPrefab = "QuestHUDTaskRow";

    [SerializeField]
    private GameObject _contentParent;

    [SerializeField]
    private Image _statusIcon;
    [SerializeField]
    private Text _name;
    [SerializeField]
    private Text _description;
    [SerializeField]
    private GameObject _taskParent;

    [SerializeField]
    private Text _experience;
    [SerializeField]
    private MoneyDisplay _money;
    [SerializeField]
    private GameObject _otherRewards;
    [SerializeField]
    private Text _itemRewardText;

    [SerializeField]
    private bool _showWord = false;

    [SerializeField]
    private GameObject _acceptButton;
    [SerializeField]
    private GameObject _abandonButton;
    [SerializeField]
    private GameObject _completeButton;

    QuestType _qtype = null;
    QuestScreen _screen = null;
    NPCType _type = null;
    private CancellationToken _token;
    public void Init(QuestType qtype, int index, QuestScreen screen, NPCType npcType, CancellationToken token)
    {
        _gs.AddEvent<UpdateQuestEvent>(this, OnUpdateQuest);
        _token = token;
        _qtype = qtype;
        _screen = screen;
        _type = npcType;
        ShowQuestInfo();
    }

    protected string GetTaskRowPrefabName()
    {
        return (_screen != null ? ScreenTaskPrefab : HUDTaskPrefab);
    }

    protected virtual void ShowQuestInfo()
    {
        GameObjectUtils.SetActive(_contentParent, _qtype != null);
        if (_qtype == null)
        {            
            return;
        }
        string rowPrefabName = GetTaskRowPrefabName();

        int state = _questService.GetQuestState(_gs, _gs.ch, _qtype);

        string nameText = _qtype.Name;

        if (_showWord)
        {
            if (state == QuestState.Complete)
            {
                nameText = "(Complete) " + nameText;
            }
        }

        UpdateButtonsFromState(state);


        UIHelper.SetText(_name, nameText);
        UIHelper.SetText(_description, _qtype.Desc);

        GameObjectUtils.DestroyAllChildren(_taskParent);
        if (_qtype.Tasks != null && _taskParent != null)
        {
            GameObjectUtils.DestroyAllChildren(_taskParent);
            foreach (QuestTask task in _qtype.Tasks)
            {
                _assetService.LoadAssetInto(_gs, _taskParent, AssetCategory.UI, rowPrefabName, OnLoadTask, task, _token);
            }
        }


        List<SpawnResult> rewards = _questService.GetRewards(_gs, _qtype, false);

        SpawnResult expReward = rewards.FirstOrDefault(x => x.EntityTypeId == EntityType.Currency && x.EntityId == CurrencyType.Exp);
        if (expReward != null)
        {
            UIHelper.SetText(_experience, "XP: " + expReward.Quantity.ToString());
        }
        SpawnResult moneyReward = rewards.FirstOrDefault(x => x.EntityTypeId == EntityType.Currency && x.EntityId == CurrencyType.Money);
        if (moneyReward != null && _money != null)
        {
            _money.SetMoney(moneyReward.Quantity);
        }

        List<SpawnResult> itemRewards = rewards.Where(X => X.EntityTypeId == EntityType.Item && (X.Data as Item) != null).ToList();


        UIHelper.SetText(_itemRewardText, "");
        if (itemRewards.Count > 0)
        {

            foreach (SpawnResult ireward in itemRewards)
            {
                Item item = ireward.Data as Item;
                if (item != null)
                {
                    InitItemIconData idata = new InitItemIconData()
                    {
                        Data = item,
                        entityTypeId = EntityType.Item,
                        entityId = item.UseEntityId,
                    };
                    IconHelper.InitItemIcon(_gs, idata, _otherRewards, this._assetService, _token);
                }
            }
        }
        else if (_qtype.ItemQuantity > 0)
        {
            string qualityString = "";

            if (_qtype.ItemQualityTypeId > 0)
            {
                QualityType qualityType = _gs.data.GetGameData<ItemSettings>().GetQualityType(_qtype.ItemQualityTypeId);
                if (qualityType != null)
                {
                    qualityString = " " + qualityType.Name;
                }
            }
            string txt = "Create " + _qtype.ItemQuantity + "" + qualityString + " Item" + (_qtype.ItemQuantity==1?"":"s");
            UIHelper.SetText(_itemRewardText, txt);
        }
    }

    public void ShowFullQuestData()
    {
        if (_screen != null)
        {
            _screen.ShowFullQuestData(_qtype);
        }
    }

    private UpdateQuestEvent OnUpdateQuest(UnityGameState gs, UpdateQuestEvent data)
    {
        if (data == null)
        {
            return null;
        }

        QuestStatus questStatus = data.Status;

        if (questStatus == null)
        {
            return null;
        }

        QuestType quest = questStatus.Quest;

        if (quest == null)
        {
            return null;
        }

        if (_qtype == null)
        {
            return null;
        }

        if (!_qtype.IsSameQuest(quest))
        {
            return null;
        }

        int state = _questService.GetQuestState(gs, gs.ch, quest);

        if (state == QuestState.Complete)
        {
            _qtype = null;
        }
        else
        {
            _qtype = quest;
        }
        ShowQuestInfo();

        return null;
    }

    private void OnLoadTask (UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        GameObject go = obj as GameObject;
        if (go == null)
        {
            return;
        }

        QuestTask task = data as QuestTask;

        if (task == null)
        {
            GameObject.Destroy(go);
            return;
        }

        QuestTaskUI taskRow = go.GetComponent<QuestTaskUI>();

        if (taskRow == null)
        {
            GameObject.Destroy(go);
            return;
        }

        taskRow.Init(_qtype, task);
    }

    public void UpdateButtonsFromState(int state)
    {
        GameObjectUtils.SetActive(_acceptButton, false);
        GameObjectUtils.SetActive(_abandonButton, false);
        GameObjectUtils.SetActive(_completeButton, false);

        if (state == QuestState.Available)
        {
            GameObjectUtils.SetActive(_acceptButton, true);
        }
        else if (state == QuestState.Active)
        {
            GameObjectUtils.SetActive(_abandonButton, true);
        }
        else if (state == QuestState.Complete)
        {
            GameObjectUtils.SetActive(_completeButton, true);
        }
    }

    public void ClickAbandon()
    {
        AlterQuest(AlterQuestType.Abandon);
    }

    public void ClickAccept()
    {
        AlterQuest(AlterQuestType.Accept);

    }

    public void ClickComplete()
    {
        AlterQuest(AlterQuestType.Complete);
    }

    private void AlterQuest(int alterQuestTypeId)
    {
        if (_qtype == null)
        {
            return;
        }

        AlterQuestStateData alterData = new AlterQuestStateData()
        {
            QuestTypeId = _qtype.IdKey,
            MapId = _qtype.MapId,
            AlterTypeId = alterQuestTypeId,
        };
        if (_type != null)
        {
            alterData.NPCTypeId = _type.IdKey;
        }
    }
}