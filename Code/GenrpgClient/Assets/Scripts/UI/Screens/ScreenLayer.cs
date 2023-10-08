using System;
using System.Collections.Generic;
using UI.Screens.Constants;
using UnityEngine;
using GEntity = UnityEngine.GameObject;

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

    public GEntity LayerParent { get; set; }

}
