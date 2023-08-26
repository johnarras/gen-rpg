namespace ClientEvents
{
    public class LevelUpEvent
    {
        public string Id { get; set; }
        public long Level { get; set; }
        public long Exp { get; set; }
    }
}
