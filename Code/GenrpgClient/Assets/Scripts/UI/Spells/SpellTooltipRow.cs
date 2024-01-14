
public class SpellTooltipRow : BaseBehaviour
{
    public GText TextRow;

    public void Init(UnityGameState gs, SpellTooltipRowData rowData)
    {
        if (rowData == null)
        {
            GEntityUtils.Destroy(entity);
            return;
        }

        _uiService.SetText(TextRow, rowData.text);

    }
}
