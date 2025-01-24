using Genrpg.Shared.Purchasing.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.Purchasing.Entities
{
    public class PurchaseValidationResult
    {
        public EPurchaseValidationStates State { get; set; }

        public string ErrorMessage { get; set; }

    }
}
