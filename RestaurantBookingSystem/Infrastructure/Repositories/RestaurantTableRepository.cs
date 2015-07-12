using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Microsoft.CSharp.RuntimeBinder;
using RestaurantBookingSystem.Helpers;
using RestaurantBookingSystem.Infrastructure.DataEntities;
using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Infrastructure.Repositories
{
    /// <summary>
    /// A class to handle Retrival and CRUD operations with database for Tables in a Restaurent
    /// </summary>
    public class RestaurantTableRepository : IDataRepository<RestaurantTable>
    {
        #region Implementation of IDataRepository<RestaurantTable>

        /// <summary>
        /// Finds a Table with the specified Id
        /// </summary>
        /// <param name="id">The id of the table</param>
        /// <returns>Returns the table if operation is successful otherwise returns null</returns>
        public RestaurantTable Find(int id)
        {
            var tables = new List<RestaurantTable>();
            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("FindTableById", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@TABLEID", SqlDbType.BigInt).Value = id;
                    GetAllRecord(tables, cn, cmd);
                }
            }
            return tables.Count > 0 ? tables[0] : null;
        }

        /// <summary>
        /// Selects all the tables from the Database and Returns them
        /// </summary>
        /// <returns>Returns the IEnumerable List of found tables</returns>
        public IEnumerable<RestaurantTable> SelectAll()
        {
            var tables = new List<RestaurantTable>();
            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("SelectAllRestaurantTable", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    GetAllRecord(tables, cn, cmd);
                }
            }
            return tables;
        }

        /// <summary>
        /// Adds a new Table to DataBase
        /// </summary>
        /// <param name="table">Table to add</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>Returns the Id of added Item if operation is successful otherwise returns -1</returns>
        public int Add(RestaurantTable table)
        {
            if (table == null)
                throw new ArgumentNullException("table");
            if (SelectAll().Any(t => t.Position == table.Position))
                throw new Exception(String.Format("A Table already exist with the given Position of X:{0}, Y:{1}", table.Position.X,table.Position.Y));

            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("AddRestaurantTable", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ALIGNMENT", SqlDbType.TinyInt).Value = (int)table.Alignment;
                    cmd.Parameters.Add("@POSITIONX", SqlDbType.Int).Value = table.Position.X;
                    cmd.Parameters.Add("@POSITIONY", SqlDbType.Int).Value = table.Position.Y;
                    cmd.Parameters.Add("@TABLETYPE", SqlDbType.TinyInt).Value = (int)table.TableType;
                    cmd.Parameters.Add("@PRICE", SqlDbType.Int).Value = table.Price;
                    cmd.Parameters.Add("@FLOORPLANFILENAME", SqlDbType.Text).Value = table.FloorPlanFileName;

                    cn.Open();
                    var res = Convert.ToInt32(cmd.ExecuteScalar());
                    return res > 0 ? res : -1;
                }
            }
        }

        /// <summary>
        /// Updates the Available table with new Details
        /// </summary>
        /// <param name="table">The Modified table which needs to be updated</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns>Returns true if operation is successful otherwise returns false</returns>
        public bool Update(RestaurantTable table)
        {
            if (table == null)
                throw new ArgumentNullException("table");
            if (table.TableId == 0)
                throw new InvalidOperationException("For Updating a Table the Id of the Table should be non Zero");
            var existingtable = SelectAll().SingleOrDefault(t => t.Position == table.Position && t.TableId != table.TableId);
            if (existingtable != null)
                throw new InvalidOperationException(
                    String.Format(
                        "Table Id:{0} with Position ({1}{2}) already exist, please choose a new position or Update that table with another position",
                        existingtable.TableId, existingtable.Position.X, existingtable.Position.Y));

            if (Find(table.TableId) != null)
            {
                using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
                {
                    using (var cmd = new SqlCommand("UpdateRestaurantTable", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@TABLEID", SqlDbType.BigInt).Value = table.TableId;
                        cmd.Parameters.Add("@ALIGNMENT", SqlDbType.TinyInt).Value = (int)table.Alignment;
                        cmd.Parameters.Add("@POSITIONX", SqlDbType.Int, 50).Value = table.Position.X;
                        cmd.Parameters.Add("@POSITIONY", SqlDbType.Int, 50).Value = table.Position.Y;
                        cmd.Parameters.Add("@TABLETYPE", SqlDbType.TinyInt).Value = (int)table.TableType;
                        cmd.Parameters.Add("@PRICE", SqlDbType.Int).Value = table.Price;
                        cmd.Parameters.Add("@FLOORPLANFILENAME", SqlDbType.Text).Value = table.FloorPlanFileName;

                        cn.Open();
                        var res = cmd.ExecuteNonQuery();
                        return res == 1;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Deletes the table with specified id
        /// </summary>
        /// <param name="id">The id of the table that needs to be deleted</param>
        /// <returns>Returns true if operation is successful otherwise returns false</returns>
        public bool Delete(int id)
        {
            if (Find(id) != null)
            {
                using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
                {
                    using (var cmd = new SqlCommand("DeleteRestaurantTable", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@TABLEID", SqlDbType.BigInt).Value = id;
                        cn.Open();
                        var res = cmd.ExecuteNonQuery();
                        return res == 1;
                    }
                }
            }
            return false;
        }

        #endregion

        // ReSharper disable PossibleMultipleEnumeration
        /// <summary>
        /// Returns a list of all Tables in Restaurant with their Status within Given Time Period
        /// </summary>
        /// <returns>Dictionary of List of tables with DateTime as Key</returns>
        public IDictionary<DateTime, IEnumerable<RestaurantTable>> SelectAll(DateTime fromdatetime, DateTime todatetime)
        {
            fromdatetime = fromdatetime.ToUniversalTime();
            todatetime = todatetime.ToUniversalTime();
            var allTables = SelectAll();
            var allTableCount = allTables.Count();
            var bookedTableGroupList = SelectBookedTables(fromdatetime, todatetime);
            var bookedTableListCount = bookedTableGroupList.Count;
            var resultTableDictionary = new Dictionary<DateTime, IEnumerable<RestaurantTable>>(bookedTableListCount);

            var defaultfloor = new Func<DateTime, DateTime>(d => d.Floor((long)AppConfigHelper.BookingSlotMinutes, DateTimeHelper.DateTimePrecisionLevel.Minutes));
            fromdatetime = defaultfloor(fromdatetime);
            todatetime = defaultfloor(todatetime);
            var getminuteslots = new Func<ulong, double>(i => Math.Floor((todatetime - fromdatetime).TotalMinutes / (i * AppConfigHelper.BookingSlotMinutes)));
            for (var i = 1UL; getminuteslots(i) > 0; i++)
            {
                //Initially set all tables to be vacant
                var tempTableList = new List<RestaurantTable>(allTableCount);
                tempTableList.AddRange(allTables.Select(
                    table =>
                    {
                        table.Status = RestaurantTable.RestaurentTableStatus.Vacant;
                        return table;
                    }));
                foreach (var bookedTableList in bookedTableGroupList)
                {
                    foreach (var bookedTable in bookedTableList.Value)
                    {
                        tempTableList.Single(table => table.TableId == bookedTable.TableId).Status = bookedTable.Status;
                    }

                    Debug.Assert(bookedTableList.Key == fromdatetime.AddMinutes(AppConfigHelper.BookingSlotMinutes* (i - 1)));
                }
                resultTableDictionary.Add(fromdatetime.AddMinutes(AppConfigHelper.BookingSlotMinutes * (i - 1)), tempTableList);
            }

            //for (var i = 0; i < bookedTableListCount; ++i)
            //{
            //    var bookedTableList = bookedTableGroupList.ElementAt(i);
            //    var tempTableList = new List<RestaurantTable>(allTableCount);
            //    tempTableList.AddRange(allTables.Select(
            //        table =>
            //        {
            //            table.Status = RestaurantTable.RestaurentTableStatus.Vacant;
            //            return table;
            //        }));
            //    var bookedTableListValueCount = bookedTableList.Value.Count();
            //    for (var j = 0; j < bookedTableListValueCount; ++j)
            //    {
            //        var bookedTable = bookedTableList.Value.ElementAt(j);
            //        tempTableList.Single(table => table.TableId == bookedTable.TableId).Status = bookedTable.Status;
            //    }
            //    resultTableDictionary.Add(bookedTableList.Key, tempTableList);
            //}
            return resultTableDictionary;
        }

        /// <summary>
        /// Returns a list of all Booked Tables in Restaurant with their Status within Given Time Period
        /// </summary>
        /// <returns>Dictionary of List of tables with DateTime as Key</returns>
        public IDictionary<DateTime, IEnumerable<RestaurantTable>> SelectBookedTables(DateTime fromdatetime, DateTime todatetime)
        {
            fromdatetime = fromdatetime.ToUniversalTime();
            todatetime = todatetime.ToUniversalTime();
            var tables = new List<dynamic>();
            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("SelectTableBookings", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@FROMDATE", SqlDbType.DateTime).Value = fromdatetime;
                    cmd.Parameters.Add("@TODATE", SqlDbType.DateTime).Value = todatetime;
                    GetAllRecord(tables, cn, cmd);
                }
            }

            return tables.
                 GroupBy(key => ((DateTime)key.BookedFor).ToUniversalTime().Floor((long)AppConfigHelper.BookingSlotMinutes, DateTimeHelper.DateTimePrecisionLevel.Minutes), (key, value) => new { key, value }).
                 ToDictionary(key => key.key, value => ParseDynamicAsRestaurantTable(value.value));
        }

        private IEnumerable<RestaurantTable> ParseDynamicAsRestaurantTable(IEnumerable<dynamic> list)
        {
            if (list == null)
                return null;

            var resultlist = new List<RestaurantTable>(list.Count());
            try
            {
                resultlist.AddRange(list.Select(item => new RestaurantTable(item.TableId)
                                                            {
                                                                Alignment = item.Alignment,
                                                                Position = item.Position,
                                                                TableType = item.TableType,
                                                                Price = item.Price,
                                                                //TODO: Find a way to stop this forcefull setting of Occupied status,
                                                                //most probably adding somthing as a 'Checkin' Facility in database and this application
                                                                Status = item.BookedFor.ToUniversalTime() <= DateTime.UtcNow && item.BookedTill.ToUniversalTime() >= DateTime.UtcNow ? RestaurantTable.RestaurentTableStatus.Occupied : RestaurantTable.RestaurentTableStatus.Booked
                                                            }));
            }
            catch (RuntimeBinderException)
            {
                return null;
            }

            return resultlist;
        }

        /// <summary>
        /// A Utility method to test if specified tables are booked within the specified time frame
        /// </summary>
        /// <param name="tables">List of table that need to be checked</param>
        /// <param name="fromdatetime">The DateTime from which to check</param>
        /// <param name="todatetime">The DateTime till which to check</param>
        /// <returns></returns>
        public bool AreTheseTablesBooked(IEnumerable<RestaurantTable> tables, DateTime fromdatetime, DateTime todatetime)
        {
            var bookedtablesdictonary = SelectBookedTables(fromdatetime, todatetime);
            if (bookedtablesdictonary.Count > 0)
            {
                if (bookedtablesdictonary.ContainsKey(fromdatetime))
                {
                    return (from bookedtables in bookedtablesdictonary from bookedtable in bookedtables.Value select tables.Any(table => table.TableId == bookedtable.TableId)).FirstOrDefault();
                }
                return false;
            }
            return false;
        }
        // ReSharper restore PossibleMultipleEnumeration

        ///// <summary>
        ///// Gets only the list of table in the Restaurent which are <see cref="RestaurantTable.RestaurentTableStatus.Vacant"/>
        ///// </summary>
        ///// <returns>List of Vacant tables</returns>
        //public IList<RestaurantTable> GetAllVacantTables(DateTime fromdatetime)
        //{
        //    return
        //        (from table in SelectTablesBookings(fromdatetime, fromdatetime.AddMinutes(AppConfigHelper.BookingSlotMinutes))
        //         where table.Status == RestaurantTable.RestaurentTableStatus.Vacant
        //         select table).ToList();
        //}

        /// <summary>
        /// A Utility method to Get all the Tables from given SqlConnection and SqlCommand
        /// </summary>
        /// <param name="items">The Restaurant Table Collection which will be updated with Read Rows</param>
        /// <param name="cn">The SqlConnection object to use for Database connection</param>
        /// <param name="cmd">The SqlCommand object that will be used to retrive data</param>
        protected static void GetAllRecord(IList<RestaurantTable> items, SqlConnection cn, SqlCommand cmd)
        {
            cn.Open();
            var rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (rdr.Read())
            {
                var item = new RestaurantTable(rdr.TryGetDataAsInt(0))
                {
                    Alignment = (RestaurantTable.RestaurantTableAlignment)rdr.TryGetDataAsInt(1),
                    Position = new Point { X = rdr.TryGetDataAsInt(2), Y = rdr.TryGetDataAsInt(3) },
                    TableType = (RestaurantTable.RestaurantTableType)rdr.TryGetDataAsInt(4),
                    Price = rdr.TryGetDataAsInt(5),
                    FloorPlanFileName = rdr.TryGetDataAsString(6),
                    Status = RestaurantTable.RestaurentTableStatus.NotApplicable
                };

                items.Add(item);
            }
            if (!rdr.IsClosed) rdr.Close();
        }

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
                    TableId = rdr.TryGetDataAsInt(0),
                    Alignment = (RestaurantTable.RestaurantTableAlignment)rdr.TryGetDataAsInt(1),
                    Position = new Point { X = rdr.TryGetDataAsInt(2), Y = rdr.TryGetDataAsInt(3) },
                    TableType = (RestaurantTable.RestaurantTableType)rdr.TryGetDataAsInt(4),
                    Price = rdr.TryGetDataAsInt(5),
                    BookedFor = rdr.TryGetDataAsDateTime(6),
                    BookedTill = rdr.TryGetDataAsDateTime(7),
                    BookingId = rdr.TryGetDataAsInt(8),
                    FloorPlanFileName = rdr.TryGetDataAsString(9)
                };

                items.Add(item);
            }
            if (!rdr.IsClosed) rdr.Close();
        }
    }
}