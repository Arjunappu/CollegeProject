using System;
using System.Web;

namespace RestaurantBookingSystem.Helpers
{
    /// <summary>
    /// Utility Class provides methods to Manipulate DateTime objects. This calss can not be inherited
    /// </summary>
    public static class DateTimeHelper
    {
        /// <summary>
        /// Returns the Largest DateTime less then or equal to the specified DateTime object at a specified Precision Level
        /// </summary>
        /// <param name="datetime">A DateTime object</param>
        /// <param name="precisionLevel">One of DateTimePrecisionLevel values</param>
        /// <returns>A new DateTime object Floored or reduced to Specified Precision Levels</returns>
        public static DateTime Floor(this DateTime datetime, DateTimePrecisionLevel precisionLevel)
        {
            const long  milliSeconds = 10000,
                        seconds = milliSeconds*1000,
                        minutes = seconds*60,
                        hours = minutes*60,
                        days = hours * 24;
            long precisionvalue;
            switch (precisionLevel)
            {
                case DateTimePrecisionLevel.MilliSeconds:
                    {
                        precisionvalue = milliSeconds;
                        break;
                    }
                case DateTimePrecisionLevel.Seconds:
                    {
                        precisionvalue = seconds;
                        break;
                    }
                case DateTimePrecisionLevel.Minutes:
                    {
                        precisionvalue = minutes;
                        break;
                    }
                case DateTimePrecisionLevel.Hours:
                    {
                        precisionvalue = hours;
                        break;
                    }
                case DateTimePrecisionLevel.Days:
                    {
                        precisionvalue = days;
                        break;
                    }
                default:
                    {
                        throw new ArgumentException(
                            String.Format(
                                "Precission Level can only be one of DateTimePrecisionLevel Values. {0} is not a valid value for this argument",
                                (int) precisionLevel), "precisionLevel");
                    }
            }
            return new DateTime(datetime.Ticks - (datetime.Ticks % precisionvalue), datetime.Kind);
        }

        /// <summary>
        /// Returns the Largest DateTime less then or equal to the specified DateTime object to a specified Precision Value at a specified Precision Level
        /// </summary>
        /// <param name="datetime">A DateTime object </param>
        /// <param name="value">A long precision value which makes a multiple to the precision level</param>
        /// <param name="precisionLevel">One of DateTimePrecisionLevel values</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>A new DateTime object Floored or reduced to Specified Precision Levels</returns>
        public static DateTime Floor(this DateTime datetime, long value, DateTimePrecisionLevel precisionLevel)
        {
            if (value < 1)
                throw new ArgumentException("value can not be less then 1", "value");

            const long milliSeconds = 10000,
                       seconds = milliSeconds * 1000,
                       minutes = seconds * 60,
                       hours = minutes * 60,
                       days = hours * 24;
            long precisionvalue;
            switch (precisionLevel)
            {
                case DateTimePrecisionLevel.MilliSeconds:
                    {
                        precisionvalue = milliSeconds;
                        break;
                    }
                case DateTimePrecisionLevel.Seconds:
                    {
                        precisionvalue = seconds;
                        break;
                    }
                case DateTimePrecisionLevel.Minutes:
                    {
                        precisionvalue = minutes;
                        break;
                    }
                case DateTimePrecisionLevel.Hours:
                    {
                        precisionvalue = hours;
                        break;
                    }
                case DateTimePrecisionLevel.Days:
                    {
                        precisionvalue = days;
                        break;
                    }
                default:
                    {
                        throw new ArgumentException(
                            String.Format(
                                "Precission Level can only be one of DateTimePrecisionLevels. {0} is not a valid value for this argument",
                                (int)precisionLevel), "precisionLevel");
                    }
            }
            return new DateTime(datetime.Ticks - (datetime.Ticks % (precisionvalue * value)), datetime.Kind);
        }

