using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Security;
using RestaurantBookingSystem.Helpers;
using RestaurantBookingSystem.Infrastructure.DataEntities;
using RestaurantBookingSystem.Infrastructure.Repositories;
using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Infrastructure.Providers
{
    // ReSharper disable RedundantOverridenMember
    //TODO: This is a crude way to handle custom database, will definately change it later
    /// <summary>
    /// A custom Membership Provider which wraps around SqlMembershipProvider to intercept its method calls and Perform additional Database tasks (Make sure RestaurantMembershipProperty is not null toherwise it will throw exception)
    /// </summary>
    public class CustomSqlMembershipProviderWrapper : SqlMembershipProvider
    {
        private readonly RestaurantUserRepository UserRepository;

        /// <summary>
        /// A property to hold a RestaurantUser with detail information
        /// </summary>
        public RestaurantUser RestaurantMembershipUser { get; set; }

        public CustomSqlMembershipProviderWrapper()
        {
            UserRepository = new RestaurantUserRepository();
        }

        public CustomSqlMembershipProviderWrapper(RestaurantUser restaurantUser)
        {
            UserRepository = new RestaurantUserRepository();
            RestaurantMembershipUser = restaurantUser;
        }

        public virtual RestaurantUser CreateUser(RestaurantUser user, out MembershipCreateStatus status, string secretQuestion, string secretAnswer)
        {
            //users will be stored as GUID as their providerkey in Membership database and with 
            //Real name in Restaurant Database
            RestaurantMembershipUser = user;
            CheckRestaurantMembershipUser();
            MembershipUser sqlresult;
            var repoIdresult = -1;
            // Guest user will have their GUID as their username and Password because Guest user can never log in, 
            // he is automatically logged in through his authentication ticket, so this will always be an internal call
            // Also since all user needs to have a valid email id except Guest user, we are using guid as dummy email
            if (user.UserRole == UserBase.RestaurantUserRole.Guest)
            {
                sqlresult = base.CreateUser(user.UserGuid.ToString(), user.UserGuid.ToString(), user.UserGuid.ToString() + "@guestuser.com", null, null,
                                true, user.UserGuid, out status);
                if (status == MembershipCreateStatus.Success)
                {
                    //Login Expiration for a Guset user is set to 86400 Minutes, ie 60 days
                    user.LoginExpiresOn = DateTime.UtcNow.AddMinutes(86400D);
                    user.Password = user.UserGuid.ToString();
                    repoIdresult = UserRepository.Add(user);
                }
            }
            //Facebook user will have their id as username and UserGuid as password
            else if (user.FacebookDetail != null && user.FacebookDetail.FacebookId > 0 && !user.FacebookDetail.OAuthToken.IsNullOrEmpty())
            {
                sqlresult = base.CreateUser(user.FacebookDetail.FacebookId.ToString(), user.UserGuid.ToString(),
                                            user.EmailId, null, null, true, user.UserGuid, out status);
                if (status == MembershipCreateStatus.Success)
                {
                    user.Password = user.UserGuid.ToString();
                    user.LoginExpiresOn = user.FacebookDetail.ExpiresOn;
                    repoIdresult = UserRepository.Add(user);
                }
            }
            //For every one else the user creation is normal
            else
            {
                sqlresult = base.CreateUser(user.EmailId, user.Password, user.EmailId,
                                            secretQuestion, secretAnswer, true, user.UserGuid, out status);
                if (status == MembershipCreateStatus.Success)
                {
                    user.LoginExpiresOn = DateTime.UtcNow.AddMinutes(FormsAuthentication.Timeout.TotalMinutes);
                    repoIdresult = UserRepository.Add(user);
                }
            }
            if (status == MembershipCreateStatus.Success && sqlresult != null && repoIdresult > 0)
            {
                return new RestaurantUser(repoIdresult, user.Name, user.UserGuid, user.MobileNumber, user.LoginExpiresOn,
                                          user.UserRole, user.Address, user.EmailId, user.Password, user.FacebookDetail,
                                          sqlresult);
            }
            return null;
        }

        #region Overrides of MembershipProvider

        /// <summary>
        /// Adds a new membership user to the data source.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"/> object populated with the information for the newly created user.
        /// </returns>
        /// <param name="username">The user name for the new user. </param><param name="password">The password for the new user. </param><param name="email">The e-mail address for the new user.</param><param name="passwordQuestion">The password question for the new user.</param><param name="passwordAnswer">The password answer for the new user</param><param name="isApproved">Whether or not the new user is approved to be validated.</param><param name="providerUserKey">The unique identifier from the membership data source for the user.</param><param name="status">A <see cref="T:System.Web.Security.MembershipCreateStatus"/> enumeration value indicating whether the user was created successfully.</param>
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            CheckRestaurantMembershipUser();
            return CreateUser(RestaurantMembershipUser, out status, null, null);
        }

        /// <summary>
        /// Processes a request to update the password question and answer for a membership user.
        /// </summary>
        /// <returns>
        /// true if the password question and answer are updated successfully; otherwise, false.
        /// </returns>
        /// <param name="username">The user to change the password question and answer for. </param><param name="password">The password for the specified user. </param><param name="newPasswordQuestion">The new password question for the specified user. </param><param name="newPasswordAnswer">The new password answer for the specified user. </param>
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            CheckRestaurantMembershipUser();
            return base.ChangePasswordQuestionAndAnswer(username, password, newPasswordQuestion, newPasswordAnswer);
        }

        /// <summary>
        /// Gets the password for the specified user name from the data source.
        /// </summary>
        /// <returns>
        /// The password for the specified user name.
        /// </returns>
        /// <param name="username">The user to retrieve the password for. </param><param name="answer">The password answer for the user. </param>
        public override string GetPassword(string username, string answer)
        {

            return base.GetPassword(username, answer);
        }

        /// <summary>
        /// Processes a request to update the password for a membership user.
        /// </summary>
        /// <returns>
        /// true if the password was updated successfully; otherwise, false.
        /// </returns>
        /// <param name="username">The user to update the password for. </param><param name="oldPassword">The current password for the specified user. </param><param name="newPassword">The new password for the specified user. </param>
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            var currentUser = (RestaurantUser)GetUser(username, true /* userIsOnline */);
            if (currentUser == null || oldPassword == null) return false;
            var basesuccess =  base.ChangePassword(username, oldPassword, newPassword)
                && RestaurantUserRepository.ValidatePasswordWithHash(oldPassword.Trim(), currentUser.Password);
            if (basesuccess)
            {
                currentUser.Password = newPassword;
                basesuccess = UserRepository.Update(currentUser);
            }
            return basesuccess;
        }

        /// <summary>
        /// Resets a user's password to a new, automatically generated password.
        /// </summary>
        /// <returns>
        /// The new password for the specified user.
        /// </returns>
        /// <param name="username">The user to reset the password for. </param><param name="answer">The password answer for the specified user. </param>
        public override string ResetPassword(string username, string answer)
        {
            var currentuser = (RestaurantUser)GetUser(username, true);
            if (currentuser == null)
                throw new InvalidOperationException(String.Format("User with UserName {0} does not exist", username));
            var result = base.ResetPassword(username, answer);
            currentuser.Password = result;
            UserRepository.Update(currentuser);
            return result;
        }

        /// <summary>
        /// Updates information about a user in the data source.
        /// </summary>
        /// <param name="user">A <see cref="T:System.Web.Security.MembershipUser"/> object that represents the user to update and the updated information for the user. </param>
        public override void UpdateUser(MembershipUser user)
        {
            base.UpdateUser(user);
            if (user is RestaurantUser)
                UserRepository.Update(user as RestaurantUser);
        }

        /// <summary>
        /// Verifies that the specified user name and password exist in the data source.
        /// </summary>
        /// <returns>
        /// true if the specified username and password are valid; otherwise, false.
        /// </returns>
        /// <param name="username">The name of the user to validate. </param><param name="password">The password for the specified user. </param>
        public override bool ValidateUser(string username, string password)
        {
            var currentuser = (RestaurantUser) GetUser(username, true);
            return currentuser != null && base.ValidateUser(username, password) &&
                   RestaurantUserRepository.ValidatePasswordWithHash(password, currentuser.Password);
        }

        /// <summary>
        /// Clears a lock so that the membership user can be validated.
        /// </summary>
        /// <returns>
        /// true if the membership user was successfully unlocked; otherwise, false.
        /// </returns>
        /// <param name="userName">The membership user whose lock status you want to clear.</param>
        public override bool UnlockUser(string userName)
        {
            return base.UnlockUser(userName);
        }

        /// <summary>
        /// Gets user information from the data source based on the unique identifier for the membership user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"/> object populated with the specified user's information from the data source.
        /// </returns>
        /// <param name="providerUserKey">The unique identifier for the membership user to get information for.</param><param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            var restaurantresult = (RestaurantUser) null;
            var sqlresult = (MembershipUser) null;

            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            if (providerUserKey != null && (providerUserKey is Guid))
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
            {
                sqlresult = base.GetUser(providerUserKey, userIsOnline);
                if (sqlresult != null)
                    restaurantresult =
                        RestaurantUserRepository.UserBaseToRestaurantUser(UserRepository.Find((Guid) providerUserKey));
            }

            if (restaurantresult != null)
                restaurantresult = CombineResult(restaurantresult, sqlresult);

            return restaurantresult;
        }

        /// <summary>
        /// Gets information from the data source for a user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"/> object populated with the specified user's information from the data source.
        /// </returns>
        /// <param name="username">The name of the user to get information for. </param><param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user. </param>
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            var restaurantresult = (RestaurantUser)null;
            var sqlresult = (MembershipUser)null;

            if (username != null)
            {
                sqlresult = base.GetUser(username, userIsOnline);
                if (sqlresult != null && sqlresult.ProviderUserKey != null && sqlresult.ProviderUserKey is Guid)
                    restaurantresult =
                        RestaurantUserRepository.UserBaseToRestaurantUser(UserRepository.Find((Guid)sqlresult.ProviderUserKey));
            }

            if (restaurantresult != null)
                restaurantresult = CombineResult(restaurantresult, sqlresult);

            return restaurantresult;
        }

        /// <summary>
        /// Gets the user name associated with the specified e-mail address.
        /// </summary>
        /// <returns>
        /// The user name associated with the specified e-mail address. If no match is found, return null.
        /// </returns>
        /// <param name="email">The e-mail address to search for. </param>
        public override string GetUserNameByEmail(string email)
        {
            return base.GetUserNameByEmail(email);
        }

        /// <summary>
        /// Removes a user from the membership data source. 
        /// </summary>
        /// <returns>
        /// true if the user was successfully deleted; otherwise, false.
        /// </returns>
        /// <param name="username">The name of the user to delete.</param><param name="deleteAllRelatedData">true to delete data related to the user from the database; false to leave data related to the user in the database.</param>
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            bool successflag = false;
            if (deleteAllRelatedData)
            {
                var user  = (RestaurantUser)GetUser(username, true);
                if (user != null)
                    successflag = UserRepository.Delete(user.UserId);
            }
            successflag = (successflag == deleteAllRelatedData) & base.DeleteUser(username, deleteAllRelatedData);
            return successflag;
        }

        /// <summary>
        /// Gets a collection of all the users in the data source in pages of data.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"/> collection that contains a page of <paramref name="pageSize"/><see cref="T:System.Web.Security.MembershipUser"/> objects beginning at the page specified by <paramref name="pageIndex"/>.
        /// </returns>
        /// <param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex"/> is zero-based.</param><param name="pageSize">The size of the page of results to return.</param><param name="totalRecords">The total number of matched users.</param>
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            return base.GetAllUsers(pageIndex, pageSize, out totalRecords);
        }

        /// <summary>
        /// Gets the number of users currently accessing the application.
        /// </summary>
        /// <returns>
        /// The number of users currently accessing the application.
        /// </returns>
        public override int GetNumberOfUsersOnline()
        {
            return base.GetNumberOfUsersOnline();
        }

        /// <summary>
        /// Gets a collection of membership users where the user name contains the specified user name to match.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"/> collection that contains a page of <paramref name="pageSize"/><see cref="T:System.Web.Security.MembershipUser"/> objects beginning at the page specified by <paramref name="pageIndex"/>.
        /// </returns>
        /// <param name="usernameToMatch">The user name to search for.</param><param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex"/> is zero-based.</param><param name="pageSize">The size of the page of results to return.</param><param name="totalRecords">The total number of matched users.</param>
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return base.FindUsersByName(usernameToMatch, pageIndex, pageSize, out totalRecords);
        }

        /// <summary>
        /// Gets a collection of membership users where the e-mail address contains the specified e-mail address to match.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"/> collection that contains a page of <paramref name="pageSize"/><see cref="T:System.Web.Security.MembershipUser"/> objects beginning at the page specified by <paramref name="pageIndex"/>.
        /// </returns>
        /// <param name="emailToMatch">The e-mail address to search for.</param><param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex"/> is zero-based.</param><param name="pageSize">The size of the page of results to return.</param><param name="totalRecords">The total number of matched users.</param>
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return base.FindUsersByEmail(emailToMatch, pageIndex, pageSize, out totalRecords);
        }

        #endregion

        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
                throw new ArgumentNullException("config");
            if (string.IsNullOrEmpty(name))
                name = "CustomSqlMembershipProviderWrapper";

            base.Initialize(name, config);
        }

        #region Overrides of Membership Configurations

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to retrieve their passwords.
        /// </summary>
        /// <returns>
        /// true if the membership provider is configured to support password retrieval; otherwise, false. The default is false.
        /// </returns>
        public override bool EnablePasswordRetrieval
        {
            get { return base.EnablePasswordRetrieval; }
        }

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to reset their passwords.
        /// </summary>
        /// <returns>
        /// true if the membership provider supports password reset; otherwise, false. The default is true.
        /// </returns>
        public override bool EnablePasswordReset
        {
            get { return base.EnablePasswordReset; }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require the user to answer a password question for password reset and retrieval.
        /// </summary>
        /// <returns>
        /// true if a password answer is required for password reset and retrieval; otherwise, false. The default is true.
        /// </returns>
        public override bool RequiresQuestionAndAnswer
        {
            get { return base.RequiresQuestionAndAnswer; }
        }

        /// <summary>
        /// The name of the application using the custom membership provider.
        /// </summary>
        /// <returns>
        /// The name of the application using the custom membership provider.
        /// </returns>
        public override string ApplicationName
        {
            get { return base.ApplicationName; }
            set { base.ApplicationName = value; }
        }

        /// <summary>
        /// Gets the number of invalid password or password-answer attempts allowed before the membership user is locked out.
        /// </summary>
        /// <returns>
        /// The number of invalid password or password-answer attempts allowed before the membership user is locked out.
        /// </returns>
        public override int MaxInvalidPasswordAttempts
        {
            get { return base.MaxInvalidPasswordAttempts; }
        }

        /// <summary>
        /// Gets the number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        /// </summary>
        /// <returns>
        /// The number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        /// </returns>
        public override int PasswordAttemptWindow
        {
            get { return base.PasswordAttemptWindow; }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require a unique e-mail address for each user name.
        /// </summary>
        /// <returns>
        /// true if the membership provider requires a unique e-mail address; otherwise, false. The default is true.
        /// </returns>
        public override bool RequiresUniqueEmail
        {
            get { return base.RequiresUniqueEmail; }
        }

        /// <summary>
        /// Gets a value indicating the format for storing passwords in the membership data store.
        /// </summary>
        /// <returns>
        /// One of the <see cref="T:System.Web.Security.MembershipPasswordFormat"/> values indicating the format for storing passwords in the data store.
        /// </returns>
        public override MembershipPasswordFormat PasswordFormat
        {
            get { return base.PasswordFormat; }
        }

        /// <summary>
        /// Gets the minimum length required for a password.
        /// </summary>
        /// <returns>
        /// The minimum length required for a password. 
        /// </returns>
        public override int MinRequiredPasswordLength
        {
            get { return base.MinRequiredPasswordLength; }
        }

        /// <summary>
        /// Gets the minimum number of special characters that must be present in a valid password.
        /// </summary>
        /// <returns>
        /// The minimum number of special characters that must be present in a valid password.
        /// </returns>
        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return base.MinRequiredNonAlphanumericCharacters; }
        }

        /// <summary>
        /// Gets the regular expression used to evaluate a password.
        /// </summary>
        /// <returns>
        /// A regular expression used to evaluate a password.
        /// </returns>
        public override string PasswordStrengthRegularExpression
        {
            get { return base.PasswordStrengthRegularExpression; }
        }

        #endregion

        private void CheckRestaurantMembershipUser()
        {
            if (RestaurantMembershipUser == null || RestaurantMembershipUser.Name.IsNullOrEmpty())
                throw new InvalidOperationException("Cannot create a new user with null RestaurantMembershipUser Property");
            if (RestaurantMembershipUser.UserGuid == Guid.Empty)
                throw new InvalidOperationException("Cannot create a new user with an Empty Unique Id");
        }

        private static RestaurantUser CombineResult(RestaurantUser restaurantresult, MembershipUser sqlresult)
        {
            var result = (RestaurantUser) null;
            if (restaurantresult != null && sqlresult != null)
                result = new RestaurantUser(restaurantresult.UserId,
                    restaurantresult.Name,
                    restaurantresult.UserGuid,
                    restaurantresult.MobileNumber,
                    restaurantresult.LoginExpiresOn,
                    restaurantresult.UserRole,
                    restaurantresult.Address,
                    restaurantresult.EmailId,
                    restaurantresult.Password,
                    restaurantresult.FacebookDetail,
                    sqlresult
                    );
            return result;
        }
    }

    // ReSharper restore RedundantOverridenMember
}