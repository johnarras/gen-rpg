using System;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Spells.PlayerData.Spells;
using Genrpg.Shared.Spells.Messages;
using UnityEngine;
using Genrpg.Shared.Spells.Constants;

public class CombatTextUI : BaseBehaviour
{
    public const string UIPrefabName = "CombatText";

    public GText CombatText;
    public float _lifetimeSeconds = 1.0f;
    public float _pixelsPerFrame = 2.0f;

    private bool _isCrit = false;

    DateTime createTime;
    int frameCount = 0;


    public override void Init()
    {
        base.Init();
        AddUpdate(CombatTextUpdate, UpdateType.Regular);
    }

    public void Init(CombatText text)
    {
        if (CombatText == null)
        {
            return;
        }

        Transform oldParent =entity.transform.parent;
       entity.transform.parent = null;
       entity.transform.localScale = Vector3.one;
       entity.transform.parent = oldParent;
        _uiService.SetText(CombatText, text.Text);

        _isCrit = text.IsCrit;
       
        createTime = DateTime.UtcNow;

        textStartScale = CombatText.transform.localScale;

        if (CombatText != null)
        {
            float dmult = 0.03f;
            float dx = MathUtils.FloatRange(-20, 20, _rand)*dmult;
            float dy = MathUtils.FloatRange(0, 15, _rand)*dmult;
            CombatText.transform.localPosition += new Vector3(dx, dy, 0);

            switch (text.TextColor)
            {
                case CombatTextColors.Black:
                    CombatText.Color = Color.black;
                    break;
                case CombatTextColors.Red:
                    CombatText.Color = Color.red;
                    break;
                case CombatTextColors.Green:
                    CombatText.Color = Color.green;
                    break;
                case CombatTextColors.Yellow:
                    CombatText.Color = Color.yellow;
                    break;
                case CombatTextColors.Cyan:
                    CombatText.Color = Color.cyan;
                    break;
                case CombatTextColors.Blue:
                    CombatText.Color = Color.blue;
                    break;
                case CombatTextColors.Orange:
                    CombatText.Color = new Color(1, 0.5f, 0);
                    break;
                case CombatTextColors.White:
                    CombatText.Color = Color.white;
                    break;
                default:                    
                    CombatText.Color = Color.white;
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
            _clientEntityService.Destroy(entity);
            return;
        }

        if (CombatText != null)
        {
            CombatText.transform.localPosition += Vector3.up * _pixelsPerFrame * Time.deltaTime;
        }
    

        if (_isCrit)
        {
            if (frameCount <= critFrames)
            {
              
                CombatText.transform.localScale = textStartScale * (1 + critMaxSizeMult * frameCount / critFrames);
            }
            else if (frameCount <= critFrames*2)
            {
                CombatText.transform.localScale = textStartScale * (1 + critMaxSizeMult * ((critFrames * 2) - frameCount) / critFrames);
            }
            else
            {
                CombatText.transform.localScale = textStartScale;
            }
        }
        ++frameCount;
    }

}