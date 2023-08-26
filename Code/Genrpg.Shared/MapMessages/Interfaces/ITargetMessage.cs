using Genrpg.Shared.MapMessages.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;


public interface ITargetMessage : IMapApiMessage
{
    string TargetId { get; set; }
}
