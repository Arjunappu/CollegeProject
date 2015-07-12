using System;
using System.Configuration;

namespace RestaurantBookingSystem.Helpers
{
    /// <summary>
    /// Utility class to retrive App Setting Entries
    /// </summary>
    public static class AppConfigHelper
    {
        /// <summary>
        /// Retrives the Facebook APP_ID as a UInt64 from web.config (Assumes configuration name to be 'FacebookAppId')
        /// </summary>
        public static readonly ulong FacebookAppId = Convert.ToUInt64(ConfigurationManager.AppSettings["FacebookAppId"]);

        /// <summary>
        /// Retrives the Facebook APP_SECRET as a string from web.config (Assumes configuration name to be 'FacebookAppSecret')
        /// </summary>
        public static readonly string FacebookAppSecret = ConfigurationManager.AppSettings["FacebookAppSecret"];

        /// <summary>
        /// Retrives the Database Booking Time Slot in Minutes from Web.config (Assumes configuration name to be 'BookingSlotMinutes')
        /// </summary>
        public static readonly double BookingSlotMinutes = Convert.ToDouble(ConfigurationManager.AppSettings["BookingSlotMinutes"]);

        /// <summary>
        /// Retrives the Database Booking Time Slot in Minutes from Web.config (Assumes configuration name to be 'BookingSlotMinutes')
        /// </summary>
        public static readonly int MediumPageSize = Convert.ToInt32(ConfigurationManager.AppSettings["MediumPageSize"]);

        /// <summary>
        /// Retrives the Database Booking Time Slot in Minutes from Web.config (Assumes configuration name to be 'BookingSlotMinutes')
        /// </summary>
        public static readonly int LargePageSize = Convert.ToInt32(ConfigurationManager.AppSettings["LargePageSize"]);
    }

    /// <summary>
    /// Utility class to retrive database connection details
    /// </summary>
    public static class DatabaseConnection
    {
        /// <summary>
        /// Retrives the Database Connection String from Web.config (Assumes configuration name to be 'RestaurantDB')
        /// </summary>
        public static readonly string ConnectionStringToDb = ConfigurationManager.ConnectionStrings["RestaurantDB"].ConnectionString;
    }
}