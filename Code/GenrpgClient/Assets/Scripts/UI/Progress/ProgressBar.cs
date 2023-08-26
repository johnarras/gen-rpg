using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Genrpg.Shared.Core.Entities;

using Services;
using Entities;
using Genrpg.Shared.Utils;

public class ProgressBar : BaseBehaviour
{
    public enum ShowTextOption
    {
        Hide = 0,
        Current = 1,
        CurrentOverMax = 2,
        Custom=3,
        Percent=4,
    }

    [SerializeField]
    private Animator _animator;
    [SerializeField]
    private Image _frontBar;
    [SerializeField]
    private Image _backBar;
    [SerializeField]
    private int _fillTicks = 0;
    [SerializeField]
    private float _pulsePercent;
    [SerializeField]
    private float _minBarWidth;
    [SerializeField]
    private float _maxBarWidth;
    [SerializeField]
    private Text _barText;
    [SerializeField]
    private ShowTextOption _textOption = ShowTextOption.CurrentOverMax;

    private long _minValue = 0;
    private long _maxValue = 1;
    private long _currValue = 0;
    private long _targetValue = 0;
    private long _oldValue = -999999999999;
   
    private string _customText = "";

    public long GetMinValue()
    {
        return _minValue;
    }

    public long GetMaxValue()
    {
        return _maxValue;
    }

    public long GetCurrValue()
    {
        return _currValue;
    }

    public override void Initialize(UnityGameState gs)
    {
        base.Initialize(gs);
        AddUpdate(ProgressUpdate, UpdateType.Regular);
    }

    /// <summary>
    /// Initialize bar with min, max, cur values and how text will be shown.
    /// </summary>
    /// <param name="gs"></param>
    /// <param name="minValue"></param>
    /// <param name="maxValue"></param>
    /// <param name="currValue"></param>
    /// <param name="textOpt"></param>
    /// <param name="fillTicks"></param>
    public void InitRange (UnityGameState gs, long minValue, long maxValue, long currValue)
    {
        _minValue = minValue;
        _maxValue = maxValue;
        _currValue = currValue;
        _targetValue = currValue;
        _oldValue = Math.Min(-1, _currValue - 1);
        ShowBar();

    }

    /// <summary>
    /// This shows the bar(s).
    /// If there is one bar, it tracks the _currValue as it moves, but if there are two bars, the
    /// front bar tracks the 
    /// </summary>
    public void ShowBar()
    {
        ShowText();
        if (_frontBar == null)
        {
            return;
        }

        // Front bar only, always shows curr value
        if (_backBar == null)
        {
            ShowOneBar(_frontBar, _currValue);
        }
        else
        {
            long frontValue = Math.Min(_currValue, _targetValue);
            long backValue = Math.Max(_currValue, _targetValue);
            ShowOneBar(_frontBar, frontValue);
            ShowOneBar(_backBar, backValue);
        }
    }

    private double _currPct = -1;
    private bool _didInit = false;
    private void ShowOneBar (Image bar, long value)
    { 
        if (bar == null)
        {
            return;
        }

        double currPct = 1.0;

        if (_maxValue > _minValue)
        {
            currPct = (1.0 * (value - _minValue) / (_maxValue - _minValue));
        }

        currPct = MathUtils.Clamp(0, currPct, 1);
        
        if (_currPct == currPct && _didInit)
        {
            return;
        }
        _currPct = currPct;
        _didInit = true;
        if (currPct <= 0 && bar.IsActive())
        {
            GameObjectUtils.SetActive(bar, false);
        }
        else if (currPct > 0 && !bar.IsActive())
        {
            GameObjectUtils.SetActive(bar, true);
        }
        RectTransform rectTransform = GetComponent<RectTransform>();
        _maxBarWidth = rectTransform.rect.width;
        if (currPct >= 0)
        {
            int barWidth = (int)(_minBarWidth + currPct * (_maxBarWidth - _minBarWidth));
            bar.rectTransform.sizeDelta = new Vector2((float)(barWidth), bar.rectTransform.sizeDelta.y);
        }
    }

    private void ShowText()
    {
        if (_barText == null)
        {
            return;
        }

        if (_textOption == ShowTextOption.Hide)
        {
            _barText.text = "";
        }
        else if (_textOption == ShowTextOption.Current)
        {
            _barText.text = _currValue.ToString();
        }
        else if (_textOption == ShowTextOption.CurrentOverMax)
        {
            _barText.text = _currValue + "/" + _maxValue;
        }
        else if (_textOption == ShowTextOption.Custom)
        {
            _barText.text = _customText;
        }
        else if (_textOption == ShowTextOption.Percent)
        {
            if (_maxValue > _minValue)
            {
                double pct = 100.0 * (_currValue - _minValue) / (_maxValue - _minValue);
                _barText.text = (int)(pct) + "%";
            }
        }
    }

    public void SetValue(UnityGameState gs, long value, string customText = "")
    {
        _targetValue = value;
        _customText = customText;
    }


    private float fillFraction = 0;
    private float currFillFraction = 0;
    void ProgressUpdate()
    {
        if (_currValue == _targetValue)
        {
            return;
        }

        long diff = _targetValue - _currValue;

        long fillSpeed = _maxValue - _minValue;

        long startFillSpeed = fillSpeed;

        if (_fillTicks > 1)
        {
            fillSpeed /= _fillTicks;
            if (fillSpeed == 0)
            {
                if (_currValue == _oldValue)
                {
                    fillSpeed = 1;
                }
                else
                {
                    fillFraction = 1.0f * startFillSpeed / _fillTicks;
                    currFillFraction += fillFraction;
                }
            }
            else if (currFillFraction >= 1)
            {
                currFillFraction -= 1;
                fillSpeed = 1;
            }
        }

        if (_currValue < _targetValue)
        {
            _currValue += fillSpeed;
            if (_currValue > _targetValue)
            {
                _currValue = _targetValue;
            }
        }
        else if (_currValue > _targetValue)
        {
            _currValue -= fillSpeed;
            if (_currValue < _targetValue)
            {
                _currValue = _targetValue;
            }
        }
        if (_currValue != _oldValue)
        {
            _oldValue = _currValue;
            ShowBar();
        }
        ShowPulse();
    }


    private void ShowPulse()
    {
        if (_animator == null)
        {
            return;
        }

        double currPct = 1.0 * (_currValue - _minValue) / (_maxValue - _minValue);
        _animator.SetBool("Pulse", currPct <= _pulsePercent);
    }



}
