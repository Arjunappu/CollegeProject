using System;
using System.Security.Principal;
using RestaurantBookingSystem.Helpers;
using RestaurantBookingSystem.Infrastructure.DataEntities;
using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Infrastructure
{
    public class RestaurantUserIdentity: IIdentity
    {
        #region Implementation of IIdentity

        /// <summary>
        /// Gets the name of the current user.
        /// </summary>
        /// <returns>
        /// The name of the user on whose behalf the code is running.
        /// </returns>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the type of authentication used.
        /// </summary>
        /// <returns>
        /// The type of authentication used to identify the user.
        /// </returns>
        public string AuthenticationType { get; private set; }

        /// <summary>
        /// Gets a value that indicates whether the user has been authenticated.
        /// </summary>
        /// <returns>
        /// true if the user was authenticated; otherwise, false.
        /// </returns>
        public bool IsAuthenticated { get; private set; }

        #endregion

        #region IIdentity Extention for RestaurantUser

        //Default Identiy used be Web Applications does not have a Place for UserId and Friendlyname

        /// <summary>
        /// Gets the Id of the Current User
        /// </summary>
        /// <returns>
        /// Returns the integer Id of the Current User
        /// </returns>
        public int UserId { get; private set; }

        /// <summary>
        /// Gets the Unique Id for the Current User
        /// </summary>
        /// <returns>
        /// Returns the Unique Guid of the Current User
        /// </returns>
        public Guid UserGuid { get; private set; }

        /// <summary>
        /// Gets the Real Friendly Name for the Current User
        /// </summary>
        /// <returns>
        /// Returns the <see cref="UserBase.Name"/> of the Current User as string
        /// </returns>
        public string FriendlyName { get; private set; }

        #endregion

        /// <summary>
        /// Gets the AuthenticationType string as a <see cref="FormsAuthenticationHelper.AuthenticationType"/>
        /// </summary>
        /// <returns>
        /// Returns the AuthenticationType string as a <see cref="FormsAuthenticationHelper.AuthenticationType"/>
        /// </returns>
        public FormsAuthenticationHelper.AuthenticationType GetAuthenticationType
        {
            get
            {
                FormsAuthenticationHelper.AuthenticationType result;
                Enum.TryParse(AuthenticationType, true, out result);
                return result;
            }
        }

        /// <summary>
        /// Initializes a new Identity with basic Identity information
        /// </summary>
        /// <param name="name">The string Name of the user</param>
        /// <param name="authenticationtype">The AuthenticationType of the User's Authentication</param>
        /// <param name="isauthentiacted">Boolean value indicating Is user Authenticated</param>
        public RestaurantUserIdentity(string name, FormsAuthenticationHelper.AuthenticationType authenticationtype, bool isauthentiacted)
            : this(name, authenticationtype, isauthentiacted, 0, null, Guid.Empty)
        {
        }

        /// <summary>
        /// Initializes a new RestaurantUserIdentity with specified Information
        /// </summary>
        /// <param name="name">The string Name of the user</param>
        /// <param name="authenticationtype">The AuthenticationType of the User's Authentication</param>
        /// <param name="isauthentiacted">Boolean value indicating Is user Authenticated</param>
        /// <param name="userid">The integer Id of the User</param>
        /// <param name="friendlyname">The string Friendly name of the user</param>
        /// <param name="userguid">The Unique Guid of the user </param>
        public RestaurantUserIdentity(string name, FormsAuthenticationHelper.AuthenticationType authenticationtype, bool isauthentiacted, int userid, string friendlyname, Guid userguid)
        {
            Name = name;
            AuthenticationType = authenticationtype.ToString();
            IsAuthenticated = isauthentiacted;
            UserId = userid;
            FriendlyName = friendlyname;
            UserGuid = userguid;
        }
    }
}