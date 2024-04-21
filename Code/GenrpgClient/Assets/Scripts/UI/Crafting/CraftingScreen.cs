using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;
using Cysharp.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.Crafting.PlayerData.Recipes;
using Genrpg.Shared.Crafting.Settings.Recipes;

public class CraftingScreen : ItemIconScreen
{
    public const string RecipeRow = "RecipeRow";
    public const string ReagentRow = "ReagentRow";
    public const string CraftSlotIcon = "CraftSlotIcon";
    public const string CraftInventoryIcon = "CraftInventoryIcon";
    
    public GButton CraftButton;
    public GButton ClearButton;
    public InventoryPanel _inventoryPanel;
    public GEntity _recipeListParent;
    public ReagentRow _baseReagents;
    public ReagentRow _coreReagents;
    public ReagentRow _optionalReagents;

    private List<RecipeRow> _recipes { get; set; }

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
        _uIInitializable.SetButton(ClearButton, GetName(), ClickClear);
        _uIInitializable.SetButton(CraftButton, GetName(), ClickCraft);

        _recipes = new List<RecipeRow>();

        RecipeData recipeData = _gs.ch.Get<RecipeData>();
        foreach (RecipeType recipe in _gameData.Get<RecipeSettings>(_gs.ch).GetData())
        {

            RecipeStatus recipeStatus = recipeData.Get(recipe.IdKey);

            ShowOneRecipe(recipeStatus);
        }

        if (_inventoryPanel != null)
        {
            _inventoryPanel.Init(InventoryGroup.Reagents, this, _gs.ch, CraftInventoryIcon, _token);
        }

    }


    protected void ShowOneRecipe (RecipeStatus status)
    {
        if (status == null)
        {
            return;
        }

        _assetService.LoadAsset(_gs, AssetCategoryNames.UI, RecipeRow, OnLoadRecipeRow, 
            status, _recipeListParent, _token, Subdirectory);        
    }

    private void OnLoadRecipeRow(UnityGameState gs, object obj, object data, CancellationToken token)
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

