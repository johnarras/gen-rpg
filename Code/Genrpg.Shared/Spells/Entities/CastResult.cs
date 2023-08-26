using MessagePack;
namespace Genrpg.Shared.Spells.Entities
{
    [MessagePackObject]
    public class CastResult
    {
        public string Message = "";

        public void AddMessage(string txt)
        {
            Message += txt + "\n";
        }
    }
}
