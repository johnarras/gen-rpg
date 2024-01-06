using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;

public class LocalUserData : IStringId
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Password { get; set; }
}