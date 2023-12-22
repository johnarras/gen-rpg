using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Utils;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crafting.PlayerData.Recipes
{

    [MessagePackObject]
    public class RecipeData : OwnerIdObjectList<RecipeStatus>
    {
        [Key(0)] public override string Id { get; set; }

        public override List<RecipeStatus> GetData()
        {
            return _data;
        }

        public override void SetData(List<RecipeStatus> data)
        {
            _data = data;
        }

        public void AddRecipeStatus(long recipeTypeId)
        {
            RecipeStatus status = new RecipeStatus()
            {
                IdKey = recipeTypeId,
                Id = HashUtils.NewGuid(),
                OwnerId = Id,
            };
        }
    }

}
