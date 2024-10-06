using System.Collections.Generic;
using UnityEngine;

public class ItemTooltipRow : BaseBehaviour
{
    public GText TextRow;
    public GText ChangeText;
    public List<GameObject> Stars;

    public void Init(ItemTooltipRowData rowData)
    {
        if (rowData == null)
        {
            _gameObjectService.Destroy(entity);
            return;
        }

        _uiService.SetText(TextRow, rowData.text);

        if (TextRow != null)
        {
            if (rowData.isCurrent)
            {
                _uiService.SetColor(TextRow, Color.white);
            }
            else
            {
                _uiService.SetColor(TextRow, Color.gray);
            }
        }
        if (rowData.change == 0)
        {
            _uiService.SetText(ChangeText, "");
        }
        else if (ChangeText != null)
        {
            if (rowData.change < 0)
            {
                _uiService.SetColor(ChangeText, Color.red);
                _uiService.SetText(ChangeText, "(" + rowData.change + ")");
            }
            else
            {
                _uiService.SetColor(ChangeText, Color.green);
                _uiService.SetText(ChangeText, "(+" + rowData.change + ")");
            }
        }

        if (Stars != null)
        {
            for (int i = 0; i < Stars.Count; i++)
            {
                _gameObjectService.SetActive(Stars[i], i < rowData.starsToShow);
            }
        }

    }
}
