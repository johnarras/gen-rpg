
using Genrpg.Shared.Crafting.PlayerData.Recipes;
using Genrpg.Shared.Crafting.Settings.Recipes;
using Genrpg.Shared.Stats.Settings.Scaling;

public class RecipeRow : BaseBehaviour
{
    private RecipeType _recipe { get; set; }
    private RecipeStatus _status { get; set; }
    private ScalingType _scaling { get; set; }
    private CraftingScreen _screen { get; set; }
    
    public GText RecipeName;
    public GText RecipeRank;
    public GImage BGImage;

    public void Init(RecipeStatus status, CraftingScreen screen)
    {
        _screen = screen;
        _status = status;
        if (_status == null)
        {
            OnError();
            return;
        }
        _recipe = _gs.data.GetGameData<RecipeSettings>(_gs.ch).GetRecipeType(_status.IdKey);

        if (_recipe == null)
        {
            OnError();
            return;
        }

        _uiService.SetText(RecipeName, _recipe.Name);

        _uiService.SetText(RecipeRank, _status.GetLevel().ToString() + "/" + status.GetMaxLevel());

        SetIsActive(false);
    }

    private void OnError()
    {
        GEntityUtils.Destroy(entity);
        return;
    }

    public void OnClick()
    {
        if (_screen != null)
        {
            _screen.SetActiveRecipe(this);
        }
    }

    public void SetIsActive(bool active)
    {
        if (BGImage != null)
        {
            BGImage.Color = (active?GColor.yellow : GColor.gray);
        }
    }


}
