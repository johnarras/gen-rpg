using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.StateHelpers;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Sexes.Settings;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Crawler.States
{
    public class ChooseClassHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.ChooseClass; }


        public override async UniTask<CrawlerStateData> Init(UnityGameState gs, CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyMember member = action.ExtraData as PartyMember;

            IReadOnlyList<Class> classes = _gameData.Get<ClassSettings>(null).GetData();

            foreach (Class cl in classes)
            {
                if (cl.IdKey < 1)
                {
                    continue;
                }

                if (member.Classes.Any(x=>x.ClassId == cl.IdKey))
                {
                    continue;
                }

                string desc = cl.Desc;


                stateData.Actions.Add(new CrawlerStateAction(cl.Name + ": " + desc, (KeyCode)char.ToLower(cl.Abbrev[0]), 
                    (member.Classes.Count < ClassConstants.MaxClasses-1 ? ECrawlerStates.ChooseClass : ECrawlerStates.ChoosePortrait), 
                    delegate
                    {
                        member.Classes.Add(new UnitClass() { ClassId = cl.IdKey });
                    }, member ));
            }

            if (member.Classes.Count < ClassConstants.MaxClasses)
            {

                stateData.Actions.Add(new CrawlerStateAction("Escape", KeyCode.Escape, ECrawlerStates.RollStats,
                    delegate
                    {
                        member.Stats = new StatGroup();
                        member.Classes.Clear();
                    },
                    extraData: member));
            }
            else
            {
                stateData.Actions.Add(new CrawlerStateAction("Escape", KeyCode.Escape, ECrawlerStates.ChooseClass,
                    delegate
                    {
                        member.Classes.RemoveAt(member.Classes.Count - 1);
                    },
                    extraData: member));
            }
            await UniTask.CompletedTask;
            return stateData;

        }
    }
}
