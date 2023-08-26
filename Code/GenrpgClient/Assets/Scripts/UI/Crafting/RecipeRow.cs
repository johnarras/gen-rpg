
using UnityEngine;
using UnityEngine.UI;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Crafting.Entities;

public class RecipeRow : BaseBehaviour
{
    private RecipeType _recipe { get; set; }
    private RecipeStatus _status { get; set; }
    private ScalingType _scaling { get; set; }

    private CraftingScreen _screen { get; set; }

    [SerializeField]
    private Text _name;
    [SerializeField]
    private Text _rank;
    [SerializeField]
    private Image _bgImage;

    public void Init(RecipeStatus status, CraftingScreen screen)
    {
        _screen = screen;
        _status = status;
        if (_status == null)
        {
            OnError();
            return;
        }
        _recipe = _gs.data.GetGameData<CraftingSettings>().GetRecipeType(_status.IdKey);

        if (_recipe == null)
        {
            OnError();
            return;
        }

        UIHelper.SetText(_name, _recipe.Name);

        UIHelper.SetText(_rank, _status.GetLevel().ToString() + "/" + status.GetMaxLevel());

        SetIsActive(false);
    }

    private void OnError()
    {
        GameObject.Destroy(gameObject);
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
        Color bgColor = active ? Color.yellow : Color.gray;    
        if (_bgImage != null)
        {
            _bgImage.color = bgColor;
        }
    }


}
