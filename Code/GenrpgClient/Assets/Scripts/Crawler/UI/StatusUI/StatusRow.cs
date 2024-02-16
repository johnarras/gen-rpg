using Assets.Scripts.UI.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.UnitEffects.Services;
using Genrpg.Shared.Units.Entities;

namespace Assets.Scripts.UI.Crawler.StatusUI
{
    public class CrawlerStatusRow : BaseBehaviour
    {
        private IStatusEffectService _statusEffectService;

        public GText Index;
        public GText Name;
        public GText Class;
        public GText Level;
        public GText Health;
        public GText Mana;
        public GText StatusEffects;


        private PartyMember _partyMember;
        private int _index = -1;
        public void Init(PartyMember partyMember, int index)
        {
            _partyMember = partyMember;
            _index = index;

            if (_index > 0)
            {
                _uiService.SetText(Index, _index.ToString());
            }
            else
            {
                _uiService.SetText(Index, "");
                _uiService.SetText(Name, "Name");
                if (Name != null)
                {
                    Name.alignment = TMPro.TextAlignmentOptions.Center;
                }
                _uiService.SetText(Class, "Cl");
                _uiService.SetText(Level, "Lev");
                _uiService.SetText(Health, "Health");
                _uiService.SetText(Mana, "Mana");
                _uiService.SetText(StatusEffects, "Status");
            }

            UpdateText();
        }

        public void UpdateText()
        {
            if (_index == 0)
            {
                return;
            }
            if (_partyMember == null)
            {
                _uiService.SetText(Name, "");
                _uiService.SetText(Health, "");
                _uiService.SetText(Mana, "");
                _uiService.SetText(Class, "");
                _uiService.SetText(Level, "");
                return;
            }
            else
            {
                _uiService.SetText(Name, _partyMember.Name);
                _uiService.SetText(Level, _partyMember.Level.ToString());
                long currHp = _partyMember.Stats.Curr(StatTypes.Health);
                long maxHp = _partyMember.Stats.Max(StatTypes.Health);
                _uiService.SetText(Health, currHp + "/" + maxHp);
                long currMana = _partyMember.Stats.Curr(StatTypes.Mana);
                long maxMana = _partyMember.Stats.Max(StatTypes.Mana);

                if (maxMana < 1)
                {
                    _uiService.SetText(Mana, "");
                }
                else
                {
                    _uiService.SetText(Mana, currMana + "/" + maxMana);
                }

                ClassSettings classSettings = _gs.data.Get<ClassSettings>(_gs.ch);
                string classText = "";

                foreach (UnitClass uc in _partyMember.Classes)
                {
                    Class cl = classSettings.Get(uc.ClassId);

                    if (!string.IsNullOrEmpty(classText))
                    {
                        classText += "/";
                    }
                    classText += cl.Abbrev;
                }

                _uiService.SetText(Class, classText);

                _uiService.SetText(StatusEffects, _statusEffectService.ShowStatusEffects(_gs, _partyMember, true));
            }
        }
    }
}
