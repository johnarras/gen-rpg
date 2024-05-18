
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Factions.Services;
using UI.Screens.Constants;
using System.Threading;
using Genrpg.Shared.Loot.Messages;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.MapObjects.MapObjectAddons.Constants;
using Genrpg.Shared.Crafting.Settings.Crafters;
using Genrpg.Shared.Units.Constants;

public class InteractUnit : InteractableObject
{
    protected ISharedFactionService _factionService;
    protected string crafterMousePointer = "";

    public override void Init(MapObject worldObj, GEntity go, CancellationToken token)
    {
        base.Init(worldObj, go, token);
    }

    protected override void OnInit()
    {
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
        else if (unit.AddonBits > 0)
        {
            if (_factionService.CanInteract(_gs.ch, unit.FactionTypeId))
            {

                if (unit.HasAddon(MapObjectAddonTypes.Vendor))
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
            if (unit.Stats.Curr(StatTypes.Health) > 0) // alive
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
                    UnitType unitType = _gameData.Get<UnitSettings>(_gs.ch).Get(unit.EntityId);
                    if (unitType != null)
                    {
                        TribeType tribe = _gameData.Get<TribeSettings>(_gs.ch).Get(unitType.TribeTypeId);
                        if (tribe != null && tribe.LootCrafterTypeId > 0)
                        {
                            CrafterType ctype = _gameData.Get<CraftingSettings>(_gs.ch).Get(tribe.LootCrafterTypeId);
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
            _playerManager.SetCurrentTarget(unit.Id);
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

        if (_factionService.CanInteract(_gs.ch,unit.FactionTypeId))
        {
            if (unit.HasAddon(MapObjectAddonTypes.Vendor))
            {
                _screenService.Open(_gs, ScreenId.Quest, unit);
            }

            return;
        }

        if (unit.HasFlag(UnitFlags.IsDead))
        {
            if (UnitUtils.AttackerInfoMatchesObject(unit.GetFirstAttacker(),_gs.ch))
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
                _playerManager.SetCurrentTarget(unit.Id);
            }
        }

    }
}
