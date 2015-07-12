using System;
using System.Data.SqlClient;

namespace RestaurantBookingSystem.Helpers
{
    /// <summary>
    /// Utility class provides methods to get data from SqlDataReader object as required type. This class can not be inherited
    /// </summary>
    public static class DataReaderHelper
    {
        /// <summary>
        /// Gets the data as an UInt64 from the specified column index
        /// </summary>
        /// <param name="src">The SqlDataReader instance from which to retrive data</param>
        /// <param name="columnnumber">The Zero based column index in SqlDataReader current row</param>
        /// <returns>If Successful Returns the data as UInt64 initialized to parsed value otherwise returns 0</returns>
        public static ulong TryGetDataAsUInt64(this SqlDataReader src, int columnnumber)
        {
            var res = default(UInt64);
            if (!src.IsDBNull(columnnumber))
            {
                ulong.TryParse(src.GetValue(columnnumber).ToString(), out res);
            }
            return res;
        }

        /// <summary>
        /// Gets the data as an Int32 from the specified column index
        /// </summary>
        /// <param name="src">The SqlDataReader instance from which to retrive data</param>
        /// <param name="columnnumber">The Zero based column index in SqlDataReader current row</param>
        /// <returns>If Successful Returns the data as Int32 initialized to parsed value otherwise returns 0</returns>
        public static int TryGetDataAsInt(this SqlDataReader src, int columnnumber)
        {
            var res = 0;
            if (!src.IsDBNull(columnnumber))
            {
                Int32.TryParse(src.GetValue(columnnumber).ToString(), out res);
            }
            return res;
        }

        /// <summary>
        /// Gets the data as an Decimal from the specified column index
        /// </summary>
        /// <param name="src">The SqlDataReader instance from which to retrive data</param>
        /// <param name="columnnumber">The Zero based column index in SqlDataReader current row</param>
        /// <returns>If Successful Returns the data as Decimal initialized to parsed value otherwise returns 0</returns>
        public static decimal TryGetDataAsDecimal(this SqlDataReader src, int columnnumber)
        {
            Decimal res = 0;
            if (!src.IsDBNull(columnnumber))
            {
                Decimal.TryParse(src.GetValue(columnnumber).ToString(), out res);
            }
            return res;
        }

        /// <summary>
        /// Gets the data as an String from the specified column index
        /// </summary>
        /// <param name="src">The SqlDataReader instance from which to retrive data</param>
        /// <param name="columnnumber">The Zero based column index in SqlDataReader current row</param>
        /// <returns>If Successful Returns the data as String initialized to parsed value otherwise returns String.Empty</returns>
        public static string TryGetDataAsString(this SqlDataReader src, int columnnumber)
        {
            var res = String.Empty;
            if (!src.IsDBNull(columnnumber))
            {
                res = src.GetValue(columnnumber).ToString();
            }
            return res;
        }

        /// <summary>
        /// Gets the data as an Int32 from the specified column index
        /// </summary>
        /// <param name="src">The SqlDataReader instance from which to retrive data</param>
        /// <param name="columnnumber">The Zero based column index in SqlDataReader current row</param>
        /// <returns>If Successful Returns the data as DateTime initialized to parsed value otherwise returns DateTime.MinValue</returns>
        public static DateTime TryGetDataAsDateTime(this SqlDataReader src, int columnnumber)
        {
            var res = DateTime.MinValue;
            if (!src.IsDBNull(columnnumber))
            {
                DateTime.TryParse(src.GetValue(columnnumber).ToString(), out res);
            }
            return DateTime.SpecifyKind(res, DateTimeKind.Utc);
        }

        /// <summary>
        /// Gets the data as an TimeSpan from the specified column index. (Assumes data to be number of seconds)
        /// </summary>
        /// <param name="src">The SqlDataReader instance from which to retrive data</param>
        /// <param name="columnnumber">The Zero based column index in SqlDataReader current row</param>
        /// <returns>If Successful Returns the data as TimeSpan initialized to parsed value otherwise returns 'new TimeSpan(0)'</returns>
        public static TimeSpan TryGetDataAsTimeSpan(this SqlDataReader src, int columnnumber)
        {
            var res = new TimeSpan(0);
            if (!src.IsDBNull(columnnumber))
            {
                res = new TimeSpan(0,0,src.TryGetDataAsInt(columnnumber));
            }
            return res;
        }

        /// <summary>
        /// Gets the data as a Guid from the specified column index.
        /// </summary>
        /// <param name="src">The SqldataReader instance from which to retrive data</param>
        /// <param name="columnnumber">The Zero based column index in SqlDataReader current row</param>
        /// <returns>If successful Returns the data as Guid initiallized to parsed value otherwise returns <see cref="Guid.Empty"/></returns>
        public static Guid TryGetDataAsGuid(this SqlDataReader src, int columnnumber)
        {
            var res = Guid.Empty;
            if (!src.IsDBNull(columnnumber))
            {
                Guid.TryParse(src.GetValue(columnnumber).ToString(), out res);
            }
            return res;
        }

    }
}