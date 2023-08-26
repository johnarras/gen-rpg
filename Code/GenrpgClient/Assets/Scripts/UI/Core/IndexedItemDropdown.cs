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
using Genrpg.Shared.Interfaces;

public class IndexedItemDropdown : BaseBehaviour
{
    [SerializeField]
    private Dropdown _dropdownList;
    [SerializeField]
    private Text _name;
    public void Init<T> (UnityGameState gs, List<T> items, UnityAction<int> onValueChanged  = null) where T : IId, IName
    {
        if (items == null || _dropdownList == null)
        {
            GameObject.Destroy(gameObject);
            return;
        }
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();

        for (int v = 0; v < items.Count; v++)
        {
            MyDropdownOption did = new MyDropdownOption();
            did.text = items[v].IdKey + ": " + items[v].Name;
            did.OptionItem = items[v];
            options.Add(did);          
        }

        _dropdownList.AddOptions(options);
        if (onValueChanged != null)
        {
            _dropdownList.onValueChanged.RemoveAllListeners();
            _dropdownList.onValueChanged.AddListener(onValueChanged);
        }
        _dropdownList.value = 0;

        UIHelper.SetText(_name, typeof(T).Name);


    }
}