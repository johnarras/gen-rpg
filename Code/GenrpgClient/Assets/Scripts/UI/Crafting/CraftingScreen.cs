using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Crafting.Entities;
using System.Threading.Tasks;
using System.Threading;

public class CraftingScreen : ItemIconScreen
{
    public const string RecipeRow = "RecipeRow";
    public const string ReagentRow = "ReagentRow";
    public const string CraftSlotIcon = "CraftSlotIcon";
    public const string CraftInventoryIcon = "CraftInventoryIcon";
    
    public GButton CraftButton;
    public GButton ClearButton;
    public GButton CloseButton;
    public InventoryPanel _inventoryPanel;
    public GEntity _recipeListParent;
    public ReagentRow _baseReagents;
    public ReagentRow _coreReagents;
    public ReagentRow _optionalReagents;

    private List<RecipeRow> _recipes { get; set; }

    private RecipeRow _currentRecipe = null;

    protected override async Task OnStartOpen(object data, CancellationToken token)
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
        UIHelper.SetButton(ClearButton, GetAnalyticsName(), ClickClear);
        UIHelper.SetButton(CraftButton, GetAnalyticsName(), ClickCraft);
        UIHelper.SetButton(CloseButton, GetAnalyticsName(), StartClose);

        _recipes = new List<RecipeRow>();

        RecipeData recipeData = _gs.ch.Get<RecipeData>();
        foreach (RecipeType recipe in _gs.data.GetGameData<RecipeSettings>(_gs.ch).GetData())
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
        GEntity go = obj as GEntity;
        if (go == null)
        {
            return;
        }

        RecipeRow row = go.GetComponent<RecipeRow>();
        RecipeStatus status = data as RecipeStatus;
        if (status == null || row == null)
        {
            GEntityUtils.Destroy(go);
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

