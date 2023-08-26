using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class CoreUnitUI : BaseBehaviour
{
    [SerializeField]
    private Text _nameText;
    [SerializeField]
    private Text _levelText;

    public void Init(string name, long level)
    {
        UIHelper.SetText(_nameText, name);
        UIHelper.SetText(_levelText, level.ToString());
    }
}