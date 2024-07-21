using MessagePack;
using System;
using System.Text;
using System.Collections.Generic;

namespace Genrpg.Shared.Utils
{
    [MessagePackObject]
    public class HashUtils
    {
        public static string NewGuid()
        {
            return Guid.NewGuid().ToString(); // only spot we should use this method
        }

        private static List<char> _idChars = null;
        public static List<char> GetIdChars()
        {
            if (_idChars != null)
            {
                return _idChars;
            }
            List<char> retval = new List<char>();
            for (int i = 0; i < 128; i++)
            {
                char c = (char)i;
                if (char.IsLetterOrDigit(c) && c != 'x')
                {
                    retval.Add(c);
                }
            }
            _idChars = retval;
            return _idChars;
        }
    }
}
