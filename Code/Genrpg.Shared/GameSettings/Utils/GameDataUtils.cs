using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.GameSettings.Utils
{
    public static class GameDataUtils
    {
        public static List<IIdName> GetIdNameList(GameData data, string typeName)
        {
            List<ITopLevelSettings> settingsList = data.AllSettings();

            foreach (ITopLevelSettings settings in settingsList)
            {
                List<IGameSettings> children = settings.GetChildren();

                if (children.Count > 0 && children[0] is IIdName idname &&
                    children[0].GetType().Name == typeName)
                {
                    return children.Cast<IIdName>().ToList();
                }
            }

            return new List<IIdName>();
        }

    }
}
