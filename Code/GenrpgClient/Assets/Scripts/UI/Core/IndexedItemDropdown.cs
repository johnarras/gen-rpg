using System.Collections.Generic;
using UnityEngine.Events;
using Genrpg.Shared.Interfaces;

public class IndexedItemDropdown : BaseBehaviour
{
    
    public GDropdown DropdownList;
    public GText NameText;

    public void Init<T> (UnityGameState gs, List<T> items, UnityAction<int> onValueChanged  = null) where T : IId, IName
    {
        if (items == null || DropdownList == null)
        {
            GEntityUtils.Destroy(entity);
            return;
        }
        List<GDropdown.OptionData> options = new List<GDropdown.OptionData>();

        for (int v = 0; v < items.Count; v++)
        {
            MyDropdownOption did = new MyDropdownOption();
            did.text = items[v].IdKey + ": " + items[v].Name;
            did.OptionItem = items[v];
            options.Add(did);          
        }

        DropdownList.AddOptions(options);
        if (onValueChanged != null)
        {
            DropdownList.onValueChanged.RemoveAllListeners();
            DropdownList.onValueChanged.AddListener(onValueChanged);
        }
        DropdownList.value = 0;

        UIHelper.SetText(NameText, typeof(T).Name);


    }
}