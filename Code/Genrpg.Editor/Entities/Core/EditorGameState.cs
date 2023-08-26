using Genrpg.ServerShared.Core;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.GameDatas;
using Genrpg.Shared.GameDatas.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Users.Entities;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Genrpg.Editor.Entities.Core
{
    public class EditorGameState : ServerGameState
    {

        public static CancellationTokenSource CTS = new CancellationTokenSource();

        public EditorUser EditorUser { get; set; }
        public EditorGameData EditorGameData { get; set; }
        public EditorGameState()
        {
            EditorUser = new EditorUser();
        }

        public List<object> LookedAtObjects = new List<object>();
    }

    public class EditorGameData
    {
        public GameData GameData { get; set; }

        public List<BaseGameData> Data { get; set; } = new List<BaseGameData>();

    }

    public class EditorUser
    {
        public User User { get; set; }
        public List<EditorCharacter> Characters { get; set; }

        public EditorUser()
        {
            Characters = new List<EditorCharacter>();
        }
    }

    public class EditorUnitData
    {
        public IUnitData Data { get; set; }

        public string Id
        {
            get
            {
                return Data != null ? Data.GetType().Name : "--";
            }
            set
            {

            }
        }
    }

    public class EditorCharacter
    {
        public Character Character { get; set; }
        public List<EditorUnitData> Data { get; set; }

        public EditorCharacter()
        {
            Data = new List<EditorUnitData>();
        }
        public string Id
        {
            get
            {
                return Character != null ? Character.Id : "None";
            }
            set
            {

            }
        }

        public string Name
        {
            get
            {
                return Character != null ? Character.Name : "None";
            }
            set
            {

            }
        }
    }
}
