using MessagePack;
using Genrpg.Shared.DataStores.Core;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System.Linq;
using Genrpg.Shared.Spells.Entities;
using System.Collections.Generic;

namespace Genrpg.Shared.Input.Entities
{
    [MessagePackObject]
    public class KeyComm : IStatusItem
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


        [Key(0)] public string? KeyPress { get; set; }
        [Key(1)] public string? KeyCommand { get; set; }
        [Key(2)] public int Modifiers { get; set; }



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
    public class KeyCommData : ObjectList<KeyComm>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override List<KeyComm> Data { get; set; } = new List<KeyComm>();
        public override void AddTo(Unit unit) { unit.Set(this); }
        public override void Delete(IRepositorySystem repoSystem) { repoSystem.Delete(this); }
        public KeyComm GetKeyComm(string keyComm)
        {
            return Data.FirstOrDefault(x => x.KeyCommand == keyComm);
        }

        public void AddKeyComm(string keyCommand, string keyPress)
        {
            KeyComm keyComm = new KeyComm() { KeyCommand = keyCommand, KeyPress = keyPress };
            Data.Add(keyComm);
        }
    }

}
