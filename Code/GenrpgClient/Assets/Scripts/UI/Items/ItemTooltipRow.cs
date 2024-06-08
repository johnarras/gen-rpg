using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;

public class ItemTooltipRow : BaseBehaviour
{
    public GText TextRow;
    public GText ChangeText;
    public List<GEntity> Stars;

    public void Init(ItemTooltipRowData rowData)
    {
        if (rowData == null)
        {
            GEntityUtils.Destroy(entity);
            return;
        }

        _uIInitializable.SetText(TextRow, rowData.text);

        if (TextRow != null)
        {
            if (rowData.isCurrent)
            {
                _uIInitializable.SetColor(TextRow, GColor.white);
            }
            else
            {
                _uIInitializable.SetColor(TextRow, GColor.gray);
            }
        }
        if (rowData.change == 0)
        {
            _uIInitializable.SetText(ChangeText, "");
        }
        else if (ChangeText != null)
        {
            if (rowData.change < 0)
            {
                _uIInitializable.SetColor(ChangeText, GColor.red);
                _uIInitializable.SetText(ChangeText, "(" + rowData.change + ")");
            }
            else
            {
                _uIInitializable.SetColor(ChangeText, GColor.green);
                _uIInitializable.SetText(ChangeText, "(+" + rowData.change + ")");
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
