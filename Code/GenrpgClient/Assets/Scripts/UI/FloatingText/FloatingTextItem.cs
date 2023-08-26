using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genrpg.Shared.Core.Entities;

using Services;
using UnityEngine;
using UnityEngine.UI;

public class FloatingTextItem : BaseBehaviour
{
    public Text TextString;
    public float DurationSeconds;
    public float PixelsPerSecond;

    public float ElapsedSeconds { get; set; }
}
