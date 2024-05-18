using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Factions.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.Utils;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Genrpg.Shared.Input.PlayerData
{
    [MessagePackObject]
    public class KeyComm : OwnerPlayerData, IId
    {

        public const int ModifierShift = 1 << 0;
        public const int ModifierCtrl = 1 << 1;
        public const int ModifierAlt = 1 << 2;


        public const string ShiftName = "shift";
        public const string CtrlName = "ctrl";
        public const string AltName = "alt";


        public const string ActionPrefix = "Action";

        public const string StrafeLeft = "StrafeLeft";
        public const string StrafeRight = "StrafeRight";
        public const string Forward = "Forward";
        public const string Backward = "Backward";
        public const string TurnLeft = "TurnLeft";
        public const string TurnRight = "TurnRight";

        public const string TargetNext = "TargetNext";

        public const string Jump = "Jump";


        [Key(0)] public override string Id { get; set; }
        [Key(1)] public long IdKey { get; set; }
        [Key(2)] public override string OwnerId { get; set; }
        [Key(3)] public string KeyPress { get; set; }
        [Key(4)] public string KeyCommand { get; set; }
        [Key(5)] public int Modifiers { get; set; }



        public string ShowName()
        {
            if (string.IsNullOrEmpty(KeyPress))
            {
                return "";
            }

            string txt = KeyPress;
            for (int m = 0; m < ModifierList.Length; m++)
            {
                if (FlagUtils.IsSet(Modifiers, ModifierList[m]))
                {
                    txt = ModifierNames[m] + "-" + txt;
                }
            }
            return txt;
        }


        public static readonly int[] ModifierList = { ModifierShift, ModifierCtrl, ModifierAlt };
        public static readonly string[] ModifierNames = { ShiftName, CtrlName, AltName };

    }
    [MessagePackObject]
    public class KeyCommData : OwnerObjectList<KeyComm>
    {
        [Key(0)] public override string Id { get; set; }

        public KeyComm GetKeyComm(string keyComm)
        {
            return _data.FirstOrDefault(x => x.KeyCommand == keyComm);
        }

        public void AddKeyComm(string keyCommand, string keyPress)
        {
            KeyComm keyComm = new KeyComm()
            {
                KeyCommand = keyCommand,
                KeyPress = keyPress,
                OwnerId = Id,
                Id = HashUtils.NewGuid(),
            };
            _data.Add(keyComm);
        }
    }
    [MessagePackObject]
    public class KeyCommDataLoader : OwnerIdDataLoader<KeyCommData, KeyComm> { }
    [MessagePackObject]
    public class KeyCommApi : OwnerApiList<KeyCommData, KeyComm> { }



    [MessagePackObject]
    public class KeyCommDataMapper : OwnerDataMapper<KeyCommData, KeyComm, KeyCommApi> { }
}
