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


    public override void Initialize(UnityGameState gs)
    {
        base.Initialize(gs);
        AddUpdate(CombatTextUpdate, UpdateType.Regular);
    }

    public void Init(UnityGameState gs, CombatText text)
    {
        if (CombatText == null)
        {
            return;
        }

        Transform oldParent =entity.transform().parent;
       entity.transform().parent = null;
       entity.transform().localScale = GVector3.onePlatform;
       entity.transform().parent = oldParent;
        _uIInitializable.SetText(CombatText, text.Text);

        _isCrit = text.IsCrit;
       
        createTime = DateTime.UtcNow;

        textStartScale = GVector3.Create(CombatText.transform().localScale);

        if (CombatText != null)
        {
            float dmult = 0.03f;
            float dx = MathUtils.FloatRange(-20, 20, gs.rand)*dmult;
            float dy = MathUtils.FloatRange(0, 15, gs.rand)*dmult;
            CombatText.transform().localPosition += GVector3.Create(dx, dy, 0);

            switch (text.TextColor)
            {
                case CombatTextColors.Black:
                    CombatText.Color = GColor.black;
                    break;
                case CombatTextColors.Red:
                    CombatText.Color = GColor.red;
                    break;
                case CombatTextColors.Green:
                    CombatText.Color = GColor.green;
                    break;
                case CombatTextColors.Yellow:
                    CombatText.Color = GColor.yellow;
                    break;
                case CombatTextColors.Cyan:
                    CombatText.Color = GColor.cyan;
                    break;
                case CombatTextColors.Blue:
                    CombatText.Color = GColor.blue;
                    break;
                case CombatTextColors.Orange:
                    CombatText.Color = GColor.Create(1, 0.5f, 0);
                    break;
                case CombatTextColors.White:
                    CombatText.Color = GColor.white;
                    break;
                default:                    
                    CombatText.Color = GColor.white;
                    break;
            }
        }
    }


    float critMaxSizeMult = 1.5f;
    int critFrames = 5;
    GVector3 textStartScale = GVector3.one;
    private void CombatTextUpdate()
    {
        if ((DateTime.UtcNow-createTime).TotalSeconds >= _lifetimeSeconds)
        {
            GEntityUtils.Destroy(entity);
            return;
        }

        if (CombatText != null)
        {
            CombatText.transform().localPosition += GVector3.upPlatform * _pixelsPerFrame * Time.deltaTime;
        }
    

        if (_isCrit)
        {
            if (frameCount <= critFrames)
            {
                CombatText.transform().localScale = GVector3.Create(textStartScale * (1 + critMaxSizeMult * frameCount / critFrames));
            }
            else if (frameCount <= critFrames*2)
            {
                CombatText.transform().localScale = GVector3.Create(textStartScale * (1 + critMaxSizeMult * ((critFrames * 2) - frameCount) / critFrames));
            }
            else
            {
                CombatText.transform().localScale = GVector3.Create(textStartScale);
            }
        }
        ++frameCount;
    }

}