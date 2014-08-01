using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AMSAPP
{
    public static class Extensions
    {

        public static string ToDisplayString(this TimeSpan value)
        {
            return (value < TimeSpan.Zero ? "-" : "") + value.ToString(@"hh\:mm\:ss");
        }

        public static string ToDisplayTimeString(this DateTime value)
        {
            return value.ToString("h:mm tt");
        }

        public static string ToDisplayString(this TimeSpan? value)
        {
            if (value == null)
            {
                return "";
            }
            return ((TimeSpan)value).ToDisplayString();
        }

        public static string ToDisplayTimeString(this DateTime? value)
        {
            if (value == null)
            {
                return "";
            }
            return ((DateTime)value).ToDisplayTimeString();
        }

    }
}
