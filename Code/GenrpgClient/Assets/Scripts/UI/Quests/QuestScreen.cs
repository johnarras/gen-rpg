using System;
using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;
using ClientEvents;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Quests.Entities;

using Genrpg.Shared.Quests.Services;
using Genrpg.Shared.NPCs.Entities;
using UI.Screens.Constants;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.Quests.Messages;

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
    public GButton CloseButton;
    public GEntity QuestListParent;
    public QuestInfoUI FullQuestInfo;

    NPCType _type = null;
    Unit _unit = null;

    List<QuestType> _allQuests = null;
    bool _openVendorScreenIfNoQuests = false;

    protected ISharedQuestService _questService = null;
    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        await base.OnStartOpen(data, token);
        _gs.AddEvent<AlterQuestStateEvent>(this, OnAlterQuestState);
        _gs.AddEvent<OnGetNPCQuests>(this, GetNPCQuestsHandler);
        UIHelper.SetButton(CloseButton, GetAnalyticsName(), StartClose);
        UIHelper.SetButton(VendorButton, GetAnalyticsName(), ClickVendor);
        _unit = data as Unit;
        if (_unit == null)
        {
            StartClose();
            return;
        }

        _type = _unit.NPCType;

        if (_type == null)
        {
            GEntityUtils.Destroy(VendorButton);
            ShowMyQuests();
            return;
        }

        ShowNPCQuests(true);

        await Task.CompletedTask;
    }

    private AlterQuestStateEvent OnAlterQuestState (UnityGameState gs, AlterQuestStateEvent data)
    {
        ShowNPCQuests(false);
        return null;
    }

    protected void ShowNPCQuests(bool openVendorScreenIfNoQuests)
    {
        _openVendorScreenIfNoQuests = openVendorScreenIfNoQuests;
        GetNPCQuests getQuests = new GetNPCQuests() { NPCTypeId = _type.IdKey };

        _networkService.SendMapMessage(getQuests);
        ShowQuestList(new List<QuestType>());
    }

    private OnGetNPCQuests GetNPCQuestsHandler(UnityGameState gs, OnGetNPCQuests quests)
    {
        if (quests.NPCTypeId != _type.IdKey)
        {
            quests.Quests = new List<QuestType>();
        }

        List<QuestType> allQuests = quests.Quests;

        List<QuestType> questsToShow = new List<QuestType>();

        foreach (QuestType quest in allQuests)
        {

            int questState = _questService.GetQuestState(gs, gs.ch, quest);

            if (questState == QuestState.Available || questState == QuestState.Complete ||
                questState == QuestState.Active)
            {
                questsToShow.Add(quest);
            }
        }

        if (questsToShow.Count < 1 && _openVendorScreenIfNoQuests)
        {
            ClickVendor();
            return null;
        }

        GEntityUtils.SetActive(VendorButton, _type.ItemCount > 0);

        ShowQuestList(questsToShow);

        return null;
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
        if (_type != null && _type.ItemCount > 0)
        {
            _screenService.Open(_gs, ScreenId.Vendor, _unit);
            _screenService.Close(_gs, ScreenId);
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
            _assetService.LoadAssetInto(_gs, QuestListParent, AssetCategoryNames.UI, GetQuestRowPrefab(), OnLoadScreenQuest,  questIndexInfo, _token);
        }

    }
        

    public void ShowFullQuestData(QuestType qtype)
    {
        if (FullQuestInfo != null)
        {
            FullQuestInfo.Init(qtype, 0, this, null, _token);
        }
    }

    private void OnLoadScreenQuest(UnityGameState gs, string url, object obj, object data, CancellationToken token)
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