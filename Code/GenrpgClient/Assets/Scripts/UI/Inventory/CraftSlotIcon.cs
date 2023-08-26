using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;

using Services;
using Genrpg.Shared.Inventory.Entities;

/// <summary>
/// Use this to track items that are currently being added to a recipe.
/// </summary>
public class CraftSlotIcon : BaseBehaviour
{
    public Item item;
    public long quantity;
    public string description;
}

