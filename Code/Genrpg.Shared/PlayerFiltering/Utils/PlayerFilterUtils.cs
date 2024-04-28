using Genrpg.Shared.Currencies.PlayerData;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Genrpg.Shared.PlayerFiltering.Utils
{
    public static class PlayerFilterUtils
    {
        public static bool IsActive (IPlayerFilter filter)
        {

            if (!filter.Enabled)
            {
                return false;
            }

            if (!filter.UseDateRange)
            {
                return true;
            }

            DateTime currTime = DateTime.UtcNow;

            if (currTime < filter.StartDate)
            {
                return false;
            }

            if (currTime >= filter.StartDate && currTime <= filter.EndDate)
            {
                return true;
            }

            if (filter.RepeatMonthly)
            {
                int currDay = currTime.Day;
                TimeSpan currTimeOfDay = currTime.TimeOfDay;

                int startDay = filter.StartDate.Day;
                TimeSpan startTimeOfDay = filter.StartDate.TimeOfDay;

                bool beforeStart = currDay < startDay ||
                    (currDay == startDay && currTimeOfDay < startTimeOfDay);

                int endDay = filter.EndDate.Day;
                TimeSpan endTimeOfDay = filter.EndDate.TimeOfDay;

                bool afterEnd = currDay > endDay ||
                    (currDay == endDay && currTimeOfDay > endTimeOfDay);


                if (startDay <= endDay)
                {
                    // If before start or after end, it's not valid.
                    if (beforeStart || afterEnd)
                    {
                        return false;
                    }
                }
                else
                {
                    // startDay > endDay so it overlaps the end of month.
                    // Only bad if it's before the start AND after the end.

                    return beforeStart && afterEnd;
                }

            }
            else if (filter.RepeatHours > 0)
            {
                TimeSpan startTimeDiff = (currTime - filter.StartDate);

                int startHourRepeats = (int)(startTimeDiff.TotalHours / filter.RepeatHours);

                startTimeDiff.Add(TimeSpan.FromHours(-startHourRepeats * filter.RepeatHours));

                return startTimeDiff.TotalHours <= (filter.EndDate - filter.StartDate).TotalHours;
            }


            return false;
        }

        public static DateTime GetNextStartDate(IPlayerFilter filter)
        {
            return GetNextDate(filter, filter.StartDate);
        }

        public static DateTime GetNextEndDate(IPlayerFilter filter)
        {
            return GetNextDate(filter, filter.EndDate);
        }

        private static DateTime GetNextDate(IPlayerFilter filter, DateTime dateToCheck)
        {

         

            DateTime currTime = DateTime.UtcNow;

            if (dateToCheck > currTime)
            {
                return dateToCheck;
            }

            if (filter.RepeatMonthly)
            {
                int addMonths = 0;
                if (currTime.Day < dateToCheck.Day ||
                    (currTime.Day == dateToCheck.Day && currTime.TimeOfDay < dateToCheck.TimeOfDay))
                {
                    addMonths = 0;
                }
                else
                {
                    addMonths = 1;
                }

                return new DateTime(currTime.Year, currTime.Month + addMonths, currTime.Day, dateToCheck.TimeOfDay.Hours,
                    dateToCheck.TimeOfDay.Minutes, dateToCheck.TimeOfDay.Seconds);

            }
            else if (filter.RepeatHours > 0)
            {
                int repeatTimes = (int)(currTime - dateToCheck).TotalHours / filter.RepeatHours;
                repeatTimes++;
                return dateToCheck.AddHours(repeatTimes * filter.RepeatHours);
            }


            return filter.EndDate;
        }
    }
}
