using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using RestaurantBookingSystem.Helpers;
using RestaurantBookingSystem.Infrastructure.DataEntities;
using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Infrastructure.Repositories
{
    /// <summary>
    /// A repository class to handle CRUD operations with database for Facebook User's Details
    /// </summary>
    public class FacebookUserDetailRepository //: IDataRepository<FacebookUserDetail> //commented as the add method was truncating returned long facebookid
    {
        #region Implementation of IDataRepository<FacebookUserDetail>

        /// <summary>
        /// Finds a Facebook User's Details with the specified Id
        /// </summary>
        /// <param name="id">The FBID of the Facebook User</param>
        /// <returns>Resturns the Facebook User's Details if operation is successful otherwise returns null</returns>
        public FacebookUserDetail Find(ulong id)
        {
            var items = new List<FacebookUserDetail>();
            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("FindFacebookUserByFacebookId", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@FACEBOOKID", SqlDbType.BigInt).Value = id;
                    GetAllRecord(items, cn, cmd);
                }
            }
            return items.Count > 0 ? items[0] : null;
        }

        /// <summary>
        /// Finds a Record with the specified Id
        /// </summary>
        /// <param name="id">The id of the record</param>
        /// <returns>Resturns the Record if operation is successful otherwise returns null</returns>
        [Obsolete("Do not use this method, use Find(ulong id) instead", false)]
        public FacebookUserDetail Find(int id)
        {
            return Find((ulong)id);
        }

        /// <summary>
        /// Selects all the reords from the Database and Returns them
        /// </summary>
        /// <returns>Returns the IEnumerable List of found Facebook User's Details</returns>
        public IEnumerable<FacebookUserDetail> SelectAll()
        {
            var items = new List<FacebookUserDetail>();
            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("SelectAllFacebookUsers", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    GetAllRecord(items, cn, cmd);
                }
            }
            return items;
        }

        /// <summary>
        /// Adds a new Facebook User's Details to DataBase
        /// </summary>
        /// <param name="facebookUser">Facebook User's Details to add</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns>Returns -1 if operation is unsuccessful otherwise value greater than 0</returns>
        public ulong Add(FacebookUserDetail facebookUser)
        {
            if (facebookUser == null)
                throw new ArgumentNullException("facebookuser");
            if (facebookUser.FacebookId == 0)
                throw new InvalidOperationException("To Add a Facebook User Detail the FBID of the User should be non zero");

            if (Find(facebookUser.FacebookId) != null)
                return 0;

            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("AddFacebookUser", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@FACEBOOKID", SqlDbType.BigInt).Value = facebookUser.FacebookId;
                    cmd.Parameters.Add("@OAUTHTOKEN", SqlDbType.VarChar, 300).Value = facebookUser.OAuthToken;
                    cmd.Parameters.Add("@PROFILELINK", SqlDbType.VarChar, 50).Value = facebookUser.ProfileLink.AbsoluteUri;
                    //cmd.Parameters.Add("@ALIVEFOR", SqlDbType.BigInt).Value = (long) Math.Floor(facebookUser.AliveFor.TotalSeconds); //This data seems to be inappropriate as AliveFor changes at the time of calculation
                    cmd.Parameters.Add("@EXPIRESON", SqlDbType.DateTime).Value = facebookUser.ExpiresOn;

                    cn.Open();
                    var res = cmd.ExecuteNonQuery();
                    return (ulong)res == 1UL ? facebookUser.FacebookId : 0;
                }
            }
        }

        /// <summary>
        /// Updates the Available Facebook User's Details with new Details
        /// </summary>
        /// <param name="facebookUser">The Modified Facebook User's Details which needs to be updated</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns>Returns true if operation is successful otherwise returns false</returns>
        public bool Update(FacebookUserDetail facebookUser)
        {
            if (facebookUser == null)
                throw new ArgumentNullException("facebookUser");
            if (facebookUser.FacebookId == 0)
                throw new InvalidOperationException("To upate a Facebook User Detail the FBID of the User should be non zero");
            if (Find(facebookUser.FacebookId) == null)
                return false;

            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("UpdateFacebookUser", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@FACEBOOKID", SqlDbType.BigInt).Value = facebookUser.FacebookId;
                    cmd.Parameters.Add("@OAUTHTOKEN", SqlDbType.VarChar, 300).Value = facebookUser.OAuthToken;
                    cmd.Parameters.Add("@PROFILELINK", SqlDbType.VarChar, 50).Value = facebookUser.ProfileLink.AbsoluteUri;
                    //cmd.Parameters.Add("@ALIVEFOR", SqlDbType.BigInt).Value = (long) Math.Floor(facebookUser.AliveFor.TotalSeconds); //This data seems to be inappropriate as AliveFor changes at the time of calculation
                    cmd.Parameters.Add("@EXPIRESON", SqlDbType.DateTime).Value = facebookUser.ExpiresOn;

                    cn.Open();
                    var res = cmd.ExecuteNonQuery();
                    return res == 1;
                }
            }
        }

        /// <summary>
        /// Deletes the Facebook User Detail with specified id
        /// </summary>
        /// <param name="id">The id of the Facebook User that needs to be deleted</param>
        /// <returns>Returns true if operation is successful otherwise returns false</returns>
        [Obsolete("Do not use this method, use Delete(ulong id) instead", false)]
        public bool Delete(int id)
        {
            return Delete((ulong)id);
        }

        /// <summary>
        /// Deletes the Facebook User's Details with specified id
        /// </summary>
        /// <param name="id">The id of the Facebook User's Details that needs to be deleted</param>
        /// <returns>Returns true if operation is successful otherwise returns false</returns>
        public bool Delete(ulong id)
        {
            if (Find(id) != null)
            {
                using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
                {
                    using (var cmd = new SqlCommand("DeleteFacebookUser", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@FACEBOOKID", SqlDbType.BigInt).Value = id;
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
        /// A Utility method to Get all the Facebook Users' Details from given SqlConnection and SqlCommand
        /// </summary>
        /// <param name="items">The Collection which will be updated with Read Rows</param>
        /// <param name="cn">The SqlConnection object to use for Database connection</param>
        /// <param name="cmd">The SqlCommand object that will be used to retrive data</param>
        protected static void GetAllRecord(IList<FacebookUserDetail> items, SqlConnection cn, SqlCommand cmd)
        {
            cn.Open();
            using (var rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
            {
                while (rdr.Read())
                {
                    var item = new FacebookUserDetail
                    {
                        FacebookId = rdr.TryGetDataAsUInt64(0),
                        OAuthToken = rdr.TryGetDataAsString(1),
                        ProfileLink = rdr.TryGetDataAsString(2) == String.Empty ? null : new Uri(rdr.TryGetDataAsString(2)),
                        ExpiresOn = rdr.TryGetDataAsDateTime(4)
                    };
                    items.Add(item);
                }
            }
        }
    }
}