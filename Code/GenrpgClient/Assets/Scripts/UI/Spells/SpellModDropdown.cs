using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;
using Genrpg.Shared.Spells.Entities;

public class SpellModDropdown : BaseBehaviour
{
    public SpellModifier Mod { get; set; }

    public GDropdown DropdownList;
    public GText SpellModName;

    public void Init(SpellModifier mod, UnityAction<int> onValueChanged = null, int initialValue = -1)
    {
        Mod = mod;
        if (mod ==null)
        {
            GEntityUtils.Destroy(entity);
            return;
        }

        if (Mod.Values == null || DropdownList == null)
        {
            return;
        }

        List<GDropdown.OptionData> options = new List<GDropdown.OptionData>();

        int defaultIndex = -1;
        int desiredIndex = -1;
        StringBuilder sb = new StringBuilder();
        for (int v = 0; v < Mod.Values.Count; v++)
        {
            SpellModifierValue val = Mod.Values[v];
            sb.Clear();
            float value = val.Value;
            if (mod.DisplayMult != 0 && mod.DisplayMult != 1)
            {
                value *= mod.DisplayMult;
            }
            sb.Append(value + Mod.DisplaySuffix);
            sb.Append(": Cost: ");
            sb.Append(val.CostScale + "%");
            MyDropdownOption did = new MyDropdownOption();
            did.OptionItem = val;
            did.text = sb.ToString();
            options.Add(did);
            if (val.CostScale == SpellModifier.DefaultCostScale)
            {
                defaultIndex = v;
            }
            if (initialValue >= 0)
            {
                if (val.Value == initialValue)
                {
                    desiredIndex = v;
                }
            }
        }


        DropdownList.onValueChanged.RemoveAllListeners();

        DropdownList.AddOptions(options);
       
        UIHelper.SetText(SpellModName, Mod.Name);


        if (desiredIndex >= 0)
        {
            defaultIndex = desiredIndex;
        }
        if (defaultIndex < 0)
        {
            defaultIndex = 0;
        }

        if (defaultIndex >= 0)
        {
            DropdownList.value = defaultIndex;
        }
        if (onValueChanged != null)
        {
            DropdownList.onValueChanged.AddListener(onValueChanged);
        }
    }
}
