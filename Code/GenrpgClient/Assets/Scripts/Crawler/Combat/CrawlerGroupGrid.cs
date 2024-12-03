using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.UnitEffects.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Combat
{
    public class CrawlerGroupGrid : BaseBehaviour
    {

        public GameObject Anchor;

        public List<CrawlerCombatIcon> Icons = new List<CrawlerCombatIcon>();


        public CrawlerCombatIcon IconTemplate;


        public void Clear()
        {
            _clientEntityService.DestroyAllChildren(Anchor);
            Icons.Clear();
        }

        public void UpdateGroups(List<CombatGroup> groups)
        {

            foreach (CombatGroup group in groups)
            {

                CrawlerCombatIcon icon = Icons.FirstOrDefault(x => x.Group.Id == group.Id);

                int okUnitCount = group.Units.Where(x => !x.StatusEffects.HasBit(StatusEffects.Dead)).Count();            
                if (okUnitCount < 1)
                {
                    if (icon != null)
                    {
                        _clientEntityService.Destroy(icon.gameObject);
                        Icons.Remove(icon);
                    }
                }
                else
                {
                    if (icon == null)
                    {
                        icon = _clientEntityService.FullInstantiate(IconTemplate);
                        icon.Group = group;
                        _clientEntityService.AddToParent(icon.gameObject, Anchor);
                        Icons.Add(icon);
                    }

                    icon.UpdateData();
                }
            }

            List<CrawlerCombatIcon> iconsToRemove = new List<CrawlerCombatIcon>();
            foreach (CrawlerCombatIcon icon in Icons)
            {
                CombatGroup currGroup = groups.FirstOrDefault(x=>x.Id == icon.Group.Id);

                if (currGroup ==null)
                {
                    iconsToRemove.Add(icon);
                }
            }

            foreach (CrawlerCombatIcon icon in iconsToRemove)
            {
                _clientEntityService.Destroy(icon);
                Icons.Remove(icon);
            }
        }
    }
}
