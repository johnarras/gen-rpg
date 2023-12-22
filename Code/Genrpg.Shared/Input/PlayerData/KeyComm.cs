using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.DataStores.Categories.PlayerData;

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
}
