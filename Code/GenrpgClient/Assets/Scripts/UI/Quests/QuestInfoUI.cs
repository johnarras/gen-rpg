﻿using System.Collections.Generic;
using System.Linq;
using GEntity = UnityEngine.GameObject;
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

public class QuestInfoUI : BaseBehaviour
{

    protected ISharedQuestService _questService;

    public const string ScreenTaskPrefab = "QuestScreenTaskRow";
    public const string HUDTaskPrefab = "QuestHUDTaskRow";

    
    public GEntity ContentParent;
    public GImage StatusIcon;
    public GText QuestName;
    public GText Description;
    public GEntity TaskParent;
    public GText Experience;
    public MoneyDisplay Money;
    public GEntity OtherRewards;
    public GText ItemRewardText;
    public bool ShowWord = false;
    public GEntity AcceptButton;
    public GEntity AbandonButton;
    public GEntity CompleteButton;

    QuestType _qtype = null;
    QuestScreen _screen = null;
    private CancellationToken _token;
    MapObject _obj = null;
    public void Init(QuestType qtype, int index, QuestScreen screen, MapObject mapObject, CancellationToken token)
    {
        _gs.AddEvent<UpdateQuestEvent>(this, OnUpdateQuest);
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
        GEntityUtils.SetActive(ContentParent, _qtype != null);
        if (_qtype == null)
        {            
            return;
        }
        string rowPrefabName = GetTaskRowPrefabName();

        int state = _questService.GetQuestState(_gs, _gs.ch, _qtype);

        string nameText = _qtype.Name;

        if (ShowWord)
        {
            if (state == QuestState.Complete)
            {
                nameText = "(Complete) " + nameText;
            }
        }

        UpdateButtonsFromState(state);


        UIHelper.SetText(QuestName, nameText);
        UIHelper.SetText(Description, _qtype.Desc);

        GEntityUtils.DestroyAllChildren(TaskParent);
        if (_qtype.Tasks != null && TaskParent != null)
        {
            GEntityUtils.DestroyAllChildren(TaskParent);
            foreach (QuestTask task in _qtype.Tasks)
            {
                _assetService.LoadAssetInto(_gs, TaskParent, AssetCategoryNames.UI,
                    rowPrefabName, OnLoadTask, task, _token, _screen.Subdirectory);
            }
        }


        List<SpawnResult> rewards = _questService.GetRewards(_gs, _gs.ch, _qtype, false);

        SpawnResult expReward = rewards.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Currency && x.EntityId == CurrencyTypes.Exp);
        if (expReward != null)
        {
            UIHelper.SetText(Experience, "XP: " + expReward.Quantity.ToString());
        }
        SpawnResult moneyReward = rewards.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Currency && x.EntityId == CurrencyTypes.Money);
        if (moneyReward != null && Money != null)
        {
            Money.SetMoney(moneyReward.Quantity);
        }

        List<SpawnResult> itemRewards = rewards.Where(X => X.EntityTypeId == EntityTypes.Item && (X.Data as Item) != null).ToList();


        UIHelper.SetText(ItemRewardText, "");
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
                        entityTypeId = EntityTypes.Item,
                        entityId = item.UseEntityId,
                    };
                    IconHelper.InitItemIcon(_gs, idata, OtherRewards, this._assetService, _token);
                }
            }
        }
        else if (_qtype.ItemQuantity > 0)
        {
            string qualityString = "";

            if (_qtype.ItemQualityTypeId > 0)
            {
                QualityType qualityType = _gs.data.GetGameData<QualityTypeSettings>(_gs.ch).GetQualityType(_qtype.ItemQualityTypeId);
                if (qualityType != null)
                {
                    qualityString = " " + qualityType.Name;
                }
            }
            string txt = "Create " + _qtype.ItemQuantity + "" + qualityString + " Item" + (_qtype.ItemQuantity==1?"":"s");
            UIHelper.SetText(ItemRewardText, txt);
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
        GEntity go = obj as GEntity;
        if (go == null)
        {
            return;
        }

        QuestTask task = data as QuestTask;

        if (task == null)
        {
            GEntityUtils.Destroy(go);
            return;
        }

        QuestTaskUI taskRow = go.GetComponent<QuestTaskUI>();

        if (taskRow == null)
        {
            GEntityUtils.Destroy(go);
            return;
        }

        taskRow.Init(_qtype, task);
    }

    public void UpdateButtonsFromState(int state)
    {
        GEntityUtils.SetActive(AcceptButton, false);
        GEntityUtils.SetActive(AbandonButton, false);
        GEntityUtils.SetActive(CompleteButton, false);

        if (state == QuestState.Available)
        {
            GEntityUtils.SetActive(AcceptButton, true);
        }
        else if (state == QuestState.Active)
        {
            GEntityUtils.SetActive(AbandonButton, true);
        }
        else if (state == QuestState.Complete)
        {
            GEntityUtils.SetActive(CompleteButton, true);
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