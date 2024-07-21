using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Accounts.Constants
{
    public class AccountConstants
    {

        public const string DefaultReferrerId = "default";

        public const long CompanyProductId = 1;

        public const int MinConnectionIndex = 1;
        public const int MaxConnectionIndex = 2;

        public const long MinShareIdLength = 3;
        public const long MaxShareIdLength = 16;

        public const long MinNameLength = 3;
        public const long MaxNameLength = 24;

        public const long MinPasswordLength = 7;

        public const long MaxConnectionFanout = 4;

    }
}
