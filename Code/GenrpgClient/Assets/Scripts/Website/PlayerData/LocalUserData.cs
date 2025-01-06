using Genrpg.Shared.Interfaces;


public class LocalUserData : IStringId
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string LoginToken { get; set; }
}