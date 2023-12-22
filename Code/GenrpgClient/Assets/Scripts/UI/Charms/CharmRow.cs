using Genrpg.Shared.Charms.Constants;
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
            UIHelper.SetText(HashText, charm.Hash);
            List<string> bonuses = _charmService.PrintBonuses(_gs, charm.Bonuses.FirstOrDefault());

            StringBuilder sb = new StringBuilder();

            foreach (string bonus in bonuses)
            {
                sb.AppendLine(bonus);
            }

            UIHelper.SetText(Bonuses, sb.ToString());

            if (charm.CurrentCharmUseId == CharmUses.Character)
            {
                UIHelper.SetText(UseType, "Character");
                UIHelper.SetText(UseIdName, charm.TargetName + ": [#" + charm.TargetId + "]");
            }
            else
            {
                UIHelper.SetText(UseType, "None");
                UIHelper.SetText(UseIdName, null);
            }
        }
    }
}
