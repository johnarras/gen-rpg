using System;
using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;
using ClientEvents;
using Genrpg.Shared.Units.Entities;

using Genrpg.Shared.Quests.Services;
using UI.Screens.Constants;

using System.Threading;
using Genrpg.Shared.Quests.Messages;
using Genrpg.Shared.Quests.WorldData;
using Genrpg.Shared.Quests.Constants;
using Genrpg.Shared.Quests.PlayerData;
using Genrpg.Shared.MapObjects.MapObjectAddons.Entities;
using Genrpg.Shared.MapObjects.MapObjectAddons.Constants;
using UnityEngine;

internal class QuestTypeWithIndex
{
    public QuestType qtype;
    public int index;
}

/// <summary>
/// This is the basic NPC screen that also lets you see the vendor screen if you wish.
/// </summary>
public class QuestScreen : ItemIconScreen
{
    public GButton VendorButton;
    public GEntity QuestListParent;
    public QuestInfoUI FullQuestInfo;

    Unit _unit = null;

    List<QuestType> _allQuests = null;
    bool _openVendorScreenIfNoQuests = false;

    protected ISharedQuestService _questService = null;
    protected override async Awaitable OnStartOpen(object data, CancellationToken token)
    {
        await base.OnStartOpen(data, token);
        _dispatcher.AddEvent<AlterQuestStateEvent>(this, OnAlterQuestState);
        _dispatcher.AddEvent<OnGetQuests>(this, OnGetQuestsHandler);
        _uIInitializable.SetButton(VendorButton, GetName(), ClickVendor);
        _unit = data as Unit;
        if (_unit == null)
        {
            StartClose();
            return;
        }

        if (!_unit.HasAddon(MapObjectAddonTypes.Vendor))
        {
            GEntityUtils.Destroy(VendorButton);
            ShowMyQuests();
            return;
        }

        ShowQuests(true);

        
    }

    private void OnAlterQuestState (AlterQuestStateEvent data)
    {
        ShowQuests(false);
        return;
    }

    protected void ShowQuests(bool openVendorScreenIfNoQuests)
    {
        _openVendorScreenIfNoQuests = openVendorScreenIfNoQuests;
        GetQuests getQuests = new GetQuests() { ObjId = _unit.Id };

        _networkService.SendMapMessage(getQuests);
        ShowQuestList(new List<QuestType>());
    }

    private void OnGetQuestsHandler(OnGetQuests quests)
    {
        if (quests.ObjId != _unit.Id)
        {
            quests.Quests = new List<QuestType>();
        }

        List<QuestType> allQuests = quests.Quests;

        List<QuestType> questsToShow = new List<QuestType>();

        foreach (QuestType quest in allQuests)
        {

            int questState = _questService.GetQuestState(_rand, _gs.ch, quest);

            if (questState == QuestState.Available || questState == QuestState.Complete ||
                questState == QuestState.Active)
            {
                questsToShow.Add(quest);
            }
        }

        if (questsToShow.Count < 1 && _openVendorScreenIfNoQuests)
        {
            ClickVendor();
            return;
        }

        GEntityUtils.SetActive(VendorButton, _unit.HasAddon(MapObjectAddonTypes.Vendor));

        ShowQuestList(questsToShow);

        return;
    }

    protected void ShowMyQuests()
    {
        List<QuestType> showList = new List<QuestType>();
        QuestData quests = _gs.ch.Get<QuestData>();
        foreach (QuestStatus qstatus in quests.GetData())
        {
            if (qstatus.Quest != null)
            {
                showList.Add(qstatus.Quest);
            }
        }
        ShowQuestList(showList);
    }

    public void ClickVendor()
    {
        if (_unit.HasAddon(MapObjectAddonTypes.Vendor))
        {
            _screenService.Open(ScreenId.Vendor, _unit);
            _screenService.Close(ScreenID);
        }
    }

    protected virtual string GetQuestRowPrefab()
    {
        return "QuestScreenRow";
    }

    protected void ShowQuestList(List<QuestType> quests)
    {
        _allQuests = quests;

        if (_allQuests == null)
        {
            return;
        }

        if (QuestListParent == null)
        {
            return;
        }

        GEntityUtils.DestroyAllChildren(QuestListParent);

        ShowFullQuestData(null);

        for (int i = 0; i < _allQuests.Count; i++)
        {
            QuestTypeWithIndex questIndexInfo = new QuestTypeWithIndex()
            {
                qtype = _allQuests[i],
                index = i,
            };
            _assetService.LoadAssetInto(QuestListParent, AssetCategoryNames.UI, 
                GetQuestRowPrefab(), OnLoadScreenQuest,  questIndexInfo, _token, Subdirectory);
        }

    }
        

    public void ShowFullQuestData(QuestType qtype)
    {
        if (FullQuestInfo != null)
        {
            FullQuestInfo.Init(qtype, 0, this, _unit, _token);
        }
    }

    private void OnLoadScreenQuest(object obj, object data, CancellationToken token)
    {
        GEntity go = obj as GEntity;
        if (go == null)
        {
            return;
        }

        QuestTypeWithIndex qindex = data as QuestTypeWithIndex;

        if (qindex == null || qindex.qtype == null) 
        {
            GEntityUtils.Destroy(go);
            return;
        }

        QuestInfoUI questUI = go.GetComponent<QuestInfoUI>();
        if (questUI == null)
        {
            return;
        }

        questUI.Init(qindex.qtype, qindex.index, this, null, token);
    }
}