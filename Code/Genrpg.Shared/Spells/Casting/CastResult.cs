using MessagePack;
namespace Genrpg.Shared.Spells.Casting
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
