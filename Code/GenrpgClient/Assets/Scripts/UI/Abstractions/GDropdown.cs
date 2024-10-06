
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.ProcGen.Settings.Names;
using Genrpg.Shared.UI.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using UnityEngine.EventSystems;

public class GDropdown : TMPro.TMP_Dropdown, IDropdown
{
    public GText DropdownName;

    private List<IIdName> _iidList = new List<IIdName>();
    private List<double> _doubleList = new List<double>();
    public void Init<IDN>(IReadOnlyList<IDN> list, Action onValueChangedAction) where IDN : IIdName
    {
        ClearOptions();

        AddOptions(list.Select(x => x.Name).ToList());

        _iidList = new List<IIdName>();
        foreach (IDN item in list)
        {
            _iidList.Add(item);
        }
        if (onValueChangedAction != null)
        {
            onValueChanged.RemoveAllListeners();
            onValueChanged.AddListener(delegate { onValueChangedAction(); });
        }
    }

    public void Init(List<double> list, Action onValueChangedAction)
    {
        ClearOptions();
        AddOptions(list.Select(x => x.ToString()).ToList());
        _doubleList = list;
        if (onValueChangedAction != null)
        {
            onValueChanged.RemoveAllListeners();
            onValueChanged.AddListener(delegate { onValueChangedAction(); });
        }
    }

    public void SetFromValue(double value)
    {
        for (int i = 0; i < options.Count; i++)
        {
            if (options[i].text == value.ToString())
            {
                SetValueWithoutNotify(i);
                break;
            }
        }
    }

    public void SetFromTextValue(string textValue)
    {
        for (int i = 0; i < options.Count; i++)
        {
            if (options[i].text == textValue)
            {
                SetValueWithoutNotify(i);
                break;
            }
        }
    }

    public void SetFromId(long initialValue)
    { 
        IIdName selectedIdn = _iidList.FirstOrDefault(x => x.IdKey == initialValue);
        if (selectedIdn != null)
        {
            for (int i = 0; i < options.Count; i++)
            {
                if (options[i].text == selectedIdn.Name)
                {
                    SetValueWithoutNotify(i);
                    break;
                }
            }
        }
    }
}
