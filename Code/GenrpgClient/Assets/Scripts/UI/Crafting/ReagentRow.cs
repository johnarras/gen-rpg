using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;

using Services;
using UnityEngine;
using UnityEngine.UI;

public class ReagentRow : BaseBehaviour
{
    public Text ReagentCategory;

    public GameObject IconParent;

    public List<CraftSlotIcon> Icons;
}
