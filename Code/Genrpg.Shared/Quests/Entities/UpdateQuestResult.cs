using MessagePack;
namespace Genrpg.Shared.Quests.Entities
{
    [MessagePackObject]
    public class UpdateQuestResult
    {
        [Key(0)] public bool Success { get; set; }

        [Key(1)] public string Message { get; set; }

        public UpdateQuestResult()
        {
            Message = "";
        }
    }
}
