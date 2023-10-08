
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

        UIHelper.SetText(TextRow, rowData.text);

    }
}
