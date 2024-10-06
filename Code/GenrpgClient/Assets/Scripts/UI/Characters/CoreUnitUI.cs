
public class CoreUnitUI : BaseBehaviour
{
    public GText NameText;
    public GText LevelText;

    public void Init(string name, long level)
    {
        _uiService.SetText(NameText, name);
        _uiService.SetText(LevelText, level.ToString());
    }
}