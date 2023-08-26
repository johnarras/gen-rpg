using System;
using UnityEngine;
using UnityEngine.UI;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Spells.Messages;

public class CombatTextUI : BaseBehaviour
{
    public const string UIPrefabName = "CombatText";

    [SerializeField]
    private Text _text;

    [SerializeField]
    private float _lifetimeSeconds = 1.0f;

    [SerializeField]
    private float _pixelsPerFrame = 2.0f;


    private bool _isCrit = false;

    DateTime createTime;
    int frameCount = 0;


    public override void Initialize(UnityGameState gs)
    {
        base.Initialize(gs);
        AddUpdate(CombatTextUpdate, UpdateType.Regular);
    }

    public void Init(UnityGameState gs, CombatText text)
    {
        if (_text == null)
        {
            return;
        }

        Transform oldParent = transform.parent;
        transform.parent = null;
        transform.localScale = Vector3.one;
        transform.parent = oldParent;
        UIHelper.SetText(_text, text.Text);

        _isCrit = text.IsCrit;
       
        createTime = DateTime.UtcNow;

        textStartScale = _text.transform.localScale;

        if (_text != null)
        {
            float dmult = 0.03f;
            float dx = MathUtils.FloatRange(-20, 20, gs.rand)*dmult;
            float dy = MathUtils.FloatRange(0, 15, gs.rand)*dmult;
            _text.transform.localPosition += new Vector3(dx, dy, 0);

            switch (text.TextColor)
            {
                case CombatTextColors.Black:
                    _text.color = Color.black;
                    break;
                case CombatTextColors.Red:
                    _text.color = Color.red;
                    break;
                case CombatTextColors.Green:
                    _text.color = Color.green;
                    break;
                case CombatTextColors.Yellow:
                    _text.color = Color.yellow;
                    break;
                case CombatTextColors.Cyan:
                    _text.color = Color.cyan;
                    break;
                case CombatTextColors.Blue:
                    _text.color = Color.blue;
                    break;
                case CombatTextColors.Orange:
                    _text.color = new Color(1, 0.5f, 0);
                    break;
                case CombatTextColors.White:
                    _text.color = Color.white;
                    break;
                default:                    
                    _text.color = Color.white;
                    break;
            }
        }
    }


    float critMaxSizeMult = 1.5f;
    int critFrames = 5;
    Vector3 textStartScale = Vector3.one;
    private void CombatTextUpdate()
    {
        if ((DateTime.UtcNow-createTime).TotalSeconds >= _lifetimeSeconds)
        {
            GameObject.Destroy(gameObject);
            return;
        }

        if (_text != null)
        {
            _text.transform.localPosition += Vector3.up * _pixelsPerFrame * Time.deltaTime;
        }
    

        if (_isCrit)
        {
            if (frameCount <= critFrames)
            {
                _text.transform.localScale = textStartScale * (1 + critMaxSizeMult * frameCount / critFrames);
            }
            else if (frameCount <= critFrames*2)
            {
                _text.transform.localScale = textStartScale * (1 + critMaxSizeMult * ((critFrames * 2) - frameCount) / critFrames);
            }
            else
            {
                _text.transform.localScale = textStartScale;
            }
        }
        ++frameCount;
    }

}