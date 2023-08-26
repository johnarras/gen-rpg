using MessagePack;
namespace Genrpg.Shared.Utils
{
    /// <summary>
    /// Use this to return an error message and a "valid or not" bool, along with the data originally sent in.
    /// </summary>
    [MessagePackObject]
    public class ValidityResult
    {
        [Key(0)] public bool IsValid { get; set; }
        [Key(1)] public string Message { get; set; }
        [Key(2)] public object Data { get; set; }
    }
}
