using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Input.Constants;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.Utils;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Input.PlayerData
{
    [MessagePackObject]
    public class ActionInput : OwnerPlayerData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public int Index { get; set; }
        [Key(3)] public long SpellId { get; set; }
    }


    [MessagePackObject]
    public class ActionInputData : OwnerObjectList<ActionInput>
    {
        [Key(0)] public override string Id { get; set; }

        public ActionInput GetInput(int actionIndex)
        {
            if (!InputConstants.OkActionIndex(actionIndex))
            {
                return null;
            }

            ActionInput input = _data.FirstOrDefault(x => x.Index == actionIndex);
            if (input == null)
            {
                input = new ActionInput
                {
                    Index = actionIndex,
                    OwnerId = Id,
                    Id = HashUtils.NewGuid(),
                };
                _data.Add(input);
                _repoService.QueueSave(input);
            }
            return input;
        }

        public void SetInput(int actionIndex, long spellTypeId)
        {
            ActionInput input = GetInput(actionIndex);
            if (input == null)
            {
                return;
            }

            if (input.SpellId != spellTypeId)
            {
                input.SpellId = spellTypeId;
                _repoService.QueueSave(input);
            }
        }
    }
    [MessagePackObject]
    public class ActionInputApi : OwnerApiList<ActionInputData, ActionInput> { }

    [MessagePackObject]
    public class ActionInputDataLoader : OwnerDataLoader<ActionInputData, ActionInput> { }

    [MessagePackObject]
    public class ActionInputDataMapper : OwnerDataMapper<ActionInputData, ActionInput, ActionInputApi> { }

}
