
public class SpellTooltipRow : BaseBehaviour
{
    public GText TextRow;

    public void Init(SpellTooltipRowData rowData)
    {
        if (rowData == null)
        {
            _clientEntityService.Destroy(entity);
            return;
        }

        _uiService.SetText(TextRow, rowData.text);

    }
}
