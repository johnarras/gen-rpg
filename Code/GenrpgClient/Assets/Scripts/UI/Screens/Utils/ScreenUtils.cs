using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using UI.Screens.Constants;
using UnityEngine;

namespace UI.Screens.Utils
{
    public class ScreenUtils
    {
        public static string GetFullScreenNameFromEnum(ScreenId id)
        {
            return (id.ToString().Replace("_","/") + "Screen");
        }

        public static void SetupScreenSystem(int width, int height, bool isFullScreen, bool isLandscape, int vsyncCount)
        {
            Screen.SetResolution(width, height, isFullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
            Screen.orientation = isLandscape ? ScreenOrientation.LandscapeLeft : ScreenOrientation.Portrait;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            QualitySettings.vSyncCount = vsyncCount;
        }

        public static int Width
        {
            get { return Screen.width; }
        }

        public static int Height
        {
            get { return Screen.height; }
        }
    }
}
