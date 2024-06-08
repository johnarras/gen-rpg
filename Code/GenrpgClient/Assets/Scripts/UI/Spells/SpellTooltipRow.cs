
public class SpellTooltipRow : BaseBehaviour
{
    public GText TextRow;

    public void Init(SpellTooltipRowData rowData)
    {
        if (rowData == null)
        {
            GEntityUtils.Destroy(entity);
            return;
        }

        _uIInitializable.SetText(TextRow, rowData.text);

    }
}
