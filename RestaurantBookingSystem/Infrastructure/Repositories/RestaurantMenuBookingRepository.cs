using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using RestaurantBookingSystem.Helpers;
using RestaurantBookingSystem.Infrastructure.DataEntities;

namespace RestaurantBookingSystem.Infrastructure.Repositories
{
    // NOTE: These methods below are kept dynamic so as to save time in creating new classes for storing retrived data
    // NOTE: as they are only temp data which will be used for further processing by RestaurantBooking Repository Class
    // NOTE: Please note that following code are not at all good in performance and should be heavily optimised in future

    // ReSharper disable PossibleMultipleEnumeration

    /// <summary>
    /// A class to handle Retrival and CRUD operations with database for MenuItemBookings of a Restaurant
    /// </summary>
    public class RestaurantMenuBookingRepository
    {
        #region  RestaurantMenueBookingRepository Methods

        /// <summary>
        /// Finds all the Record with the specified BookingId
        /// </summary>
        /// <param name="bookingid">The bookingid of the record</param>
        /// <returns>Resturns the BookedMenuItems if operation is successful otherwise returns null</returns>
        public IEnumerable<RestaurantMenuItem> Find(int bookingid)
        {
            var items = new List<dynamic>();
            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("FindMenuItemBookingsByBookingId", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BOOKINGID", SqlDbType.BigInt).Value = bookingid;
                    GetAllRecord(items, cn, cmd);
                }
            }
            var tempmenulist = new RestaurantMenuItemRepository().SelectAll();
            var finalmenulist = items.Select(item => tempmenulist.Single(menuitem => menuitem.ItemId == item.ItemId));

            //Return empty list instead of null for now TODO:Fix this later
            return finalmenulist.Count() > 0 ? finalmenulist : new List<RestaurantMenuItem>();
        }

        /// <summary>
        /// Selects all the reords from the Database and Returns them
        /// </summary>
        /// <returns>Returns the IEnumerable List of found records</returns>
        public IEnumerable<dynamic> SelectAll()
        {
            var items = new List<dynamic>();
            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("SelectAllMenuItemBookings", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    GetAllRecord(items, cn, cmd);
                }
            }
            return items;
        }

        /// <summary>
        /// Adds new MenuItems Booking Record to DataBase
        /// </summary>
        /// <param name="thebooking">The RestaurantBooking instance for which these bookings are being done</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>Returns true if operation is successful otherwise returns false</returns>
        public bool Add(RestaurantBooking thebooking)
        {
            if (thebooking == null)
                throw new ArgumentNullException("thebooking");
            if (thebooking.BookingId < 1)
                throw new ArgumentOutOfRangeException("thebooking", thebooking.BookingId, "A new Menu Item Booking record can not be added for a booking with invalid Id");
            var res = 0;

            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                foreach (var item in thebooking.PrefferedMenuItems)
                {
                    using (var cmd = new SqlCommand("AddMenuItemBookings", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@BOOKINGID", SqlDbType.BigInt).Value = thebooking.BookingId;
                        cmd.Parameters.Add("@ITEMID", SqlDbType.BigInt).Value = item.ItemId;
                        cmd.Parameters.Add("@BOOKEDFOR", SqlDbType.DateTime).Value = thebooking.BookedFor.ToUniversalTime();

                        if (cn.State != ConnectionState.Open) cn.Open();
                        res += cmd.ExecuteNonQuery();
                    }
                }
            }
            return res == thebooking.PrefferedMenuItems.Count;
        }

        /// <summary>
        /// Updates the Available Record with new Details
        /// </summary>
        /// <param name="thebooking">The BookingId for which these bookings are being done</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns>Returns true if operation is successful otherwise returns false</returns>
        /// <remarks>This method actually deleates all the record with the specified booking id and calls Add() method</remarks>
        public bool Update(RestaurantBooking thebooking)
        {
            if (thebooking == null)
                throw new ArgumentNullException("thebooking");
            if (thebooking.BookingId < 1)
                throw new ArgumentOutOfRangeException("thebooking", thebooking.BookingId, "A new Menu Item Booking record can not be Updated for a booking with invalid Id");

            return Delete(thebooking.BookingId) && Add(thebooking);
        }

        /// <summary>
        /// Deletes the record with specified id
        /// </summary>
        /// <param name="bookingid">The id of the record that needs to be deleted</param>
        /// <returns>Returns true if operation is successful otherwise returns false</returns>
        public bool Delete(int bookingid)
        {
            if (bookingid < 1)
                return false;

            var menuitems = Find(bookingid);
            if (menuitems != null && menuitems.Count() > 0)
            {
                using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
                {
                    using (var cmd = new SqlCommand("DeleteMenuItemBookings", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@BOOKINGID", SqlDbType.BigInt).Value = bookingid;
                        cn.Open();
                        var res = cmd.ExecuteNonQuery();
                        return res > 0;
                    }
                }
            }
            return true;
        }

        #endregion

        /// <summary>
        /// A Utility method to Get all the Booked REstaurant Table from given SqlConnection and SqlCommand
        /// </summary>
        /// <param name="items">The 'dynamic' Collection which will be updated with Read Rows</param>
        /// <param name="cn">The SqlConnection object to use for Database connection</param>
        /// <param name="cmd">The SqlCommand object that will be used to retrive data</param>
        protected static void GetAllRecord(List<dynamic> items, SqlConnection cn, SqlCommand cmd)
        {
            cn.Open();
            var rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (rdr.Read())
            {
                dynamic item = new
                {
                    BookingId = rdr.TryGetDataAsInt(0),
                    ItemId = rdr.TryGetDataAsInt(1),
                    BookedFor = rdr.TryGetDataAsDateTime(2)
                };

                items.Add(item);
            }
            if (!rdr.IsClosed) rdr.Close();
        }
    }
    // ReSharper restore PossibleMultipleEnumeration
}