using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using RestaurantBookingSystem.Helpers;
using RestaurantBookingSystem.Infrastructure.DataEntities;
using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Infrastructure.Repositories
{
    public class RestaurantBookingRepository : IDataRepository<RestaurantBooking>
    {
        #region Implementation of IDataRepository<RestaurantBooking>


        /// <summary>
        /// Finds a Record with the specified Id
        /// </summary>
        /// <param name="id">The id of the record</param>
        /// <returns>Resturns the Record if operation is successful otherwise returns null</returns>
        public RestaurantBooking Find(int id)
        {
            var users = new List<RestaurantBooking>();
            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("FindRestaurantBookingByBookingId", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BOOKINGID", SqlDbType.BigInt).Value = id;
                    GetAllRecord(users, cn, cmd);
                }
            }
            return users.Count > 0 ? users[0] : null;
        }

        /// <summary>
        /// Selects all the reords from the Database and Returns them
        /// </summary>
        /// <returns>Returns the IEnumerable List of found records</returns>
        public IEnumerable<RestaurantBooking> SelectAll()
        {
            var users = new List<RestaurantBooking>();
            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("SelectAllBRestaurantBookings", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    GetAllRecord(users, cn, cmd);
                }
            }
            return users;
        }

        /// <summary>
        /// Adds a new Record to DataBase
        /// </summary>
        /// <param name="booking">Record to add</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns>Returns the Id of added Item if operation is successful otherwise returns -1</returns>
        public int Add(RestaurantBooking booking)
        {
            int result;

            if (booking == null)
                throw new ArgumentNullException("booking");
            if (booking.Bills.Count < 1)
                throw new InvalidOperationException("A new Restaurant Booking cannot be added without a Bill");
            if (booking.BookingCustomer == null)
                throw new InvalidOperationException("A new Restaurant Booking cannot be added without a Valid Customer");
            if (booking.BookedTables.Count < 1)
                throw new InvalidOperationException("A new Restaurant Booking cannot be added without at least one table selected");
            if (booking.Status == RestaurantBooking.BookingStatus.Confirmed || booking.Status == RestaurantBooking.BookingStatus.Served)
                throw new InvalidOperationException("A Restaurant Booking with Confirmed or Served Status cannot be added to DataBase");

            booking.Status = RestaurantBooking.BookingStatus.InProcess;
            booking.BookedFor = booking.BookedFor.ToUniversalTime().Floor((long)AppConfigHelper.BookingSlotMinutes,
                                                        DateTimeHelper.DateTimePrecisionLevel.Minutes);
            booking.BookedTill = booking.BookedTill.ToUniversalTime().Floor((long)AppConfigHelper.BookingSlotMinutes,
                                                         DateTimeHelper.DateTimePrecisionLevel.Minutes);

            var bookingCustomerid = booking.BookingCustomer.UserId;
            if (bookingCustomerid == 0)
                bookingCustomerid = new RestaurantUserRepository().Add(booking.BookingCustomer);

            //Checks to see if the tables selected are already booked
            if (new RestaurantTableRepository().AreTheseTablesBooked(booking.BookedTables, booking.BookedFor.ToUniversalTime(), booking.BookedTill))
                throw new InvalidOperationException("A Restaurant Booking cannot be done as One or More of the Selected Table are already booked");

            booking.Status = RestaurantBooking.BookingStatus.Confirmed;
            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("AddRestaurantBooking", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@CUSTOMERID", SqlDbType.BigInt).Value = bookingCustomerid;
                    cmd.Parameters.Add("@BOOKEDON", SqlDbType.DateTime).Value = DateTime.UtcNow;
                    cmd.Parameters.Add("@BOOKEDFOR", SqlDbType.DateTime).Value = booking.BookedFor.ToUniversalTime();
                    cmd.Parameters.Add("@BOOKEDTILL", SqlDbType.DateTime).Value = booking.BookedTill.ToUniversalTime();
                    cmd.Parameters.Add("@BOOKINGSTATUS", SqlDbType.TinyInt).Value = (int)booking.Status;

                    cn.Open();
                    var res = Convert.ToInt32(cmd.ExecuteScalar());
                    result = res > 0 ? res : -1;
                }
            }
            // if adding was not successful then return immediatly
            if (result < 1) return result;

            //refresh the booking with new bookingid recived in result
            booking = new RestaurantBooking(result)
                          {
                              Bills = booking.Bills,
                              BookedFor = booking.BookedFor.ToUniversalTime(),
                              BookedOn = booking.BookedOn,
                              BookedTables = booking.BookedTables,
                              BookedTill = booking.BookedTill.ToUniversalTime(),
                              BookingCustomer = booking.BookingCustomer,
                              PrefferedMenuItems = booking.PrefferedMenuItems,
                              Status = booking.Status
                          };
            //Finally Add new Table and Menu Booking Detail to the DataBase
            if (!((new RestaurantTableBookingRepository().Add(booking))
                &&
                (new RestaurantMenuBookingRepository().Add(booking))))
                return result;

            //And Add the bill ofcourse
            if (result > 0)
            {
                var bookingbill = booking.Bills;
                var repo = new BookingBillRepository();
                foreach (var bookingBill in bookingbill)
                    repo.Add(new BookingBill(result)
                                 {
                                     DiscountAmount = bookingBill.DiscountAmount,
                                     GrossAmount = bookingBill.GrossAmount,
                                     NetAmount = bookingBill.NetAmount,
                                 });
            }
            return result;
        }

        /// <summary>
        /// Updates the Available Record with new Details (Do not use this to cancel a Booking)
        /// </summary>
        /// <param name="booking">The Modified Record which needs to be updated</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns>Returns true if operation is successful otherwise returns false</returns>
        public bool Update(RestaurantBooking booking)
        {
            bool result;
            if (booking == null)
                throw new ArgumentNullException("booking");
            if (Find(booking.BookingId) == null)
                return false;

            if (booking.Status != RestaurantBooking.BookingStatus.Confirmed || booking.Status != RestaurantBooking.BookingStatus.InProcess)
                throw new InvalidOperationException("Only a Booking which is Confirmed or is In Process can be altered");
            if (booking.Bills.Count < 1)
                throw new InvalidOperationException("A Booking cannot be altered without the Bills Details");
            if (booking.BookingCustomer == null || booking.BookingCustomer.UserId < 1)
                throw new InvalidOperationException("A Booking Cannot be altered without details of the customer who booked it");
            if (booking.BookedTables.Count < 1)
                throw new InvalidOperationException("A Booking cannot be altered if it does not have any Booked Table Details");

            if (new RestaurantTableBookingRepository().Delete(booking.BookingId))
                //Checks to see if the tables selected are already booked
                if (new RestaurantTableRepository().AreTheseTablesBooked(booking.BookedTables, booking.BookedFor.ToUniversalTime(), booking.BookedTill.ToUniversalTime()))
                    throw new InvalidOperationException("Cannot update with new Table selection as One or More of them are already booked");

            booking.Status = RestaurantBooking.BookingStatus.Confirmed;
            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("UpdateRestaurantBooking", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BOOKINGID", SqlDbType.BigInt).Value = booking.BookingId;
                    cmd.Parameters.Add("@CUSTOMERID", SqlDbType.BigInt).Value = booking.BookingCustomer.UserId;
                    cmd.Parameters.Add("@BOOKEDON", SqlDbType.DateTime).Value = booking.BookedOn;
                    cmd.Parameters.Add("@BOOKEDFOR", SqlDbType.DateTime).Value = booking.BookedFor.ToUniversalTime();
                    cmd.Parameters.Add("@BOOKEDTILL", SqlDbType.DateTime).Value = booking.BookedTill.ToUniversalTime();
                    cmd.Parameters.Add("@BOOKINGSTATUS", SqlDbType.TinyInt).Value = (int)booking.Status;

                    cn.Open();
                    var res = cmd.ExecuteNonQuery();
                    result = res == 1;
                }
            }

            //Finally Update the Table and Menu bookings and expect Table booking to returns false as Delete was already called on it
            if (result)
                result = new RestaurantTableBookingRepository().Update(booking) |
                         new RestaurantMenuBookingRepository().Update(booking);

            //And Update the bill ofcourse
            if (result)
            {
                var bookingbill = booking.Bills;
                var repo = new BookingBillRepository();
                foreach (var bookingBill in bookingbill)
                    result = bookingBill.BillId < 1 ?
                        repo.Add(new BookingBill(booking.BookingId)
                                     {
                                         DiscountAmount = bookingBill.DiscountAmount,
                                         GrossAmount = bookingBill.GrossAmount,
                                         NetAmount = bookingBill.NetAmount
                                     }) > 0
                                     : repo.Update(bookingBill);

            }
            return result;
        }

        /// <summary>
        /// Deletes the record with specified id
        /// </summary>
        /// <param name="id">The id of the record that needs to be deleted</param>
        /// <returns>Returns true if operation is successful otherwise returns false</returns>
        public bool Delete(int id)
        {
            var result = false;
            var booking = Find(id);
            if (booking != null)
            {
                //First Delete refrencing MenuBooking Records
                result = new RestaurantMenuBookingRepository().Delete(booking.BookingId);

                //If successful then delete Table Booking Records
                if (result)
                    result = new RestaurantTableBookingRepository().Delete(booking.BookingId);

                //If successful then delete all associated Bill
                if (result)
                    result = booking.Bills
                        .Select(bill => new BookingBillRepository().Delete(bill.BillId))
                        .All(deleted => deleted);

                //If all goes well then delete the booking
                if (result)
                {
                    using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
                    {
                        using (var cmd = new SqlCommand("DeleteRestaurantBooking", cn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@BOOKINGID", SqlDbType.BigInt).Value = id;
                            cn.Open();
                            var res = cmd.ExecuteNonQuery();
                            result = res == 1;
                        }
                    }
                }
            }
            return result;
        }

        #endregion

        /// <summary>
        /// Cancells the Restaurant Booking with specfied booking id
        /// </summary>
        /// <param name="bookingid">The id of the Booking which needs to be cancelled</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns>Returns true if operation is successful otherwise returns false</returns>
        public bool Cancel(int bookingid)
        {
            var booking = Find(bookingid);
            bool result;

            if (booking == null)
                throw new ArgumentException(String.Format("A Booking with booking id:{0} does not exist!", bookingid), "bookingid");
            if (booking.Status != RestaurantBooking.BookingStatus.Confirmed)
                throw new InvalidOperationException("A booking cannot be cancelled if it is not a Confirmed Booking");

            booking.Status = RestaurantBooking.BookingStatus.Cancelled;
            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("UpdateRestaurantBooking", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BOOKINGID", SqlDbType.BigInt).Value = booking.BookingId;
                    cmd.Parameters.Add("@CUSTOMERID", SqlDbType.BigInt).Value = booking.BookingCustomer.UserId;
                    cmd.Parameters.Add("@BOOKEDON", SqlDbType.DateTime).Value = booking.BookedOn;
                    cmd.Parameters.Add("@BOOKEDFOR", SqlDbType.DateTime).Value = booking.BookedFor.ToUniversalTime();
                    cmd.Parameters.Add("@BOOKEDTILL", SqlDbType.DateTime).Value = booking.BookedTill.ToUniversalTime();
                    cmd.Parameters.Add("@BOOKINGSTATUS", SqlDbType.TinyInt).Value = (int)booking.Status;

                    cn.Open();
                    var res = cmd.ExecuteNonQuery();
                    result = res == 1;
                }
            }

            if (result)
                result = new RestaurantTableBookingRepository().Delete(booking.BookingId)
                    & new RestaurantMenuBookingRepository().Delete(booking.BookingId);

            return result;
        }

        //TODO: Try fixing the expensive call to ToList() in method below

        /// <summary>
        /// A Utility method to Get all the Users from given SqlConnection and SqlCommand
        /// </summary>
        /// <param name="items">The RestaurantUser Collection which will be updated with Read Rows</param>
        /// <param name="cn">The SqlConnection object to use for Database connection</param>
        /// <param name="cmd">The SqlCommand object that will be used to retrive data</param>
        protected static void GetAllRecord(IList<RestaurantBooking> items, SqlConnection cn, SqlCommand cmd)
        {
            cn.Open();
            var rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (rdr.Read())
            {
                var item = new RestaurantBooking(rdr.TryGetDataAsInt(0))
                {
                    BookingCustomer = new RestaurantUserRepository().Find(rdr.TryGetDataAsInt(1)),
                    BookedOn = rdr.TryGetDataAsDateTime(2),
                    BookedFor = rdr.TryGetDataAsDateTime(3).ToUniversalTime(),
                    BookedTill = rdr.TryGetDataAsDateTime(4).ToUniversalTime(),
                    Status = (RestaurantBooking.BookingStatus)rdr.TryGetDataAsInt(5),
                    Bills = new BookingBillRepository().FindBillsByBookingId(rdr.TryGetDataAsInt(0)).ToList(),
                    BookedTables = new RestaurantTableBookingRepository().Find(rdr.TryGetDataAsInt(0)).ToList(),
                    PrefferedMenuItems = new RestaurantMenuBookingRepository().Find(rdr.TryGetDataAsInt(0)).ToList()
                };

                items.Add(item);
            }
            if (!rdr.IsClosed) rdr.Close();
        }
    }
}