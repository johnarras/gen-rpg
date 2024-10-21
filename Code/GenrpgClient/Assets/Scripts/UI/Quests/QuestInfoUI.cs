using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ClientEvents;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Quests.Entities;
using Genrpg.Shared.Quests.Services;
using System.Threading;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Quests.WorldData;
using Genrpg.Shared.Quests.Constants;
using Genrpg.Shared.Quests.PlayerData;
using Genrpg.Shared.Inventory.Settings.Qualities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Client.Assets.Constants;

public class QuestInfoUI : BaseBehaviour
{

    protected ISharedQuestService _questService;
    protected IIconService _iconService;

    public const string ScreenTaskPrefab = "QuestScreenTaskRow";
    public const string HUDTaskPrefab = "QuestHUDTaskRow";

    
    public GameObject ContentParent;
    public GImage StatusIcon;
    public GText QuestName;
    public GText Description;
    public GameObject TaskParent;
    public GText Experience;
    public MoneyDisplay Money;
    public GameObject OtherRewards;
    public GText ItemRewardText;
    public bool ShowWord = false;
    public GameObject AcceptButton;
    public GameObject AbandonButton;
    public GameObject CompleteButton;

    QuestType _qtype = null;
    QuestScreen _screen = null;
    private CancellationToken _token;
    MapObject _obj = null;
    public void Init(QuestType qtype, int index, QuestScreen screen, MapObject mapObject, CancellationToken token)
    {
        AddListener<UpdateQuestEvent>(OnUpdateQuest);
        _token = token;
        _qtype = qtype;
        _screen = screen;
        _obj = mapObject;
        ShowQuestInfo();
    }

    protected string GetTaskRowPrefabName()
    {
        return (_screen != null ? ScreenTaskPrefab : HUDTaskPrefab);
    }

    protected virtual void ShowQuestInfo()
    {
        _clientEntityService.SetActive(ContentParent, _qtype != null);
        if (_qtype == null)
        {            
            return;
        }
        string rowPrefabName = GetTaskRowPrefabName();

        int state = _questService.GetQuestState(_rand, _gs.ch, _qtype);

        string nameText = _qtype.Name;

        if (ShowWord)
        {
            if (state == QuestState.Complete)
            {
                nameText = "(Complete) " + nameText;
            }
        }

        UpdateButtonsFromState(state);


        _uiService.SetText(QuestName, nameText);
        _uiService.SetText(Description, _qtype.Desc);

        _clientEntityService.DestroyAllChildren(TaskParent);
        if (_qtype.Tasks != null && TaskParent != null)
        {
            _clientEntityService.DestroyAllChildren(TaskParent);
            foreach (QuestTask task in _qtype.Tasks)
            {
                _assetService.LoadAssetInto(TaskParent, AssetCategoryNames.UI,
                    rowPrefabName, OnLoadTask, task, _token, _screen.Subdirectory);
            }
        }


        List<Reward> rewards = _questService.GetRewards(_rand, _gs.ch, _qtype, false);

        Reward expReward = rewards.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Currency && x.EntityId == CurrencyTypes.Exp);
        if (expReward != null)
        {
            _uiService.SetText(Experience, "XP: " + expReward.Quantity.ToString());
        }
        Reward moneyReward = rewards.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Currency && x.EntityId == CurrencyTypes.Money);
        if (moneyReward != null && Money != null)
        {
            Money.SetMoney(moneyReward.Quantity);
        }

        List<Reward> itemRewards = rewards.Where(X => X.EntityTypeId == EntityTypes.Item && (X.ExtraData as Item) != null).ToList();


        _uiService.SetText(ItemRewardText, "");
        if (itemRewards.Count > 0)
        {

            foreach (Reward ireward in itemRewards)
            {
                Item item = ireward.ExtraData as Item;
                if (item != null)
                {
                    InitItemIconData idata = new InitItemIconData()
                    {
                        Data = item,
                        EntityTypeId = EntityTypes.Item,
                    };
                    _iconService.InitItemIcon(idata, OtherRewards, this._assetService, _token);
                }
            }
        }
        else if (_qtype.ItemQuantity > 0)
        {
            string qualityString = "";

            if (_qtype.ItemQualityTypeId > 0)
            {
                QualityType qualityType = _gameData.Get<QualityTypeSettings>(_gs.ch).Get(_qtype.ItemQualityTypeId);
                if (qualityType != null)
                {
                    qualityString = " " + qualityType.Name;
                }
            }
            string txt = "Create " + _qtype.ItemQuantity + "" + qualityString + " Item" + (_qtype.ItemQuantity==1?"":"s");
            _uiService.SetText(ItemRewardText, txt);
        }
    }

    public void ShowFullQuestData()
    {
        if (_screen != null)
        {
            _screen.ShowFullQuestData(_qtype);
        }
    }

    private void OnUpdateQuest(UpdateQuestEvent data)
    {
        if (data == null)
        {
            return;
        }

        QuestStatus questStatus = data.Status;

        if (questStatus == null)
        {
            return;
        }

        QuestType quest = questStatus.Quest;

        if (quest == null)
        {
            return;
        }

        if (_qtype == null)
        {
            return;
        }

        if (!_qtype.IsSameQuest(quest))
        {
            return;
        }

        int state = _questService.GetQuestState(_rand, _gs.ch, quest);

        if (state == QuestState.Complete)
        {
            _qtype = null;
        }
        else
        {
            _qtype = quest;
        }
        ShowQuestInfo();

        return;
    }

    private void OnLoadTask (object obj, object data, CancellationToken token)
    {
        GameObject go = obj as GameObject;
        if (go == null)
        {
            return;
        }

        QuestTask task = data as QuestTask;

        if (task == null)
        {
            _clientEntityService.Destroy(go);
            return;
        }

        QuestTaskUI taskRow = go.GetComponent<QuestTaskUI>();

        if (taskRow == null)
        {
            _clientEntityService.Destroy(go);
            return;
        }

        taskRow.Init(_qtype, task);
    }

    public void UpdateButtonsFromState(int state)
    {
        _clientEntityService.SetActive(AcceptButton, false);
        _clientEntityService.SetActive(AbandonButton, false);
        _clientEntityService.SetActive(CompleteButton, false);

        if (state == QuestState.Available)
        {
            _clientEntityService.SetActive(AcceptButton, true);
        }
        else if (state == QuestState.Active)
        {
            _clientEntityService.SetActive(AbandonButton, true);
        }
        else if (state == QuestState.Complete)
        {
            _clientEntityService.SetActive(CompleteButton, true);
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

        if (_obj != null)
        {
            alterData.QuestGiverId = _obj.Id;
        }
    }
}