using MessagePack;
using Genrpg.Shared.DataStores.Core;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using System.Linq;
using System.Threading.Tasks;
using Genrpg.Shared.Spells.Entities;
using System.Collections.Generic;

namespace Genrpg.Shared.Input.Entities
{
    [MessagePackObject]
    public class ActionInput : IStatusItem
    {
        [Key(0)] public int Index { get; set; }
        [Key(1)] public long SpellId { get; set; }
    }


    [MessagePackObject]
    public class ActionInputData : ObjectList<ActionInput>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override List<ActionInput> Data { get; set; } = new List<ActionInput>();
        public override void AddTo(Unit unit) { unit.Set(this); }
        public override void Delete(IRepositorySystem repoSystem) { repoSystem.Delete(this); }
        public ActionInput GetInput(int actionIndex)
        {
            if (!InputConstants.OkActionIndex(actionIndex))
            {
                return null;
            }

            ActionInput input = Data.FirstOrDefault(x => x.Index == actionIndex);
            if (input == null)
            {
                input = new ActionInput { Index = actionIndex };
                Data.Add(input);
                SetDirty(true);
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
            }
            SetDirty(true);
        }
    }

}
