using System;
using System.Web.Security;
using RestaurantBookingSystem.Helpers;

namespace RestaurantBookingSystem.Infrastructure.DataEntities
{
    /// <summary>
    /// A class to represent a Booking Customer, Inherits Membership User
    /// </summary>
    /// <remarks>
    /// This class represents a User with some minimal details for the refrence of the Booking Details
    /// </remarks>
    public class UserBase : MembershipUser
    {
        /// <summary>
        /// Gets the User's Id
        /// </summary>
        public int UserId { get; private set; }

        private Guid _userguid;
        /// <summary>
        /// Gets or Sets the Unique Identifier for the User
        /// </summary>
        public Guid UserGuid { 
            get
            {
                if(base.ProviderUserKey != null && base.ProviderUserKey is Guid)
                {
                    return (Guid)base.ProviderUserKey;
                }
                return _userguid;
            }
            private set 
            {
                _userguid = value;
            }
        }

        /// <summary>
        /// Gets or Sets the Customer's Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets the mobile number of the customer
        /// </summary>
        public ulong MobileNumber { get; set; }

        private DateTime _loginExpiresOn;

        /// <summary>
        /// Ges or Sets the Login Expire Time of the User
        /// </summary>
        public DateTime LoginExpiresOn
        {
            get { return _loginExpiresOn.ToLocalTime(); }
            set { _loginExpiresOn = value.ToUniversalTime(); }
        }

        /// <summary>
        /// Gets the User's Role
        /// </summary>
        public RestaurantUserRole UserRole { get; set; }

        /// <summary>
        /// A constructor to Initialize a new User with given User Name
        /// </summary>
        public UserBase(string name)
            : this(0, name)
        {
        }

        /// <summary>
        /// A constructor to Initialize a new User with a given UserId and Name
        /// </summary>
        /// <param name="userid">The User's Id</param>
        /// <param name="name">The User's Name</param>
        /// <exception cref="ArgumentNullException"></exception>
        public UserBase(int userid, string name)
            : this(userid,name, Guid.Empty)
        {
        }

