using System;
using GEntity = UnityEngine.GameObject;
using UnityEngine.UI; // FIX
using UnityEngine; // FIX

public class CollapsableScrollView : BaseBehaviour
{
    public float ExpandTime = 0.2f;

    protected float _minScrollViewSize = 20;
    protected float _targetScrollViewSize = 0;
    protected float _currentScrollViewSize = 1;
    protected float _maxScrollViewSize = 0;
    protected float _buttonHeight = 0;

    protected ScrollRect _collapsingRect = null;
    protected VerticalLayoutGroup _collapsingLayout = null;

    CollapsingScrollViewGroup ParentScrollGroup;


    public override void Init()
    {
        base.Init();
        AddUpdate(ScrollUpdate, UpdateType.Regular);
    }

    public void Init(CollapsingScrollViewGroup parentGroup)
    {
        ParentScrollGroup = parentGroup;
        _collapsingRect = GetComponent<ScrollRect>();

        if (_collapsingRect != null && _collapsingRect.content != null)
        {
            _collapsingLayout = _collapsingRect.content.GetComponent<VerticalLayoutGroup>();
        }      
        CalculateContentSize();
        _targetScrollViewSize = _minScrollViewSize;
        _currentScrollViewSize = _targetScrollViewSize - 1;
    }


    public void CalculateContentSize()
    {
       
        if (_collapsingLayout == null)
        {
            return;
        }

        _maxScrollViewSize = 0;

        for (int c = 0; c < _collapsingLayout.transform().childCount; c++)
        {
            GEntity go = _collapsingLayout.transform().GetChild(c).entity();
            RectTransform rect = go.GetComponent<RectTransform>();
            if (rect != null)
            {
                _maxScrollViewSize += rect.rect.height;
            }
            if (c == 0)
            {
                _minScrollViewSize = rect.rect.height;
            }
        }
        

    }

    public void ToggleScrollViewSize()
    {
        CalculateContentSize();
        if (_targetScrollViewSize <= _minScrollViewSize)
        {
            _targetScrollViewSize = _maxScrollViewSize;
        }
        else
        {
            _targetScrollViewSize = _minScrollViewSize;
        }
    }

    void ScrollUpdate()
    {
        if (_targetScrollViewSize == _currentScrollViewSize || _collapsingRect == null)
        {
            return;
        }

        int transitionFrames = (int)Math.Max(1, AppUtils.TargetFrameRate * ExpandTime);

        float expansionPerStep = (_maxScrollViewSize-_minScrollViewSize) / (transitionFrames);


        float oldSize = _currentScrollViewSize;

        if (_currentScrollViewSize < _targetScrollViewSize)
        {
            _currentScrollViewSize += expansionPerStep;
            if (_currentScrollViewSize > _targetScrollViewSize)
            {
                _currentScrollViewSize = _targetScrollViewSize;
            }
        }
        else
        {
            _currentScrollViewSize -= expansionPerStep;
            if (_currentScrollViewSize < _targetScrollViewSize)
            {
                _currentScrollViewSize = _targetScrollViewSize;
            }
        }

        float newSize = _currentScrollViewSize;

        float sizeDelta = newSize - oldSize;

        RectTransform crect = _collapsingRect.GetComponent<RectTransform>();
        if (crect != null)
        {
            crect.sizeDelta = GVector3.Create(crect.rect.width, _currentScrollViewSize, 0);
        }

        if (_collapsingLayout != null)
        {
            
            _collapsingLayout.enabled = false;
            _collapsingLayout.enabled = true;
        }

        if (ParentScrollGroup != null)
        {
            ParentScrollGroup.OnUpdateChild();
        }

    }

    public float GetCurrentHeight()
    {
        return _currentScrollViewSize;
    }
}