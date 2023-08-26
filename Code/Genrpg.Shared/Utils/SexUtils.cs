using MessagePack;
namespace Genrpg.Shared.Utils
{
    /// <summary>
    ///  This contains some simple functions for converting a sex into
    ///  him/her/he/she and so forth.
    /// </summary>
    [MessagePackObject]
    public class SexUtils
    {
        public static string HimHer(string sex)
        {
            if (sex == null)
            {
                sex = "";
            }

            sex = sex.ToLower();
            if (sex == "male")
            {
                return "him";
            }
            else if (sex == "female")
            {
                return "her";
            }
            else
            {
                return "them";
            }
        }

        public static string HeShe(string sex)
        {
            if (sex == null)
            {
                sex = "";
            }

            sex = sex.ToLower();
            if (sex == "male")
            {
                return "he";
            }
            else if (sex == "female")
            {
                return "she";
            }
            else
            {
                return "they";
            }
        }


        public static string HisHer(string sex)
        {
            if (sex == null)
            {
                sex = "";
            }

            sex = sex.ToLower();
            if (sex == "male")
            {
                return "his";
            }
            else if (sex == "female")
            {
                return "her";
            }
            else
            {
                return "their";
            }
        }

    }
}
