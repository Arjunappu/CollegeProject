using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Security;
using RestaurantBookingSystem.Helpers;
using RestaurantBookingSystem.Infrastructure.DataEntities;
using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Infrastructure.Repositories
{
    /// <summary>
    /// A class to handle Retrival and CRUD operations with database for Users of a Restaurant
    /// </summary>
    public class RestaurantUserRepository : IDataRepository<UserBase>
    {
        #region Implementation of IDataRepository<UserBase>

        /// <summary>
        /// Finds a User with the specified Id
        /// </summary>
        /// <param name="id">The id of the User</param>
        /// <returns>Resturns the User Details if operation is successful otherwise returns null</returns>
        public UserBase Find(int id)
        {
            var users = new List<RestaurantUser>();
            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("FindRestaurantUserById", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@USERID", SqlDbType.BigInt).Value = id;
                    GetAllRecord(users, cn, cmd);
                }
            }
            return users.Count > 0 ? users[0] : null;
        }

        /// <summary>
        /// Selects all the User Details from the Database and Returns them
        /// </summary>
        /// <returns>Returns the IEnumerable List of found Users</returns>
        public IEnumerable<UserBase> SelectAll()
        {
            var users = new List<RestaurantUser>();
            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("SelectAllRestaurantUser", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    GetAllRecord(users, cn, cmd);
                }
            }
            return users;
        }

        /// <summary>
        /// Adds a new User to DataBase
        /// </summary>
        /// <param name="user">User to add</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns>Returns the Id of added Item if operation is successful otherwise returns -1</returns>
        public int Add(UserBase user)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            if (String.IsNullOrWhiteSpace(user.Name))
                throw new InvalidOperationException("User name cannot be a Null, Empty or WhiteSpace");

            var restaurantuser = UserBaseToRestaurantUser(user);

            if (restaurantuser.UserRole == UserBase.RestaurantUserRole.Guest && restaurantuser.MobileNumber.ToString().Length != 10)
                throw new InvalidOperationException("A Guest user cannot be added without a Valid Mobile Number");

            var facebookaddsuccess = 0UL;

            if (restaurantuser.FacebookDetail != null)
                facebookaddsuccess = new FacebookUserDetailRepository().Add(restaurantuser.FacebookDetail);

            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("AddRestaurantUser", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NAME", SqlDbType.VarChar, 100).Value = restaurantuser.Name;
                    if (restaurantuser.MobileNumber > 0)
                        cmd.Parameters.Add("@MOBILENUMBER", SqlDbType.BigInt).Value = restaurantuser.MobileNumber;
                    cmd.Parameters.Add("@ADDRESS", SqlDbType.Text).Value = restaurantuser.Address;
                    cmd.Parameters.Add("@EMAILID", SqlDbType.VarChar, 80).Value = restaurantuser.EmailId;
                    if (facebookaddsuccess > 0 && restaurantuser.FacebookDetail != null)
                        cmd.Parameters.Add("@FACEBOOKID", SqlDbType.BigInt).Value = restaurantuser.FacebookDetail.FacebookId;
                    cmd.Parameters.Add("@PASSWORD", SqlDbType.VarChar, 80).Value = HashPasswordForStoringInDb(restaurantuser.Password);
                    cmd.Parameters.Add("@USERROLE", SqlDbType.TinyInt).Value = (int)restaurantuser.UserRole;
                    cmd.Parameters.Add("@LOGINEXPIRESON", SqlDbType.DateTime).Value = restaurantuser.LoginExpiresOn;
                    cmd.Parameters.Add("@USERGUID", SqlDbType.UniqueIdentifier).Value = restaurantuser.UserGuid !=
                                                                                        Guid.Empty
                                                                                            ? restaurantuser.UserGuid
                                                                                            : (object)DBNull.Value;

                    cn.Open();
                    var res = Convert.ToInt32(cmd.ExecuteScalar());
                    return res > 0 ? res : -1;
                }
            }
        }

        /// <summary>
        /// Updates the Available User Details with new Details
        /// </summary>
        /// <param name="user">The Modified User Detail which needs to be updated</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns>Returns true if operation is successful otherwise returns false</returns>
        public bool Update(UserBase user)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            if (user.UserId == 0)
                throw new InvalidOperationException("To update  a Restaurant user the UserID should be non zero");
            if (String.IsNullOrWhiteSpace(user.Name))
                throw new InvalidOperationException("User name cannot be a Null, Empty or WhiteSpace");
            var prevuser = Find(user.UserId);
            if (prevuser == null)
                return false;

            var restaurantuser = UserBaseToRestaurantUser(user);

            if (restaurantuser.UserRole == UserBase.RestaurantUserRole.Guest && restaurantuser.MobileNumber < 1)
                throw new InvalidOperationException("A Guest user cannot be Update without a Valid Mobile Number");

            var facebookdetailexist = false;
            var facebookaddsuccess = 0UL;

            if (restaurantuser.FacebookDetail != null)
                facebookdetailexist = new FacebookUserDetailRepository().Update(restaurantuser.FacebookDetail);

            if (facebookdetailexist) facebookaddsuccess = restaurantuser.FacebookDetail.FacebookId;

            if (restaurantuser.FacebookDetail != null && !facebookdetailexist)
                facebookaddsuccess = new FacebookUserDetailRepository().Add(restaurantuser.FacebookDetail);

            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("UpdateRestaurantUser", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@USERID", SqlDbType.BigInt).Value = restaurantuser.UserId;
                    cmd.Parameters.Add("@NAME", SqlDbType.VarChar, 100).Value = restaurantuser.Name;
                    if (restaurantuser.MobileNumber > 0)
                        cmd.Parameters.Add("@MOBILENUMBER", SqlDbType.BigInt).Value = restaurantuser.MobileNumber;
                    cmd.Parameters.Add("@ADDRESS", SqlDbType.Text).Value = restaurantuser.Address;
                    cmd.Parameters.Add("@EMAILID", SqlDbType.VarChar, 80).Value = restaurantuser.EmailId;
                    if (facebookaddsuccess > 0 && restaurantuser.FacebookDetail != null)
                        cmd.Parameters.Add("@FACEBOOKID", SqlDbType.BigInt).Value = restaurantuser.FacebookDetail.FacebookId;
                    if (restaurantuser.Password != null)
                    {
                        cmd.Parameters.Add("@PASSWORD", SqlDbType.VarChar, 80).Value =
                            //if following conditions are met then the password string is already an unchanged hashed string
                            (
                                UserBaseToRestaurantUser(prevuser).Password == restaurantuser.Password
                                && restaurantuser.Password.Trim().Length == 73 && restaurantuser.Password.Trim().Split('.').Length == 2
                            )
                            ? restaurantuser.Password
                            : HashPasswordForStoringInDb(restaurantuser.Password);
                    }
                    cmd.Parameters.Add("@USERROLE", SqlDbType.TinyInt).Value = (int)restaurantuser.UserRole;
                    cmd.Parameters.Add("@LOGINEXPIRESON", SqlDbType.DateTime).Value = restaurantuser.LoginExpiresOn;
                    cmd.Parameters.Add("@USERGUID", SqlDbType.UniqueIdentifier).Value = restaurantuser.UserGuid !=
                                                                                        Guid.Empty
                                                                                            ? restaurantuser.UserGuid
                                                                                            : (object)DBNull.Value;

                    cn.Open();
                    var res = cmd.ExecuteNonQuery();
                    return res == 1;
                }
            }
        }

        /// <summary>
        /// Deletes the User with specified UserId
        /// </summary>
        /// <param name="id">The UserId of the User that needs to be deleted</param>
        /// <returns>Returns true if operation is successful otherwise returns false</returns>
        public bool Delete(int id)
        {
            if (Find(id) != null)
            {
                using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
                {
                    using (var cmd = new SqlCommand("DeleteRestaurantUser", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@USERID", SqlDbType.BigInt).Value = id;
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
        /// Finds a User with the specified UserGuid
        /// </summary>
        /// <param name="userguid">The Unique GUID of the User</param>
        /// <returns>Resturns the User Details if operation is successful otherwise returns null</returns>
        public UserBase Find(Guid userguid)
        {
            var users = new List<RestaurantUser>();
            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("FindRestaurantUserByUserGuid", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@USERGUID", SqlDbType.UniqueIdentifier).Value = userguid;
                    GetAllRecord(users, cn, cmd);
                }
            }
            return users.Count > 0 ? users[0] : null;
        }

        /// <summary>
        /// Checks if a user with specified UserId belongs to the specified <see cref="RestaurantUser.RestaurantUserRole"/>
        /// </summary>
        /// <param name="id">Id of the User</param>
        /// <returns>Returns true of the Role of the User match otherwise returns false</returns>
        public UserBase.RestaurantUserRole GetRestaurantUserRole(int id)
        {
            if (id < 1)
                return (UserBase.RestaurantUserRole)(-1);
            var user = Find(id);
            if (user == null)
                return (UserBase.RestaurantUserRole)(-2);
            return user.UserRole;
        }

        /// <summary>
        /// Check if user is Logged in or Not
        /// </summary>
        /// <param name="id">The id of the User</param>
        /// <returns>Returns true of User is logged in otherwise returns false</returns>
        public bool IsUserLoggedIn(int id)
        {
            if (id < 1)
                return false;
            var user = Find(id);
            if (user == null)
                return false;
            return user.LoginExpiresOn.ToUniversalTime() > DateTime.UtcNow;
        }

        /// <summary>
        /// A Utility method to convert a user of type or subtype of UserBase to RestaurantUser
        /// </summary>
        /// <param name="user">A user instance</param>
        /// <returns>Returns a User of Type RestaurantUser</returns>
        public static RestaurantUser UserBaseToRestaurantUser(UserBase user)
        {
            if (user == null) return null;
            if (user.GetType() == typeof(RestaurantUser))
                return user as RestaurantUser;
            return new RestaurantUser(user.UserId, user.Name, user.UserGuid, user.MobileNumber, user.LoginExpiresOn,
                                      user.UserRole, null, null, null, null, user);
        }

        /// <summary>
        /// A Utility method to do a ONE TIME hash of a Given string for storing in the Database
        /// </summary>
        /// <param name="password">The plaintext password string</param>
        /// <returns>A hashed string of given plaintext</returns>
        /// <remarks>This method does a ONE TIME hash of a Given string for storing in the Database, every time it will generate a new hash</remarks>
        public static string HashPasswordForStoringInDb(string password)
        {
            if(password == null)
                throw new ArgumentNullException("password", "Value cannot be null");
            Func<string, string, string> hasher = FormsAuthentication.HashPasswordForStoringInConfigFile;
            var salt = hasher(Guid.Empty.ToString("x"), "MD5");
            salt = hasher(String.Format("{0}{1}", salt, Guid.NewGuid().ToString("x")), "MD5");
            var temppassword = hasher(password, "MD5");
            //using a period (.) in between because some implementation of hash algo can have different output length
            return String.Format("{0}.{1}", hasher(String.Format("{0}{1}", temppassword, salt), "SHA1"), salt);
        }

        /// <summary>
        /// A Utility method to match a string with a string priviously hashed with <see cref="HashPasswordForStoringInDb"/> method
        /// </summary>
        /// <param name="plainpassword">The plaintext password string</param>
        /// <param name="hashedpassword">The hased password string</param>
        /// <returns>Returns true if hash match with the hash of Given plaintext password string, otherwise Returns false</returns>
        public static bool ValidatePasswordWithHash(string plainpassword, string hashedpassword)
        {
            if (plainpassword == null)
                throw new ArgumentNullException("plainpassword", "Cannot match with null value");
            if (hashedpassword.IsNullOrEmpty()) return false;
            if (hashedpassword.Split('.').Length != 2) return false;
            Func<string, string, string> hasher = FormsAuthentication.HashPasswordForStoringInConfigFile;
            var salt = hashedpassword.Split('.')[1];
            var temppassword = hasher(plainpassword, "MD5");
            return String.Equals(String.Format("{0}.{1}", hasher(String.Format("{0}{1}", temppassword, salt), "SHA1"), salt), hashedpassword);
        }

        /// <summary>
        /// A Utility method to Get all the Users from given SqlConnection and SqlCommand
        /// </summary>
        /// <param name="items">The RestaurantUser Collection which will be updated with Read Rows</param>
        /// <param name="cn">The SqlConnection object to use for Database connection</param>
        /// <param name="cmd">The SqlCommand object that will be used to retrive data</param>
        protected static void GetAllRecord(IList<RestaurantUser> items, SqlConnection cn, SqlCommand cmd)
        {
            cn.Open();
            var rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (rdr.Read())
            {
                var item = new RestaurantUser(rdr.TryGetDataAsInt(0), rdr.TryGetDataAsString(1), rdr.TryGetDataAsGuid(9))
                {
                    MobileNumber = rdr.TryGetDataAsUInt64(2),
                    Address = rdr.TryGetDataAsString(3),
                    EmailId = rdr.TryGetDataAsString(4),
                    FacebookDetail = new FacebookUserDetailRepository().Find(rdr.TryGetDataAsUInt64(5)),
                    Password = rdr.TryGetDataAsString(6),
                    UserRole = (UserBase.RestaurantUserRole)rdr.TryGetDataAsInt(7),
                    LoginExpiresOn = rdr.TryGetDataAsDateTime(8)
                };

                items.Add(item);
            }
            if (!rdr.IsClosed) rdr.Close();
        }
    }
}