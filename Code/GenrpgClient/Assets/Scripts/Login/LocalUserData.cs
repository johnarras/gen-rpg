using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class LocalUserData : IStringId
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Password { get; set; }
}