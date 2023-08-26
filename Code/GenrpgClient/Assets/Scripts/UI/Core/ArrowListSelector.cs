using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;

using Services;
using UnityEngine;
using System.Dynamic;
using UnityEngine.UI;
using System.Configuration;
using Genrpg.Shared.Interfaces;
using UI.Screens.Constants;

public class ArrowListSelector : BaseBehaviour
{
    [SerializeField]
    private Text _infoText;
    [SerializeField]
    private Button _prevButton;
    [SerializeField]
    private Button _nextButton;

    int currentIndex = 0;

    private List<IInfo> _items = null;

    private IScreen _screen = null;
    public void Init<T>(UnityGameState gs, List<T> items, IScreen screen, T currentItem = null) where T : class, IInfo
    {
        if (items == null)
        {
            GameObject.Destroy(gameObject);
            return;
        }

        UIHelper.SetButton(_prevButton, screen.GetAnalyticsName(), ClickPrev);
        UIHelper.SetButton(_nextButton, screen.GetAnalyticsName(), ClickNext);

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
        if (_infoText == null)
        {
            return;
        }

        if (_items == null || _items.Count < 1)
        {
            _infoText.text = "No Items";
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
        _infoText.text = _items[currentIndex].ShowInfo();

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
