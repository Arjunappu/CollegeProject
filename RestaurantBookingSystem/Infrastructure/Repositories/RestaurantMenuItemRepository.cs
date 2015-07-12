using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using RestaurantBookingSystem.Helpers;
using RestaurantBookingSystem.Infrastructure.DataEntities;

namespace RestaurantBookingSystem.Infrastructure.Repositories
{
    /// <summary>
    /// A repository class to handle Retrival and CRUD operations with database for Restaurent Menu items
    /// </summary>
    public class RestaurantMenuItemRepository : IDataRepository<RestaurantMenuItem>
    {

        #region Implementation of IDataRepository<RestaurantMenuItem>

        /// <summary>
        /// Finds a Menu Item with the specified Id
        /// </summary>
        /// <param name="id">The id of the Menu Item</param>
        /// <returns>Resturns the Menu Item if operation is successful otherwise returns null</returns>
        public RestaurantMenuItem Find(int id)
        {
            var items = new List<RestaurantMenuItem>();
            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("FindRestaurantMenuItemByItemId", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ITEMID", SqlDbType.BigInt).Value = id;
                    GetAllRecord(items, cn, cmd);
                }
            }
            return items.Count > 0 ? items[0] : null;
        }

        /// <summary>
        /// Selects all the Menu Items from the Database and Returns them
        /// </summary>
        /// <returns>Returns the IEnumerable List of found Menu Items</returns>
        public IEnumerable<RestaurantMenuItem> SelectAll()
        {
            var items = new List<RestaurantMenuItem>();
            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("SelectAllRestaurantMenuItem", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    GetAllRecord(items, cn, cmd);
                }
            }
            return items;
        }

        /// <summary>
        /// Adds a new Menu Item to DataBase
        /// </summary>
        /// <param name="item">Menu Item to add</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>Returns the Id of added Item if operation is successful otherwise returns -1</returns>
        public int Add(RestaurantMenuItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("AddRestaurantMenuItem", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@DESCRIPTION", SqlDbType.Text).Value = item.Description;
                    cmd.Parameters.Add("@NAME", SqlDbType.VarChar, 100).Value = item.Name;
                    cmd.Parameters.Add("@PICTUREFILE", SqlDbType.Text).Value = item.PictureFile;
                    cmd.Parameters.Add("@PRICE", SqlDbType.Money).Value = item.Price;

                    cn.Open();
                    var res = Convert.ToInt32(cmd.ExecuteScalar());
                    return res > 0 ? res : -1;
                }
            }
        }

        /// <summary>
        /// Updates the Available Menu Item with new Details
        /// </summary>
        /// <param name="item">The Modified Menu Item which needs to be updated</param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>Returns true if operation is successful otherwise returns false</returns>
        public bool Update(RestaurantMenuItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            if (item.ItemId == 0)
                throw new InvalidOperationException("For Updating a Menu Item the Id of the Menu Item should be non Zero");

            if (Find(item.ItemId) != null)
            {
                using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
                {
                    using (var cmd = new SqlCommand("UpdateRestaurantMenuItem", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ITEMID", SqlDbType.BigInt).Value = item.ItemId;
                        cmd.Parameters.Add("@DESCRIPTION", SqlDbType.Text).Value = item.Description;
                        cmd.Parameters.Add("@NAME", SqlDbType.VarChar, 100).Value = item.Name;
                        cmd.Parameters.Add("@PICTUREFILE", SqlDbType.Text).Value = item.PictureFile;
                        cmd.Parameters.Add("@PRICE", SqlDbType.Int).Value = item.Price;

                        cn.Open();
                        var res = cmd.ExecuteNonQuery();
                        return res == 1;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Deletes the Menu Item with specified id
        /// </summary>
        /// <param name="id">The id of the Menu Item that needs to be deleted</param>
        /// <returns>Returns true if operation is successful otherwise returns false</returns>
        public bool Delete(int id)
        {
            if (Find(id) != null)
            {
                using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
                {
                    using (var cmd = new SqlCommand("DeleteRestaurantMenuItem", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ITEMID", SqlDbType.BigInt).Value = id;
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
        /// Selects all the Menu Items from the Database which are Booked on given Date and Returns them
        /// </summary>
        /// <param name="fordate">A Date for which the search will be done</param>
        /// <returns>Returns the IEnumerable List of found Menu Items</returns>
        public IEnumerable<BookedRestaurantMenuItem> SelectBookedMenuItems(DateTime fordate)
        {
            var items = new List<dynamic>();
            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("SelectMenuItemBookings", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@FORDATE", SqlDbType.DateTime).Value = fordate.ToUniversalTime();
                    GetAllRecord(items, cn, cmd);
                }
            }

            //Now Aggregate the retrived result
            var result = new List<BookedRestaurantMenuItem>();
            foreach(var item in items)
            {
                var tmp = item;
                if (result.FirstOrDefault(bmitem => bmitem.ItemId == tmp.ItemId) == null)
                {
                    result.Add(new BookedRestaurantMenuItem(item.ItemId)
                                   {
                                       Description = item.Description,
                                       Name = item.Name,
                                       PictureFile = item.PictureFile,
                                       Price = item.Price,
                                       Qty = items.Count(ditem => ditem.ItemId == tmp.ItemId),
                                       BookedFor = item.BookedFor.ToUniversalTime()
                                   });
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the specified number of menu items with newer first
        /// </summary>
        /// <param name="count">The number of Items to return</param>
        /// <returns>Return a IEnumerable List of Newer MenuItems</returns>
        public IEnumerable<RestaurantMenuItem> GetNewMenuItems(int count)
        {
            return count < 1 ? null : SelectAll().OrderByDescending(item => item.ItemId).Take(count);
        }

        /// <summary>
        /// A Utility method to Get all the records from given SqlConnection and SqlCommand
        /// </summary>
        /// <param name="items">The Collection which will be updated with Read Rows</param>
        /// <param name="cn">The SqlConnection object to use for Database connection</param>
        /// <param name="cmd">The SqlCommand object that will be used to retrive data</param>
        protected static void GetAllRecord(IList<RestaurantMenuItem> items, SqlConnection cn, SqlCommand cmd)
        {
            cn.Open();
            using (var rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
            {
                while (rdr.Read())
                {
                    var item = new RestaurantMenuItem(rdr.TryGetDataAsInt(0))
                                   {
                                       Description = rdr.TryGetDataAsString(1),
                                       Name = rdr.TryGetDataAsString(2),
                                       PictureFile = rdr.TryGetDataAsString(3),
                                       Price = rdr.TryGetDataAsDecimal(4)
                                   };
                    items.Add(item);
                }
            }
        }

        /// <summary>
        /// A Utility method to Get all the Booked REstaurant Menu from given SqlConnection and SqlCommand
        /// </summary>
        /// <param name="items">The 'dynamic' Collection which will be updated with Read Rows</param>
        /// <param name="cn">The SqlConnection object to use for Database connection</param>
        /// <param name="cmd">The SqlCommand object that will be used to retrive data</param>
        protected static void GetAllRecord(IList<dynamic> items, SqlConnection cn, SqlCommand cmd)
        {
            cn.Open();
            var rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (rdr.Read())
            {
                var item = new
                {
                    ItemId = rdr.TryGetDataAsInt(0),
                    Description = rdr.TryGetDataAsString(1),
                    Name = rdr.TryGetDataAsString(2),
                    PictureFile = rdr.TryGetDataAsString(3),
                    Price = rdr.TryGetDataAsDecimal(4),
                    BookingId = rdr.TryGetDataAsInt(5),
                    BookedFor = rdr.TryGetDataAsDateTime(6)
                };

                items.Add(item);
            }
            if (!rdr.IsClosed) rdr.Close();
        }
    }
}