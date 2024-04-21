
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
        _recipe = _gameData.Get<RecipeSettings>(_gs.ch).Get(_status.IdKey);

        if (_recipe == null)
        {
            OnError();
            return;
        }

        _uIInitializable.SetText(RecipeName, _recipe.Name);

        _uIInitializable.SetText(RecipeRank, _status.Get().ToString() + "/" + status.GetMaxLevel());

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
