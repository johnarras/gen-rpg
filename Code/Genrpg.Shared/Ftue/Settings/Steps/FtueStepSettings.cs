using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Ftue.Settings.Steps
{
    [MessagePackObject]
    public class FtueStep : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        /// <summary>
        /// Description text shown to the player in the main popup
        /// </summary>
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }

        [Key(7)] public long PrereqFtueStepId { get; set; }
        [Key(8)] public long FtueTriggerId { get; set; }

        [Key(9)] public string TriggerName { get; set; }

        // If this exists, open this screen
        [Key(10)] public string ActionScreenName { get; set; }

        // If this exists, this is the only button we can click
        [Key(11)] public string ActionButtonName { get; set; }

        /// <summary>
        /// Do we show the popup tint or hide the popup
        /// </summary>
        [Key(12)] public long FtuePopupTypeId { get; set; }

    }

    [MessagePackObject]
    public class FtueStepSettings : ParentSettings<FtueStep>
    {
        [Key(0)] public override string Id { get; set; }

        public FtueStep GetFtueStep(long idkey) { return _lookup.Get<FtueStep>(idkey); }

        public FtueStep FindFtueStep(long ftueTriggerId, string ftueTriggerName)
        {
            return _data.FirstOrDefault(x => x.FtueTriggerId == ftueTriggerId && x.TriggerName == ftueTriggerName);
        }
    }

    [MessagePackObject]
    public class FtueStepSettingsApi : ParentSettingsApi<FtueStepSettings, FtueStep> { }
    [MessagePackObject]
    public class FtueStepSettingsLoader : ParentSettingsLoader<FtueStepSettings, FtueStep, FtueStepSettingsApi> { }


}
