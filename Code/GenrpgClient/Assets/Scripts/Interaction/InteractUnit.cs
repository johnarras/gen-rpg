
using UnityEngine;
using Genrpg.Shared.Units.Entities;

using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Factions.Services;
using Genrpg.Shared.Crafting.Entities;
using UI.Screens.Constants;
using System.Threading;
using Genrpg.Shared.Loot.Messages;

public class InteractUnit : InteractableObject
{
    protected ISharedFactionService _factionService;
    protected string crafterMousePointer = "";
    private void Start()
    {
    }

    public override void Init(MapObject worldObj, GameObject go, CancellationToken token)
    {
        base.Init(worldObj, go, token);
    }

    protected override void InnerOnEnable()
    {
        HideGlow(0, false);
    }

    protected override void _OnPointerEnter()
    {

        if (!(_mapObj is Unit unit))
        {
            return;
        }

        if (unit.IsPlayer())
        { 
            return;
        }
        else if (unit.NPCType != null)
        {
            if (_factionService.CanInteract(_gs, _gs.ch, unit.FactionTypeId))
            {

                if (unit.NPCType.ItemCount > 0)
                {
                    Cursors.SetCursor(Cursors.Shop);
                }
                else
                {
                    Cursors.SetCursor(Cursors.Chat);
                }
                return;
            }
        }
        else 
        {
            if (unit.Stats.Curr(StatType.Health) > 0) // alive
            {
                Cursors.SetCursor(Cursors.Fight);
            }
            else if (unit.Loot != null && 
                unit.Loot.Count > 0)
            {
                Cursors.SetCursor(Cursors.Interact);
            }
            else if (unit.SkillLoot != null && unit.SkillLoot.Count > 0)
            {
                if (string.IsNullOrEmpty(crafterMousePointer))
                {
                    UnitType unitType = _gs.data.GetGameData<UnitSettings>().GetUnitType(unit.EntityId);
                    if (unitType != null)
                    {
                        TribeType tribe = _gs.data.GetGameData<UnitSettings>().GetTribeType(unitType.TribeTypeId);
                        if (tribe != null && tribe.LootCrafterTypeId > 0)
                        {
                            CrafterType ctype = _gs.data.GetGameData<CraftingSettings>().GetCrafterType(tribe.LootCrafterTypeId);
                            if (ctype != null && !string.IsNullOrEmpty(ctype.MousePointer))
                            {
                                crafterMousePointer = ctype.MousePointer;
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(crafterMousePointer))
                    {
                        crafterMousePointer = Cursors.Default;
                    }
                }
                Cursors.SetCursor(crafterMousePointer);

            }
        }
    }

    protected override void _LeftClick(float distance)
    {
        if (!CanInteract())
        {
            return;
        }

        if (_mapObj is Unit unit)
        {
            PlayerController.Instance.SetCurrentTarget(unit);
        }
    }

    protected override void _OnPointerExit()
    {
        Cursors.SetCursor(Cursors.Default);
    }

    protected override void _RightClick(float distance)
    {

        if (!CanInteract())
        {
            return;
        }

        if (!(_mapObj is Unit unit))
        {
            return;
        }

        if (_factionService.CanInteract(_gs,_gs.ch,unit.FactionTypeId))
        {
            if (unit.NPCType != null)
            {
                if (unit.NPCType.ItemCount > 0)
                {
                    _screenService.Open(_gs, ScreenId.Quest, unit);
                }

                return;
            }

            return;
        }

        if (unit.HasFlag(UnitFlags.IsDead))
        {
            if (unit.GetFirstAttacker() == _gs.ch.Id)
            {
                if (unit.Loot != null && unit.Loot.Count > 0)
                {
                    _networkService.SendMapMessage(new LootCorpse() { UnitId = unit.Id });
                    unit.Loot = null;
                    if (unit.SkillLoot == null || unit.SkillLoot.Count < 1)
                    {
                        HideGlow(4.0f, false);
                        Cursors.SetCursor(Cursors.Default);
                    }
                }
                else if (unit.SkillLoot != null && unit.SkillLoot.Count > 0)
                {
                    _networkService.SendMapMessage(new SkillLootCorpse() { UnitId = unit.Id });
                }
            }
            else
            {
                HideGlow(0, false);
            }
        }
        else
        {
            if (unit.FactionTypeId != _gs.ch.FactionTypeId)
            {
                PlayerController.Instance.SetCurrentTarget(unit);
            }
        }

    }
}
