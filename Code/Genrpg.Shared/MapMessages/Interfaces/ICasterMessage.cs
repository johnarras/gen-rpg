using Genrpg.Shared.MapMessages.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

public interface ICasterMessage : IMapApiMessage
{
    string CasterId { get; set; }
}