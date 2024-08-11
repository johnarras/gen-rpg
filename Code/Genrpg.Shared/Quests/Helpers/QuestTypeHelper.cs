using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.Quests.WorldData;
namespace Genrpg.Shared.Quests.Helpers
{
    public class QuestTypeHelper : BaseMapEntityHelper<QuestType>
    {
        public override long GetKey() { return EntityTypes.Quest; }

}
}
