using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;

using Services;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Entities;
using Genrpg.Shared.Spells.Entities;

public class SpellModDropdown : BaseBehaviour
{
    public SpellModifier Mod { get; set; }

    [SerializeField]
    private Dropdown _dropdownList;
    [SerializeField]
    private Text _name;

    public void Init(SpellModifier mod, UnityAction<int> onValueChanged = null, int initialValue = -1)
    {
        Mod = mod;
        if (mod ==null)
        {
            GameObject.Destroy(gameObject);
            return;
        }

        if (Mod.Values == null || _dropdownList == null)
        {
            return;
        }

        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();

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


        _dropdownList.onValueChanged.RemoveAllListeners();

        _dropdownList.AddOptions(options);
       
        UIHelper.SetText(_name, Mod.Name);


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
            _dropdownList.value = defaultIndex;
        }
        if (onValueChanged != null)
        {
            _dropdownList.onValueChanged.AddListener(onValueChanged);
        }
    }
}
