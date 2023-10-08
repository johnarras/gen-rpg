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

        UIHelper.SetText(TextRow, rowData.text);

        if (TextRow != null)
        {
            if (rowData.isCurrent)
            {
                UIHelper.SetColor(TextRow, GColor.white);
            }
            else
            {
                UIHelper.SetColor(TextRow, GColor.gray);
            }
        }
        if (rowData.change == 0)
        {
            UIHelper.SetText(ChangeText, "");
        }
        else if (ChangeText != null)
        {
            if (rowData.change < 0)
            {
                UIHelper.SetColor(ChangeText, GColor.red);
                UIHelper.SetText(ChangeText, "(" + rowData.change + ")");
            }
            else
            {
                UIHelper.SetColor(ChangeText, GColor.green);
                UIHelper.SetText(ChangeText, "(+" + rowData.change + ")");
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
