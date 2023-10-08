using System.Collections.Generic;
using Genrpg.Shared.Interfaces;

public class ArrowListSelector : BaseBehaviour
{
    
    public GText InfoText;
    public GButton PrevButton;
    public GButton NExtButton;

    int currentIndex = 0;

    private List<IInfo> _items = null;

    private IScreen _screen = null;
    public void Init<T>(UnityGameState gs, List<T> items, IScreen screen, T currentItem = null) where T : class, IInfo
    {
        if (items == null)
        {
            GEntityUtils.Destroy(entity);
            return;
        }

        UIHelper.SetButton(PrevButton, screen.GetAnalyticsName(), ClickPrev);
        UIHelper.SetButton(NExtButton, screen.GetAnalyticsName(), ClickNext);

        _screen = screen;

        _items = new List<IInfo>();

        foreach (T item in items)
        {
            _items.Add(item);
        }
        if (currentItem != null)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] == currentItem)
                {
                    currentIndex = i;
                    break;
                }
            }
        }

        SetIndex(currentIndex);
    }

    public T GetSelectedItem<T>() where T : class, IInfo
    {
        if (_items == null)
        {
            return null;
        }

        if (currentIndex < 0 || currentIndex >= _items.Count)
        {
            return null;
        }

        return _items[currentIndex] as T;
    }
       


    public void ClickPrev()
    {
        SetIndex(currentIndex - 1);
    }

    public void ClickNext()
    {
        SetIndex(currentIndex + 1);
    }

    private void SetIndex(int index)
    {
        int oldIndex = currentIndex;
        if (InfoText == null)
        {
            return;
        }

        if (_items == null || _items.Count < 1)
        {
            UIHelper.SetText(InfoText, "No Items");
        }
        if (index >= _items.Count)
        {
            index = _items.Count - 1;
        }

        if (index < 0)
        {
            index = 0;
        }

        currentIndex = index;
        UIHelper.SetText(InfoText, _items[currentIndex].ShowInfo());

        if (currentIndex != oldIndex && _screen != null)
        {
            _screen.OnInfoChanged();
        }
    }

    public void SetToId (long id)
    {
        for (int i =0; i < _items.Count; i++)
        {
            if (_items[i].GetId() == id)
            {
                SetIndex(i);
                break;
            }
        }
    }
        


}
