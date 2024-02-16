using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.PlayerServer.Entities
{
    public class LoggedInUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }
}