        /// <summary>
        /// Converts the valu of current DateTime object to its equivalant univarsal detailed long date time string
        /// </summary>
        /// <param name="datetime">A DateTime object </param>
        /// <returns>A Univarsal DateTime string formated in "dddd, dd MMMM yyyy, hh:mm:ss '+0000'" format</returns>
        public static string ToUtcLongDateTimeString(this DateTime datetime)
        {
            return datetime.ToUniversalTime().ToString("dddd, dd MMMM yyyy, HH:mm:ss '+0000'");
        }

        /// <summary>
        /// Converts the valu of current DateTime object to its equivalant ISO DateTime string
        /// </summary>
        /// <param name="datetime">A DateTime object </param>
        /// <returns>A Univarsal ISO DateTime string formated in ""yyyy-MM-ddTHH:mm:ss.fffZ"" format</returns>
        public static string ToISODateTimeString(this DateTime datetime)
        {
            return datetime.TryMakingUnivarsalFromClient().ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }

        /// <summary>
        /// A Utility method to set a user's local timezone offset in user's cookie
        /// </summary>
        /// <param name="usertimezone">A string representing the offset minutes from GMT</param>
        public static void SetUserTimeZoneCookie(string usertimezone)
        {
            int timezoneoffset;
            if (usertimezone != null && Int32.TryParse(usertimezone, out timezoneoffset))
            {
                var cookie = new HttpCookie("_utz__", (timezoneoffset).ToString());
                cookie.Expires = DateTime.UtcNow.AddMonths(6);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }

        /// <summary>
        /// Unitility method that tries to adjust a DateTime instance to an offset specified in the Request cookie.
        /// </summary>
        /// <param name="dateTime">A DateTime instance to convert</param>
        /// <returns>If the cookie is found the DateTime adjusted to that offset otherwise the given DateTime</returns>
        public static DateTime TryMakingLocalToClient(this DateTime dateTime)
        {
            int timezoneoffset;
            var httpCookie = HttpContext.Current.Request.Cookies["_utz__"];
            if (httpCookie != null && (httpCookie.Value != null && Int32.TryParse(httpCookie.Value, out timezoneoffset)))
            {
                return DateTime.SpecifyKind(dateTime.ToUniversalTime().AddMinutes(-timezoneoffset), DateTimeKind.Unspecified);
            }
            return dateTime;
        }

        /// <summary>
        /// Unitility method that tries to adjust a DateTime instance to an offset specified in the Request cookie.
        /// </summary>
        /// <param name="dateTime">A DateTime instance to convert</param>
        /// <returns>If the cookie is found the DateTime adjusted to that offset otherwise the given DateTime</returns>
        public static DateTime TryMakingUnivarsalFromClient(this DateTime dateTime)
        {
            int timezoneoffset;
            var httpCookie = HttpContext.Current.Request.Cookies["_utz__"];
            if (httpCookie != null && (httpCookie.Value != null && Int32.TryParse(httpCookie.Value, out timezoneoffset)) && dateTime.Kind == DateTimeKind.Unspecified)
            {
                return DateTime.SpecifyKind(dateTime.AddMinutes(timezoneoffset), DateTimeKind.Utc);
            }
            return dateTime;
        }

        /// <summary>
        /// An Enumeration of different DateTime Precision Levels
        /// </summary>
        public enum DateTimePrecisionLevel : ulong
        {
            MilliSeconds = 10000,
            Seconds = MilliSeconds * 1000,
            Minutes = Seconds * 60,
            Hours = Minutes * 60,
            Days = Hours * 24
        }

        /// <summary>
        /// A Minimum Valid Date in Microsoft Sql Server
        /// </summary>
        /// <remarks>A MS SQL's DateTime is between 1/1/1753 12:00:00 AM and 12/31/9999 11:59:59 PM.</remarks>
        public static readonly DateTime SqlDbMinDateTime = new DateTime(1753,1,1,0,0,0,0);
    }
}