using MessagePack;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Genrpg.Shared.Utils
{
    /// <summary>
    /// This class contains some string utility functions.
    /// </summary>
    [MessagePackObject]
    public class StrUtils
    {

        /// <summary>
        /// Replace all instances of a string with another.
        /// </summary>
        /// <param name="txt">The large starting text where the replacements will occur</param>
        /// <param name="start">The string to be replaced</param>
        /// <param name="repl">The replacement text</param>
        /// <returns>The new string with replaced text</returns>
        public static string ReplaceAll(string txt, string start, string repl)
        {
            if (string.IsNullOrEmpty(txt) || string.IsNullOrEmpty(start))
            {
                return txt;
            }

            if (repl == null)
            {
                repl = "";
            }

            string retval = "";

            while (txt.Length > 0)
            {
                int index = txt.IndexOf(start);
                if (index < 0)
                {
                    retval += txt;
                    break;
                }

                // Get part before repl.
                retval += txt.Substring(0, index);
                // Add repl instead of start.
                retval += repl;
                // Move txt past start.
                txt = txt.Substring(index + start.Length);
            }
            return retval;

        }

        /// <summary>
        /// Makes a string plural. This isn't really correct. It just
        /// does a few simple checks.
        /// </summary>
        /// <param name="txt">The string to make plural</param>
        /// <returns>The newly plural string</returns>
        public static string MakePlural(string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return "";
            }

            if (txt[txt.Length - 1] == 'y')
            {
                return txt.Substring(0, txt.Length - 1) + "ies";
            }
            else if (txt[txt.Length - 1] == 'f')
            {
                return txt.Substring(0, txt.Length - 1) + "ves";
            }
            return txt + "s";
        }

        public static string MakePossessive(string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return "";
            }

            if (txt[txt.Length - 1] == 'f')
            {
                return txt.Substring(0, txt.Length - 1) + "ves'";
            }


            if (txt[txt.Length - 1] == 's')
            {
                return txt + "'";
            }
            else
            {
                return txt + "'s";
            }
        }

        /// <summary>
        /// Replace a character in a string at a certain index.
        /// </summary>
        /// <param name="txt">The text to get the replacement</param>
        /// <param name="index">The location to be replaced</param>
        /// <param name="c">The new character that replaces the old one</param>
        /// <returns>A new string with the replaced character</returns>
        public static string ReplaceAtIndex(string txt, int index, char c)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return "";
            }

            if (index < 0 || index >= txt.Length)
            {
                return txt;
            }

            char[] letters = txt.ToCharArray();
            letters[index] = c;
            return new string(letters);
        }

        /// <summary>
        /// Inserts a new character into text
        /// </summary>
        /// <param name="txt">The text to receive the replacement</param>
        /// <param name="index">The index where the replacement will take place</param>
        /// <param name="c">The character to insert</param>
        /// <returns>The string with the newly inserted character</returns>
        public static string InsertAtIndex(string txt, int index, char c)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return "";
            }

            if (index < 0 || index > txt.Length)
            {
                return txt;
            }

            char[] letters = new char[txt.Length + 1];

            letters[index] = c;
            for (int i = 0; i < letters.Length; i++)
            {
                if (i < index)
                {
                    letters[i] = txt[i];
                }
                else if (i > index)
                {
                    letters[i] = txt[i - 1];
                }
            }

            return new string(letters);

        }

        /// <summary>
        /// This inserts a character at certain intervals in text. For example,
        /// adding newlines into text at intervals.
        /// </summary>
        /// <param name="txt">The text to get the inserts</param>
        /// <param name="length">The length between inserts</param>
        /// <param name="start">The text to be replaced</param>
        /// <param name="repl">The text that replaces the old start text</param>
        /// <returns></returns>
        public static string InsertCharAtIntervals(string txt, int length, char start = ' ', char repl = '\n')
        {

            if (string.IsNullOrEmpty(txt))
            {
                return "";
            }

            if (length < 3)
            {
                length = 3;
            }

            int lastPos = 0;
            int times = 0;
            for (int i = 0; i < txt.Length; i++)
            {
                char c = txt[i];

                if (c == repl)
                {
                    lastPos = i;
                    continue;
                }

                bool replacedchar = false;
                if (i - lastPos >= length)
                {
                    times++;
                    for (int j = i; j > lastPos; j--)
                    {
                        char c2 = txt[j];
                        if (c2 == start)
                        {
                            txt = ReplaceAtIndex(txt, j, repl);
                            lastPos = j;
                            replacedchar = true;
                            break;
                        }
                    }
                    if (!replacedchar)
                    {
                        txt = InsertAtIndex(txt, i, repl);
                        lastPos = i;
                    }
                }

                if (times > 1000)
                {
                    break;
                }
            }

            return txt;

        }

        /// <summary>
        /// Splits a string along a substring
        /// </summary>
        /// <param name="txt">The text to be split</param>
        /// <param name="split">The substring to delimit the splitting</param>
        /// <returns>A list of strings split along the given split string.</returns>
        public static List<string> SplitOnString(string txt, string split)
        {
            List<string> words = new List<string>();

            if (string.IsNullOrEmpty(txt))
            {
                return words;
            }


            if (string.IsNullOrEmpty(split))
            {
                words.Add(txt);
                return words;
            }

            while (txt.Length > 0)
            {
                int index = txt.IndexOf(split);
                if (index < 0)
                {
                    if (!string.IsNullOrEmpty(txt))
                    {
                        words.Add(txt);
                    }

                    break;
                }
                else
                {
                    string prefix = txt.Substring(0, index);
                    if (!string.IsNullOrEmpty(prefix))
                    {
                        words.Add(prefix);
                    }

                    txt = txt.Substring(index + split.Length);
                }
            }
            return words;
        }

        /// <summary>
        /// Tells if a character is alphanumeric
        /// </summary>
        /// <param name="c">The character to check</param>
        /// <returns>true if the character is alphanumeric or else false</returns>
        public static bool IsAlNum(char c)
        {
            return c >= 'a' && c <= 'z' ||
                    c >= 'A' && c <= 'Z' ||
                    c >= '0' && c <= '9';
        }


        /// <summary>
        /// This converts a string to alphanumeric characters for certain
        /// indexes such as Azure Table storage Partition keys that have
        /// a limited key space.
        /// 
        /// It replaces bad characters at position N with (N % 10) as a character,
        /// so '0', '1' etc...
        /// 
        /// </summary>
        /// <param name="txt">The text to replace</param>
        /// <returns>The newly safe string</returns>
        public static string MakeSafeString(string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            for (int c = 0; c < txt.Length; c++)
            {
                if (IsAlNum(txt[c]))
                {
                    sb.Append(txt[c]);
                }
                else
                {
                    sb.Append(c % 10).ToString();
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// This returns a string with commas in the thousands, millions, billions
        /// places etc...
        /// </summary>
        /// <param name="val">The number to convert</param>
        /// <returns>A new string with commas in the appropriate places</returns>
        public static string PrintCommaValue(long val)
        {
            bool negative = false;
            if (val < 0)
            {
                negative = true;
                val = -val;
            }


            string txt = "";

            do
            {
                long remainder = val % 1000;
                val /= 1000;

                string remText = remainder.ToString();

                if (val > 0)
                {
                    if (remainder < 10)
                    {
                        remText = "00" + remText;
                    }
                    else if (remainder < 100)
                    {
                        remText = "0" + remText;
                    }
                }
                if (!string.IsNullOrEmpty(txt))
                {
                    txt = "," + txt;
                }
                txt = remText + txt;

            } while (val >= 1000);

            if (val > 0)
            {

                if (!string.IsNullOrEmpty(txt))
                {
                    txt = "," + txt;
                }

                txt = val + txt;
            }
            if (negative)
            {
                txt = "-" + txt;
            }

            return txt;
        }

        /// <summary>
        /// This shows the a double without having to remember format strings
        /// </summary>
        /// <param name="val">The double to get truncated or padded with zeroes</param>
        /// <param name="places">How many decimal places to show</param>
        /// <returns>A string representation of the number with the correct number of zeroes</returns>
        public static string ShowDecimalPlaces(double val, int places)
        {
            if (places <= 0)
            {
                return ((int)val).ToString();
            }

            string formatter = "#.";
            for (int p = 0; p < places; p++)
            {
                formatter += "0";
            }

            return val.ToString(formatter);
        }

        public static string SplitOnCapitalLetters(string txt)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < txt.Length; i++)
            {
                if (i > 0 && char.IsUpper(txt[i]))
                {
                    sb.Append(' ');
                }
                sb.Append(txt[i]);
            }
            return sb.ToString();
        }


        /// <summary>
        /// This is used in procedural word/name generation to try to keep
        /// words from being too unpronounceable. It counts very blocks/
        /// syllables essentially and returns that number so a word can
        /// be made a good length. This is very touchy-feely code.
        /// </summary>
        /// <param name="txt">The current text to check</param>
        /// <returns>The number of vowel blocks.</returns>
        public static int NumVowelBlocks(string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return 0;
            }


            int numBlocks = 0;
            bool inVowel = false;

            for (int t = 0; t < txt.Length; t++)
            {
                char c = txt[t];
                if (c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u' ||
                    c == 'A' || c == 'E' || c == 'I' || c == 'O' || c == 'U' ||
                    t > 0 && (c == 'y' || c == 'Y'))
                {
                    if (!inVowel)
                    {
                        numBlocks++;
                        inVowel = true;
                    }

                }
                else
                {
                    inVowel = false;
                }
            }


            return numBlocks;

        }
        /// <summary>
        /// Returns a, A, an or an depending on if the first letter of the entered
        /// text is a vowel or not
        /// </summary>
        /// <param name="txt">Text to check the first letter of</param>
        /// <returns>a/A/an/An depending on how txt starts</returns>
        public static string A_An(string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return "a";
            }

            char c = txt[0];

            if (c == 'A' || c == 'E' || c == 'I' || c == 'O' || c == 'U')
            {
                return "An";
            }

            if (c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u')
            {
                return "an";
            }

            if (c >= 'A' && c <= 'Z')
            {
                return "A";
            }

            return "a";
        }

        public static string RemoveAAnPrefix(string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return "";
            }

            string[] prefixesToRemove = new string[] { "Some ", "some ", "A ", "a ", "An ", "an " };

            for (int p = 0; p < prefixesToRemove.Length; p++)
            {
                if (txt.IndexOf(prefixesToRemove[p]) == 0)
                {
                    return txt.Substring(prefixesToRemove[p].Length);
                }
            }

            return txt;
        }

        const int IdHashMult = 151;
        public static int GetIdHash(string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return 0;
            }
            int hash = 0;
            int mult = IdHashMult; // Need this prime and this*this*this*128 < 2^31 
            for (int i = 0; i < 3 && i < txt.Length; i++)
            {
                hash = (hash + txt[i]) * mult;
            }
            return hash;
        }

        public static string ConvertToBase64(string txt)
        {
            byte[] arr = System.Text.Encoding.UTF8.GetBytes(txt);
            return Convert.ToBase64String(arr);
        }

        public static string ConvertFromBase64(string base64String)
        {
            byte[] arr = Convert.FromBase64String(base64String);
            return System.Text.Encoding.UTF8.GetString(arr);
        }

        public static string StringBetweenTokens (string searchString, string startToken, string endToken)
        {
            int startIndex = searchString.IndexOf(startToken);
            if (startIndex < 0)
            {
                return "";
            }

            string substring = searchString.Substring(startIndex + startToken.Length);

            int endIndex = substring.IndexOf(endToken);

            if (endIndex < 0)
            {
                endIndex = substring.Length;
            }

            return substring.Substring(0, endIndex);

        }
    }
}
