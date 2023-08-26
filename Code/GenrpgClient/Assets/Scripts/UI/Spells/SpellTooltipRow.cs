using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;

using Services;
using UnityEngine;
using UnityEngine.UI;


public class SpellTooltipRow : BaseBehaviour
{
    private Text _textRow;

    public void Init(UnityGameState gs, SpellTooltipRowData rowData)
    {
        if (rowData == null)
        {
            GameObject.Destroy(gameObject);
            return;
        }

        UIHelper.SetText(_textRow, rowData.text);

    }
}
