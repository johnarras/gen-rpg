using Genrpg.Shared.Crawler.Combat.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.GameEvents
{
    public class SetCombatGroupAction
    {
        public Action Action;
        public CombatGroup Group;
    }
}
