using System.ComponentModel;
using Calendar = p3rpc.nativetypes.Interfaces.Calendar;

namespace BGME.MDmp3.Conditionals
{
    internal class DateService
    {
        static Calendar gameCalendar;
        public static int DaysElapsed = ((int)gameCalendar.DaysSinceApril1);
        public enum DayOfWeek
        {
            Monday,
            Tuesday,
            Wednesday,
            Thursday,
            Friday,
            Saturday,
            Sunday
        }
        public static int Day = (DaysElapsed + ((int)DayOfWeek.Wednesday)%7);
        public static int ElapsedFromDate (int month, int day)
        {
            int MonthOffset = 0;
            int April1Offset = 91;
            int[] MonthLength = new int[12]{31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};
            if (month > 1) {
               MonthOffset = MonthLength.Take(month - 1).Sum();
            }
            int daysToDate = day - 1 + MonthOffset;
            if (daysToDate < April1Offset) // 2010 overflow
            {
                return daysToDate + April1Offset;
            }
            else return daysToDate - April1Offset;
        }
    }
}