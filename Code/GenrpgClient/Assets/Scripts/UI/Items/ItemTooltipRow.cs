using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;

public class ItemTooltipRow : BaseBehaviour
{
    public GText TextRow;
    public GText ChangeText;
    public List<GEntity> Stars;

    public void Init(UnityGameState gs, ItemTooltipRowData rowData)
    {
        if (rowData == null)
        {
            GEntityUtils.Destroy(entity);
            return;
        }

        _uiService.SetText(TextRow, rowData.text);

        if (TextRow != null)
        {
            if (rowData.isCurrent)
            {
                _uiService.SetColor(TextRow, GColor.white);
            }
            else
            {
                _uiService.SetColor(TextRow, GColor.gray);
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
                _uiService.SetColor(ChangeText, GColor.red);
                _uiService.SetText(ChangeText, "(" + rowData.change + ")");
            }
            else
            {
                _uiService.SetColor(ChangeText, GColor.green);
                _uiService.SetText(ChangeText, "(+" + rowData.change + ")");
            }
        }

        if (Stars != null)
        {
            for (int i = 0; i < Stars.Count; i++)
            {
                GEntityUtils.SetActive(Stars[i], i < rowData.starsToShow);
            }
        }

    }
}
