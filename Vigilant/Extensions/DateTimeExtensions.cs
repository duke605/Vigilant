using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vigilant.Extensions {
    static class DateTimeExtensions
    {

        public static string TimeBetween(this DateTime s, DateTime? dest)
        {
            if (dest == null)
                return "N/A";
            
            TimeSpan between = dest.Value - s;
            string readable;

            if (between.Days > 365)
                readable = $"{between.Days / 365} years";
            else if (between.Days > 0)
                readable = $"{between.Days} days";
            else if (between.Hours > 0)
                readable = $"{between.Hours} days";
            else if (between.Minutes > 0)
                readable = $"{between.Minutes} minutes";
            else
                readable = $"{between.Seconds} seconds";

            return readable;
        }
    }
}
