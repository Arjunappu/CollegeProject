using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using RestaurantBookingSystem.Helpers;
using RestaurantBookingSystem.Infrastructure.DataEntities;
using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Infrastructure.Repositories
{
    public class BookingBillRepository : IDataRepository<BookingBill>
    {
        #region Implementation of IDataRepository<BookingBill>

        /// <summary>
        /// Finds a Bill with the specified Id
        /// </summary>
        /// <param name="id">The id of the bill</param>
        /// <returns>Resturns the Bill if operation is successful otherwise returns null</returns>
        public BookingBill Find(int id)
        {
            var bills = new List<BookingBill>();
            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("FindBookingBillById", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BILLID", SqlDbType.BigInt).Value = id;
                    GetAllRecord(bills, cn, cmd);
                }
            }
            return bills.Count > 0 ? bills[0] : null;
        }

        /// <summary>
        /// Selects all the reords from the Database and Returns them
        /// </summary>
        /// <returns>Returns the IEnumerable List of found Bills</returns>
        public IEnumerable<BookingBill> SelectAll()
        {
            var bills = new List<BookingBill>();
            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("SelectAllBookingBills", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    GetAllRecord(bills, cn, cmd);
                }
            }
            return bills;
        }

        /// <summary>
        /// Adds a new Bill to DataBase
        /// </summary>
        /// <param name="bill">Bill to add</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns>Returns the Id of added Item if operation is successful otherwise returns -1</returns>
        public int Add(BookingBill bill)
        {
            if (bill == null)
                throw new ArgumentNullException("bill");
            if (bill.BookingId == 0)
                throw new InvalidOperationException("For Adding a Bill the BookingID should be non Zero");

            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("AddBookingBill", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BOOKINGID", SqlDbType.BigInt).Value = bill.BookingId;
                    cmd.Parameters.Add("@DISCOUNTAMOUNT", SqlDbType.Money, 50).Value = bill.DiscountAmount;
                    cmd.Parameters.Add("@GROSSAMOUNT", SqlDbType.Money).Value = (int)bill.GrossAmount;
                    cmd.Parameters.Add("@NETAMOUNT", SqlDbType.Money).Value = bill.NetAmount;

                    cn.Open();
                    var res = Convert.ToInt32(cmd.ExecuteScalar());
                    return res > 0 ? res : -1;
                }
            }
        }

        /// <summary>
        /// Updates the Available Bill with new Details
        /// </summary>
        /// <param name="bill">The Modified Bill which needs to be updated</param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>Returns true if operation is successful otherwise returns false</returns>
        public bool Update(BookingBill bill)
        {
            if (bill == null)
                throw new ArgumentNullException("bill");
            if (bill.BillId == 0)
                throw new InvalidOperationException("For Updating a Bill the Id of the Bill should be non Zero");
            if (bill.BookingId == 0)
                throw new InvalidOperationException("For Updating a Bill the Booking ID should be non Zero");
            if (new RestaurantBookingRepository().Find(bill.BookingId) == null)
                return false;

            if (Find(bill.BillId) != null)
            {
                using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
                {
                    using (var cmd = new SqlCommand("UpdateBookingBill", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@BILLID", SqlDbType.BigInt).Value = bill.BillId;
                        cmd.Parameters.Add("@BOOKINGID", SqlDbType.BigInt).Value = bill.BookingId;
                        cmd.Parameters.Add("@DISCOUNTAMOUNT", SqlDbType.Money, 50).Value = bill.DiscountAmount;
                        cmd.Parameters.Add("@GROSSAMOUNT", SqlDbType.Money).Value = (int)bill.GrossAmount;
                        cmd.Parameters.Add("@NETAMOUNT", SqlDbType.Money).Value = bill.NetAmount;

                        cn.Open();
                        var res = cmd.ExecuteNonQuery();
                        return res == 1;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Deletes the Bill with specified id
        /// </summary>
        /// <param name="id">The id of the Bill that needs to be deleted</param>
        /// <returns>Returns true if operation is successful otherwise returns false</returns>
        public bool Delete(int id)
        {
            if (Find(id) != null)
            {
                using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
                {
                    using (var cmd = new SqlCommand("DeleteBookingBill", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@BILLID", SqlDbType.BigInt).Value = id;
                        cn.Open();
                        var res = cmd.ExecuteNonQuery();
                        return res == 1;
                    }
                }
            }
            return false;
        }

        #endregion

        /// <summary>
        /// Finds a Bill with the specified Id
        /// </summary>
        /// <param name="id">The id of the bill</param>
        /// <returns>Resturns the Bill if operation is successful otherwise returns null</returns>
        public IEnumerable<BookingBill> FindBillsByBookingId(int id)
        {
            var bills = new List<BookingBill>();
            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("FindBookingBillByBookingId", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BOOKINGID", SqlDbType.BigInt).Value = id;
                    GetAllRecord(bills, cn, cmd);
                }
            }
            return bills.Count > 0 ? bills : null;
        }

        /// <summary>
        /// A Utility method to Get all the Bills from given SqlConnection and SqlCommand
        /// </summary>
        /// <param name="bills">The Booking Bill Collection which will be updated with Read Rows</param>
        /// <param name="cn">The SqlConnection object to use for Database connection</param>
        /// <param name="cmd">The SqlCommand object that will be used to retrive data</param>
        protected static void GetAllRecord(IList<BookingBill> bills, SqlConnection cn, SqlCommand cmd)
        {
            cn.Open();
            var rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (rdr.Read())
            {
                var bill = new BookingBill(rdr.TryGetDataAsInt(0), rdr.TryGetDataAsInt(1))
                {
                    DiscountAmount = rdr.TryGetDataAsDecimal(2),
                    GrossAmount = rdr.TryGetDataAsDecimal(3),
                    NetAmount = rdr.TryGetDataAsDecimal(4)
                };

                bills.Add(bill);
            }
            if (!rdr.IsClosed) rdr.Close();
        }
    }
}