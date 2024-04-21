
public class CoreUnitUI : BaseBehaviour
{
    public GText NameText;
    public GText LevelText;

    public void Init(string name, long level)
    {
        _uIInitializable.SetText(NameText, name);
        _uIInitializable.SetText(LevelText, level.ToString());
    }
}