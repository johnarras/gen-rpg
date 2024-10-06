using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.ClientEvents.UserCoins
{
    public class AddUserCoinVisual
    {
        public long UserCoinTypeId { get; set; }
        public long QuantityAdded { get; set; }
        public bool InstantUpdate { get; set; }
    }
}
