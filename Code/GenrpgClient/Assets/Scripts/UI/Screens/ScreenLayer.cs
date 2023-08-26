using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genrpg.Shared.Core.Entities;

using Services;
using UI.Screens.Constants;
using UnityEngine;

[Serializable]
public class ScreenLayer
{
    public ScreenLayers LayerId;
    [HideInInspector]
    public int Index;
    public ActiveScreen CurrentScreen;
    public ActiveScreen CurrentLoading;
    public List<ActiveScreen> ScreenQueue;
    public bool SkipInAllScreensList;

    public GameObject LayerParent { get; set; }

}
