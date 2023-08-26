using System;
using System.Collections.Generic;
using UnityEngine;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Crafting.Entities;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.UI;

public class CraftingScreen : ItemIconScreen
{
    public const string RecipeRow = "RecipeRow";
    public const string ReagentRow = "ReagentRow";
    public const string CraftSlotIcon = "CraftSlotIcon";
    public const string CraftInventoryIcon = "CraftInventoryIcon";

    [SerializeField]
    private Button _craftButton;
    [SerializeField]
    private Button _clearButton;
    [SerializeField]
    private Button _closeButton;
    [SerializeField]
    private InventoryPanel _inventoryPanel;
    [SerializeField]
    private GameObject _recipeListParent;

    private List<RecipeRow> _recipes { get; set; }

    [SerializeField]
    private ReagentRow _baseReagents;
    [SerializeField]
    private ReagentRow _coreReagents;
    [SerializeField]
    private ReagentRow _optionalReagents;

    private RecipeRow _currentRecipe = null;

    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        Init();
        await base.OnStartOpen (data, token);
    }

    public override void OnRightClickIcon(UnityGameState gs, ItemIcon icon)
    {
    }

    public override void OnLeftClickIcon(UnityGameState gs, ItemIcon icon)
    {
    }

    public void ClickCraft()
    {

    }

    public void ClickClear()
    {
        ClearScreen();
    }

    private void ClearScreen()
    {
        
    }

    public void Init()
    {
        UIHelper.SetButton(_clearButton, GetAnalyticsName(), ClickClear);
        UIHelper.SetButton(_craftButton, GetAnalyticsName(), ClickCraft);
        UIHelper.SetButton(_closeButton, GetAnalyticsName(), StartClose);
        if (_gs.data == null || _gs.data.GetGameData<CraftingSettings>().RecipeTypes == null)
        {
            ErrorClose("Missing recipe list");
            return;
        }

        _recipes = new List<RecipeRow>();

        RecipeData recipeData = _gs.ch.Get<RecipeData>();
        foreach (RecipeType recipe in _gs.data.GetGameData<CraftingSettings>().RecipeTypes)
        {

            RecipeStatus recipeStatus = recipeData.Get(recipe.IdKey);

            ShowOneRecipe(recipeStatus);
        }

        if (_inventoryPanel != null)
        {
            _inventoryPanel.Init(InventoryGroup.Reagents, this, CraftInventoryIcon, _token);
        }

    }


    protected void ShowOneRecipe (RecipeStatus status)
    {
        if (status == null)
        {
            return;
        }

        _assetService.LoadAsset(_gs, AssetCategory.UI, RecipeRow, OnLoadRecipeRow, status, _recipeListParent, _token);        
    }

    private void OnLoadRecipeRow(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        GameObject go = obj as GameObject;
        if (go == null)
        {
            return;
        }

        RecipeRow row = go.GetComponent<RecipeRow>();
        RecipeStatus status = data as RecipeStatus;
        if (status == null || row == null)
        {
            GameObject.Destroy(go);
            return;
        }

        row.Init(status, this);
        _recipes.Add(row);
    }

    public void SetActiveRecipe (RecipeRow row)
    {
        if (row == _currentRecipe)
        {
            return;
        }

        if (_recipes == null)
        {
            return;
        }

        foreach (RecipeRow rec in _recipes)
        {
            rec.SetIsActive(rec == row);
        }
        _currentRecipe = row;
    }
}

