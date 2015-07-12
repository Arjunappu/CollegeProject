using System;
using System.Configuration;
using System.Web.Profile;
using RestaurantBookingSystem.Infrastructure.Repositories;
using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Infrastructure.Providers
{
    // ReSharper disable RedundantOverridenMember

    public class CustomSqlProfileProviderWrapper : SqlProfileProvider
    {
        private readonly RestaurantUserRepository UserRepository;
        private readonly FacebookUserDetailRepository FacebookDetailRepository;

        public CustomSqlProfileProviderWrapper()
        {
            UserRepository = new RestaurantUserRepository();
            FacebookDetailRepository = new FacebookUserDetailRepository();
        }

        #region Overrides of SettingsProvider

        /// <summary>
        /// Returns the collection of settings property values for the specified application instance and settings property group.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Configuration.SettingsPropertyValueCollection"/> containing the values for the specified settings property group.
        /// </returns>
        /// <param name="context">A <see cref="T:System.Configuration.SettingsContext"/> describing the current application use.</param><param name="collection">A <see cref="T:System.Configuration.SettingsPropertyCollection"/> containing the settings property group whose values are to be retrieved.</param><filterpriority>2</filterpriority>
        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
        {
            return base.GetPropertyValues(context, collection);
        }

        /// <summary>
        /// Sets the values of the specified group of property settings.
        /// </summary>
        /// <param name="context">A <see cref="T:System.Configuration.SettingsContext"/> describing the current application usage.</param><param name="collection">A <see cref="T:System.Configuration.SettingsPropertyValueCollection"/> representing the group of property settings to set.</param><filterpriority>2</filterpriority>
        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            base.SetPropertyValues(context, collection);
        }

        #endregion

        #region Overrides of ProfileProvider


        /// <summary>
        /// Deletes profile properties and information for the supplied list of profiles.
        /// </summary>
        /// <returns>
        /// The number of profiles deleted from the data source.
        /// </returns>
        /// <param name="profiles">A <see cref="T:System.Web.Profile.ProfileInfoCollection"/>  of information about profiles that are to be deleted.</param>
        public override int DeleteProfiles(ProfileInfoCollection profiles)
        {
            return base.DeleteProfiles(profiles);
        }

        /// <summary>
        /// Deletes profile properties and information for profiles that match the supplied list of user names.
        /// </summary>
        /// <returns>
        /// The number of profiles deleted from the data source.
        /// </returns>
        /// <param name="usernames">A string array of user names for profiles to be deleted.</param>
        public override int DeleteProfiles(string[] usernames)
        {
            return base.DeleteProfiles(usernames);
        }

        /// <summary>
        /// Deletes all user-profile data for profiles in which the last activity date occurred before the specified date.
        /// </summary>
        /// <returns>
        /// The number of profiles deleted from the data source.
        /// </returns>
        /// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are deleted.</param><param name="userInactiveSinceDate">A <see cref="T:System.DateTime"/> that identifies which user profiles are considered inactive. If the <see cref="P:System.Web.Profile.ProfileInfo.LastActivityDate"/>  value of a user profile occurs on or before this date and time, the profile is considered inactive.</param>
        public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            return base.DeleteInactiveProfiles(authenticationOption, userInactiveSinceDate);
        }

        /// <summary>
        /// Returns the number of profiles in which the last activity date occurred on or before the specified date.
        /// </summary>
        /// <returns>
        /// The number of profiles in which the last activity date occurred on or before the specified date.
        /// </returns>
        /// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are returned.</param><param name="userInactiveSinceDate">A <see cref="T:System.DateTime"/> that identifies which user profiles are considered inactive. If the <see cref="P:System.Web.Profile.ProfileInfo.LastActivityDate"/>  of a user profile occurs on or before this date and time, the profile is considered inactive.</param>
        public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            return base.GetNumberOfInactiveProfiles(authenticationOption, userInactiveSinceDate);
        }

        /// <summary>
        /// Retrieves user profile data for all profiles in the data source.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Profile.ProfileInfoCollection"/> containing user-profile information for all profiles in the data source.
        /// </returns>
        /// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are returned.</param><param name="pageIndex">The index of the page of results to return.</param><param name="pageSize">The size of the page of results to return.</param><param name="totalRecords">When this method returns, contains the total number of profiles.</param>
        public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
        {
            return base.GetAllProfiles(authenticationOption, pageIndex, pageSize, out totalRecords);
        }

        /// <summary>
        /// Retrieves user-profile data from the data source for profiles in which the last activity date occurred on or before the specified date.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Profile.ProfileInfoCollection"/> containing user-profile information about the inactive profiles.
        /// </returns>
        /// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are returned.</param><param name="userInactiveSinceDate">A <see cref="T:System.DateTime"/> that identifies which user profiles are considered inactive. If the <see cref="P:System.Web.Profile.ProfileInfo.LastActivityDate"/>  of a user profile occurs on or before this date and time, the profile is considered inactive.</param><param name="pageIndex">The index of the page of results to return.</param><param name="pageSize">The size of the page of results to return.</param><param name="totalRecords">When this method returns, contains the total number of profiles.</param>
        public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            return base.GetAllInactiveProfiles(authenticationOption, userInactiveSinceDate, pageIndex, pageSize, out totalRecords);
        }

        /// <summary>
        /// Retrieves profile information for profiles in which the user name matches the specified user names.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Profile.ProfileInfoCollection"/> containing user-profile information for profiles where the user name matches the supplied <paramref name="usernameToMatch"/> parameter.
        /// </returns>
        /// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are returned.</param><param name="usernameToMatch">The user name to search for.</param><param name="pageIndex">The index of the page of results to return.</param><param name="pageSize">The size of the page of results to return.</param><param name="totalRecords">When this method returns, contains the total number of profiles.</param>
        public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return base.FindProfilesByUserName(authenticationOption, usernameToMatch, pageIndex, pageSize,
                                               out totalRecords);
        }

        /// <summary>
        /// Retrieves profile information for profiles in which the last activity date occurred on or before the specified date and the user name matches the specified user name.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Profile.ProfileInfoCollection"/> containing user profile information for inactive profiles where the user name matches the supplied <paramref name="usernameToMatch"/> parameter.
        /// </returns>
        /// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are returned.</param><param name="usernameToMatch">The user name to search for.</param><param name="userInactiveSinceDate">A <see cref="T:System.DateTime"/> that identifies which user profiles are considered inactive. If the <see cref="P:System.Web.Profile.ProfileInfo.LastActivityDate"/> value of a user profile occurs on or before this date and time, the profile is considered inactive.</param><param name="pageIndex">The index of the page of results to return.</param><param name="pageSize">The size of the page of results to return.</param><param name="totalRecords">When this method returns, contains the total number of profiles.</param>
        public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            return base.FindInactiveProfilesByUserName(authenticationOption, usernameToMatch, userInactiveSinceDate,
                                                       pageIndex, pageSize, out totalRecords);
        }

        #endregion
    }

    // ReSharper restore RedundantOverridenMember
}