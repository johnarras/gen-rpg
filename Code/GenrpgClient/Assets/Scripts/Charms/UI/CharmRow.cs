﻿using Genrpg.Shared.Charms.Constants;
using Genrpg.Shared.Charms.PlayerData;
using Genrpg.Shared.Charms.Services;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Charms
{
    public class CharmRow : BaseBehaviour
    {
        public GText HashText;
        public GText Bonuses;
        public GText UseType;
        public GText UseIdName;
        private ICharmService _charmService = null;

        private PlayerCharm _charm = null;
        public void Init(PlayerCharm charm)
        {
            _charm = charm;

            ShowData(_charm);
        }

        private void ShowData(PlayerCharm charm)
        {
            _uiService.SetText(HashText, charm.Hash);
            List<string> bonuses = _charmService.PrintBonuses(charm.Bonuses.FirstOrDefault());

            StringBuilder sb = new StringBuilder();

            foreach (string bonus in bonuses)
            {
                sb.AppendLine(bonus);
            }

            _uiService.SetText(Bonuses, sb.ToString());

            if (charm.CurrentCharmUseId == CharmUses.Character)
            {
                _uiService.SetText(UseType, "Character");
                _uiService.SetText(UseIdName, charm.TargetName + ": [#" + charm.TargetId + "]");
            }
            else
            {
                _uiService.SetText(UseType, "None");
                _uiService.SetText(UseIdName, null);
            }
        }
    }
}
