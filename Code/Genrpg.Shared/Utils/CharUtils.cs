using MessagePack;
using System;

namespace Genrpg.Shared.Utils
{
    [MessagePackObject]
    public class CharUtils
    {/// <summary>
     /// Convert hex character to an int from 0-15
     /// </summary>
     /// <param name="c0">The character to convert</param>
     /// <returns>The converted hex value from 0-15</returns>
        public static int GetHexValue(char c0)
        {
            int val = 0;
            if (c0 >= '0' && c0 <= '9')
            {
                val = c0 - '0';
            }
            else if (c0 >= 'a' && c0 <= 'f')
            {
                val = 10 + c0 - 'a';
            }
            else if (c0 >= 'A' && c0 <= 'F')
            {
                val = 10 + c0 - 'A';
            }
            else
            {
                val = c0 % 16;
            }

            return val;

        }

    }
}
