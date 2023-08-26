using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;

using Services;
using UnityEngine;
using UnityEngine.UI;


public class ItemTooltipRow : BaseBehaviour
{
    [SerializeField]
    private Text _textRow;
    [SerializeField]
    private Text _changeText;
    [SerializeField]
    private List<GameObject> _stars;

    public void Init(UnityGameState gs, ItemTooltipRowData rowData)
    {
        if (rowData == null)
        {
            GameObject.Destroy(gameObject);
            return;
        }

        UIHelper.SetText(_textRow, rowData.text);

        if (_textRow != null)
        {
            if (rowData.isCurrent)
            {
                UIHelper.SetColor(_textRow, Color.white);
            }
            else
            {
                UIHelper.SetColor(_textRow, Color.gray);
            }
        }
        if (rowData.change == 0)
        {
            UIHelper.SetText(_changeText, "");
        }
        else if (_changeText != null)
        {
            if (rowData.change < 0)
            {
                UIHelper.SetColor(_changeText, Color.red);
                UIHelper.SetText(_changeText, "(" + rowData.change + ")");
            }
            else
            {
                UIHelper.SetColor(_changeText, Color.green);
                UIHelper.SetText(_changeText, "(+" + rowData.change + ")");
            }
        }

        if (_stars != null)
        {
            for (int i = 0; i < _stars.Count; i++)
            {
                GameObjectUtils.SetActive(_stars[i], i < rowData.starsToShow);
            }
        }

    }
}