        /// <summary>
        /// A constructor to Initialize a new User with a given UserId and Name
        /// </summary>
        /// <param name="userid">The User's Id</param>
        /// <param name="name">The User's Name</param>
        /// <param name="userguid">The unique Id of the user</param>
        /// <exception cref="ArgumentNullException"></exception>
        public UserBase(int userid, string name, Guid userguid)
            : base(Membership.Provider.Name, name, userguid, null, null, null, true, false, DateTimeHelper.SqlDbMinDateTime, DateTimeHelper.SqlDbMinDateTime, DateTimeHelper.SqlDbMinDateTime, DateTimeHelper.SqlDbMinDateTime, DateTimeHelper.SqlDbMinDateTime)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name", "The Name can not be null or Empty String");
            }
            UserId = userid;
            Name = name;
            LoginExpiresOn = DateTimeHelper.SqlDbMinDateTime;
            UserRole = RestaurantUserRole.Guest;
            UserGuid = userguid;
        }

        /// <summary>
        /// A constructor to Initialize a new User with a given UserId and Name
        /// </summary>
        /// <param name="userid">The User's Id</param>
        /// <param name="name">The User's Name</param>
        /// <param name="userguid">The unique Id of the user</param>
        /// <param name="mobileNumber">The Mobile number of the user</param>
        /// <param name="loginexpireson">Login Expiry DateTime</param>
        /// <param name="role">The role ofthe user</param>
        /// <param name="membershipuser">The membershipuser instance that will ba passed on to base class</param>
        /// <exception cref="ArgumentNullException"></exception>
        public UserBase(int userid, string name, Guid userguid, ulong mobileNumber, DateTime loginexpireson,
                        RestaurantUserRole role, MembershipUser membershipuser)
            : base(
                Membership.Provider.Name, membershipuser.UserName, userguid, membershipuser.Email,
                membershipuser.PasswordQuestion, membershipuser.Comment, membershipuser.IsApproved,
                membershipuser.IsLockedOut, membershipuser.CreationDate, membershipuser.LastLoginDate,
                membershipuser.LastActivityDate, membershipuser.LastPasswordChangedDate, membershipuser.LastLockoutDate)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name", "The Name can not be null or Empty String");
            }
            UserId = userid;
            Name = name;
            LoginExpiresOn = DateTimeHelper.SqlDbMinDateTime;
            UserRole = RestaurantUserRole.Guest;
            UserGuid = userguid;
            MobileNumber = mobileNumber;
            LoginExpiresOn = loginexpireson;
            UserRole = role;
        }

        /// <summary>
        /// Enuumeration to define User's Role
        /// </summary>
        public enum RestaurantUserRole
        {
            /// <summary>
            /// A Guest User
            /// </summary>
            Guest,

            /// <summary>
            /// A Restaurant's Customer
            /// </summary>
            Customer,

            /// <summary>
            /// A Restaurant's Employee
            /// </summary>
            Employee,

            /// <summary>
            /// The Admin for this Website
            /// </summary>
            Admin
        }
    }

    /// <summary>
    /// A class to represent a Restaurant User
    /// </summary>
    /// <remarks>
    /// This class can be used for both a Restaurant Customer, Employee or Administrator
    /// </remarks>
    public class RestaurantUser : UserBase
    {
        /// <summary>
        /// Gets or Sets the Address of the User
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Gets or Sets the Email Id of the User
        /// </summary>
        public string EmailId
        {
            get { return base.Email; }
            set { base.Email = value; }
        }

        /// <summary>
        /// Gets or Sets the Password for the User
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or Sets the User's Facebook Id 
        /// </summary>
        public FacebookUserDetail FacebookDetail { get; set; }

        /// <summary>
        /// A constructor to Initialize a new User with a given Name
        /// </summary>
        public RestaurantUser(string name)
            : base(name)
        {
        }

        /// <summary>
        /// A constructor to Initialize a user with a given User Id and Name
        /// </summary>
        /// <param name="userid">The User's Id</param>
        /// <param name="name">The User's Name</param>
        public RestaurantUser(int userid, string name)
            : base(userid, name)
        {
        }

        /// <summary>
        /// A constructor to Initialize a user with a given User Id, Name and UserGuid
        /// </summary>
        /// <param name="userid">The User's Id</param>
        /// <param name="name">The User's Name</param>
        /// <param name="userguid">The Unique id of the User</param>
        public RestaurantUser(int userid, string name, Guid userguid)
            : base(userid, name, userguid)
        {
        }

        /// <summary>
        /// A constructor to Initialize a user with a given Restaurant Detail and Details for al base classes
        /// </summary>
        /// <param name="userid">The User's Id</param>
        /// <param name="name">The User's Name</param>
        /// <param name="userguid">The Unique id of the User</param>
        /// <param name="mobileNumber">The Mobile number of the user</param>
        /// <param name="loginexpireson">Login Expiry DateTime</param>
        /// <param name="role">The role ofthe user</param>
        /// <param name="address">The user's Address</param>
        /// <param name="emailid">The user's email id</param>
        /// <param name="password">The user's password as string (This has become obsolete and should not be used)</param>
        /// <param name="facebookdetail">The user's facebook detail</param>
        /// <param name="membershipuser">The membershipuser instance that will ba passed on to base class</param>
        public RestaurantUser(int userid, string name, Guid userguid, ulong mobileNumber, DateTime loginexpireson,
                              RestaurantUserRole role, string address, string emailid, string password,
                              FacebookUserDetail facebookdetail, MembershipUser membershipuser)
            : base(userid, name, userguid, mobileNumber, loginexpireson,role,membershipuser)
        {
            Address = address;
            EmailId = emailid;
            Password = password;
            FacebookDetail = facebookdetail;
        }
    }
}