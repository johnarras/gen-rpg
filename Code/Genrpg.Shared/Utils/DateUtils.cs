using MessagePack;
using System;

namespace Genrpg.Shared.Utils
{
    /// <summary>
    /// This contains utilities used with the DateTime object
    /// </summary>
    [MessagePackObject]
    public class DateUtils
    {
        /// <summary>
        /// This creates a specific Year_Month_Day representation from a DateTime
        /// </summary>
        /// <param name="time">DateTime to convert</param>
        /// <returns>new representation of the DateTime in Y_M_D form</returns>
        public static string GetYMD(DateTime time)
        {
            string yearStr = time.Year.ToString();
            string monthStr = (time.Month < 10 ? "0" : "") + time.Month;
            string dayStr = (time.Day < 10 ? "0" : "") + time.Day;
            return yearStr + "_" + monthStr + "_" + dayStr;
        }

        /// <summary>
        /// Print the value of time given in milliseconds.
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        public static string PrintTime(long seconds)
        {
            long minutes = seconds / 60;
            seconds %= 60;
            long hours = minutes / 60;
            minutes %= 60;
            long days = hours / 24;
            hours %= 24;

            // Now print something nice.


            string txt = "";
            if (days > 0)
            {
                txt += days + "day" + (days != 1 ? "s" : "") + " ";
            }

            if (hours < 10)
            {
                txt += "0";
            }
            txt += hours + ":";

            if (minutes < 10)
            {
                txt += "0";
            }
            txt += minutes + ":";

            if (seconds < 10)
            {
                txt += "0";
            }
            txt += seconds + "";

            return txt;

        }



    }
}
