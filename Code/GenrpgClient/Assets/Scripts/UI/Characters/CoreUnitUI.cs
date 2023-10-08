
public class CoreUnitUI : BaseBehaviour
{
    public GText NameText;
    public GText LevelText;

    public void Init(string name, long level)
    {
        UIHelper.SetText(NameText, name);
        UIHelper.SetText(LevelText, level.ToString());
    }
}