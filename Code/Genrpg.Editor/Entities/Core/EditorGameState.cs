using Genrpg.Editor.Interfaces;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Utils;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Threading;

namespace Genrpg.Editor.Entities.Core
{
    public class EditorGameState : ServerGameState
    {
        public static CancellationTokenSource CTS = new CancellationTokenSource();

        public EditorUser EditorUser { get; set; }
        public EditorGameData EditorGameData { get; set; }
        public IGameData data { get; set; }
        public MyRandom rand { get; set; } = new MyRandom();
        public EditorGameState(IServerConfig config): base(config)
        {
            EditorUser = new EditorUser();
        }

        public List<object> LookedAtObjects = new List<object>();
    }

    public class EditorGameData : IShowChildListAsButton
    {
        public IGameData GameData { get; set; }

        public List<EditorSettingsList> Data { get; set; } = new List<EditorSettingsList>();

    }

    public interface IEditorScaffold
    {
        System.Collections.IEnumerable GetData();
    }

    public abstract class EditorSettingsList : IEditorScaffold
    {
        public string TypeName { get; set; }
        public virtual void SetData(List<BaseGameSettings> baseList) { }
        public abstract System.Collections.IEnumerable GetData();
    }

    public class TypedEditorSettingsList<T> : EditorSettingsList where T : BaseGameSettings, new()
    {
        // This list needs a concrete type as a parameter or it won't bind to the datagrid correctly...
        // either doesn't appear or doesn't have the id visible.
        public List<T> Data { get; set; } = new List<T>();

        public override void SetData(List<BaseGameSettings> baseList)
        {
            List<T> list = new List<T>();

            foreach (BaseGameSettings settings in baseList)
            {
                if (settings is T t)
                {
                    list.Add(t);
                }
            }
            Data = list;
        }

        public override System.Collections.IEnumerable GetData()
        {
            return Data;
        }
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
        public CoreCharacter CoreCharacter { get; set; }
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
