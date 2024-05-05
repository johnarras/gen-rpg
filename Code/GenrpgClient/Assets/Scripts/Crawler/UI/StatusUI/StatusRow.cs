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
        public GText Summons;
        public GText StatusEffects;


        private PartyMember _partyMember;
        private int _index = -1;
        public void Init(PartyMember partyMember, int index)
        {
            _partyMember = partyMember;
            _index = index;

            if (_index > 0)
            {
                _uIInitializable.SetText(Index, _index.ToString());
            }
            else
            {
                _uIInitializable.SetText(Index, "");
                _uIInitializable.SetText(Name, "Name");
                if (Name != null)
                {
                    Name.alignment = TMPro.TextAlignmentOptions.Center;
                }
                _uIInitializable.SetText(Class, "Cl");
                _uIInitializable.SetText(Level, "Lev");
                _uIInitializable.SetText(Health, "Health");
                _uIInitializable.SetText(Mana, "Mana");
                _uIInitializable.SetText(Summons, "Summon");
                _uIInitializable.SetText(StatusEffects, "Status");

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
                _uIInitializable.SetText(Name, "");
                _uIInitializable.SetText(Health, "");
                _uIInitializable.SetText(Mana, "");
                _uIInitializable.SetText(Class, "");
                _uIInitializable.SetText(Level, "");
                _uIInitializable.SetText(Summons, "");
                _uIInitializable.SetText(StatusEffects, "");
                return;
            }
            else
            {
                _uIInitializable.SetText(Name, _partyMember.Name);
                _uIInitializable.SetText(Level, _partyMember.Level.ToString());
                long currHp = _partyMember.Stats.Curr(StatTypes.Health);
                long maxHp = _partyMember.Stats.Max(StatTypes.Health);
                _uIInitializable.SetText(Health, currHp + "/" + maxHp);
                long currMana = _partyMember.Stats.Curr(StatTypes.Mana);
                long maxMana = _partyMember.Stats.Max(StatTypes.Mana);

                if (maxMana < 1)
                {
                    _uIInitializable.SetText(Mana, "");
                }
                else
                {
                    _uIInitializable.SetText(Mana, currMana + "/" + maxMana);
                }

                ClassSettings classSettings = _gameData.Get<ClassSettings>(_gs.ch);
                string classText = "";

                foreach (UnitClass uc in _partyMember.Classes)
                {
                    Class cl = classSettings.Get(uc.ClassId);

                    if (cl == null)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(classText))
                    {
                        classText += "/";
                    }
                    classText += cl.Abbrev;
                }

                _uIInitializable.SetText(Class, classText);

                _uIInitializable.SetText(StatusEffects, _statusEffectService.ShowStatusEffects(_gs, _partyMember, true));


                if (_partyMember.Summons != null && _partyMember.Summons.Count > 0)
                {
                    _uIInitializable.SetText(Summons, _partyMember.Summons[0].Name);
                }
                else
                {
                    _uIInitializable.SetText(Summons, "");
                }
            }
        }
    }
}
