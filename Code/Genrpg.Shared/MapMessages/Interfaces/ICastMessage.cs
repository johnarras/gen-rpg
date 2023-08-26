using System;
using System.Collections.Generic;
using System.Text;

public interface ICastMessage : ICasterMessage, ITargetMessage
{
    float Duration { get; set; }
}