using Assets.Scripts.Atlas.Constants;
using Assets.Scripts.ClientEvents.UserCoins;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.UserCoins.Messages;
using Genrpg.Shared.UserCoins.Settings;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.MobileGame
{
    public class UserCoinDisplay : BaseBehaviour
    {

        public long UserCoinTypeId;
        public long UpdateTicks = 10;

        public GText QuantityText;

        public GImage Icon;

        private long _startQuantity;
        private long _currQuantity;
        private long _targetQuantity;
        private long _ticksSinceUpdate = 0;
        

        public override void Init()
        {
            _updateService.AddUpdate(this, UpdateQuantity, UpdateTypes.Late, GetToken());
            AddListener<AddUserCoinVisual>(OnAddUserCoinVisual);

            CoreUserData userData = _gs.ch.Get<CoreUserData>();

            _startQuantity = userData.Coins.Get(UserCoinTypeId);
            _currQuantity = _startQuantity;
            _targetQuantity = _startQuantity;
            _ticksSinceUpdate = UpdateTicks;
            ShowQuantity();
        }

        private void ShowQuantity()
        {
            _uiService.SetText(QuantityText, StrUtils.PrintCommaValue(_currQuantity));
        }

        private void OnAddUserCoinVisual(AddUserCoinVisual visual)
        {

            if (visual.UserCoinTypeId != UserCoinTypeId)
            {
                return;
            }

            _targetQuantity += visual.QuantityAdded;

            if (visual.InstantUpdate)
            {
                _startQuantity = _targetQuantity;
                _currQuantity = _targetQuantity;
            }
            else
            {
                _startQuantity = _currQuantity;
            }
            _ticksSinceUpdate = UpdateTicks;
            ShowQuantity();
        }

        private void UpdateQuantity()
        {
            _ticksSinceUpdate++;
            if (_ticksSinceUpdate > UpdateTicks)
            {
                _ticksSinceUpdate = UpdateTicks;
            }
            
            if (_currQuantity == _targetQuantity)
            {
                return;
            }

            _currQuantity = (_targetQuantity - _startQuantity) * _ticksSinceUpdate / UpdateTicks + _startQuantity;

            ShowQuantity();
        }
    }
}
