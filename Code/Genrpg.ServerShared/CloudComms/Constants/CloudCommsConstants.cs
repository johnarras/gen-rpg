using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.Constants
{
    public class CloudCommsConstants
    {
        public static readonly TimeSpan EndpointDeleteTime = TimeSpan.FromDays(7);
        public static readonly TimeSpan MessageDeleteTime = TimeSpan.FromSeconds(10);
    }
}
