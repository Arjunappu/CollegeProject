using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using RestaurantBookingSystem.Infrastructure;
using RestaurantBookingSystem.Infrastructure.DataEntities;
using RestaurantBookingSystem.Infrastructure.Providers;
using RestaurantBookingSystem.Helpers;
using RestaurantBookingSystem.Infrastructure.Repositories;

namespace RestaurantBookingSystem.Models
{

    #region Models

    public class ChangePasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [ValidatePasswordLength]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LogOnModel
    {
        [Required]
        [Display(Name = "Email")]
        [Email]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }


    public class RegisterModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        [Email]
        public string Email { get; set; }

        [Digit(ErrorMessage = "A Mobile number can only have numeric values")]
        [Display(Name = "Mobile Number:  +91")]
        [StringLength(10, ErrorMessage = "A Mobile Number Needs to be Exactly 10 digits", MinimumLength = 10)]
        public string MobileNumber { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Your Address")]
        [StringLength(300,MinimumLength = 10, ErrorMessage = "A Valid Address is Expected to be atleast 10 characters")]
        public string Address { get; set; }

        [Required]
        [ValidatePasswordLength]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Secret Question")]
        public string SecretQuestion { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Secret Answer")]
        public string SecretAnswer { get; set; }
    }

    public class RegisterGuest
    {
        [Required(ErrorMessage = "A Name is required")]
        [Display(Name = "Your Name: ")]
        [StringLength(50, MinimumLength = 5,ErrorMessage = "A name should be atleast 5 characters")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "A Mobile number is required")]
        [Digit(ErrorMessage = "A Mobile number can only have numeric values")]
        [Display(Name = "Mobile Number:  +91")]
        [StringLength(10, ErrorMessage = "A Mobile Number Needs to be Exactly 10 digits", MinimumLength = 10)]
        public string MobileNumber { get; set; }
    }
    #endregion

    #region Services
    // The FormsAuthentication type is sealed and contains static members, so it is difficult to
    // unit test code that calls its members. The interface and helper class below demonstrate
    // how to create an abstract wrapper around such a type in order to make the AccountController
    // code unit testable.

    public interface IMembershipService
    {
        int MinPasswordLength { get; }

        bool ValidateUser(string userName, string password);
        MembershipCreateStatus CreateUser(string userName, string password, string email);
        RestaurantUser CreateUser(RestaurantUser user, out MembershipCreateStatus status, string secretQuestion = null, string secretAnswer = null);
        RestaurantUser GetUser(string username, bool isonline);
        RestaurantUser GetUser(Guid userguid, bool isonline);
        IEnumerable<RestaurantUser> GetAllUsers();
        bool DeleteUser(string username);
        void UpdateUser(RestaurantUser user);
        bool ChangePassword(Guid userGuid, string oldPassword, string newPassword);
    }

    public class AccountMembershipService : IMembershipService
    {
        private readonly MembershipProvider _provider;

        public AccountMembershipService()
            : this(null)
        {
        }

        public AccountMembershipService(MembershipProvider provider)
        {
            _provider = provider ?? Membership.Provider;
        }

        public int MinPasswordLength
        {
            get
            {
                return _provider.MinRequiredPasswordLength;
            }
        }

        public bool ValidateUser(string userName, string password)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");

            return _provider.ValidateUser(userName, password);
        }

        [Obsolete("This method is obsolete and should not be used, use CreateUser(RestaurantUser) insted", true)]
        public MembershipCreateStatus CreateUser(string userName, string password, string email)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");
            if (String.IsNullOrEmpty(email)) throw new ArgumentException("Value cannot be null or empty.", "email");

            MembershipCreateStatus status;
            _provider.CreateUser(userName, password, email, null, null, true, null, out status);
            return status;
        }

        public RestaurantUser CreateUser(RestaurantUser user, out MembershipCreateStatus status, string secretQuestion, string secretAnswer)
        {
            var result = (RestaurantUser) null;
            status = MembershipCreateStatus.ProviderError;
            if (_provider is CustomSqlMembershipProviderWrapper)
                result = (_provider as CustomSqlMembershipProviderWrapper).CreateUser(user, out status, secretQuestion, secretAnswer);
            return result;
        }

        public RestaurantUser GetUser(string username, bool isonline)
        {
            if (username.IsNullOrEmpty())
                return null;
            return (RestaurantUser)_provider.GetUser(username, isonline);
        }

        public RestaurantUser GetUser(Guid userguid, bool isonline)
        {
            if (userguid == Guid.Empty)
                return null;
            return (RestaurantUser)_provider.GetUser(userguid, isonline);
        }

        public IEnumerable<RestaurantUser> GetAllUsers()
        {
            int totalrecords;
            var membershipusers = _provider.GetAllUsers(0, Int32.MaxValue, out totalrecords);
            var restaurantusers = new RestaurantUserRepository().SelectAll();
            var result = new List<RestaurantUser>(restaurantusers.Count());
            try
            {
                foreach (var user in restaurantusers)
                {
                    var restaurantuser = RestaurantUserRepository.UserBaseToRestaurantUser(user);
                    var membershipusername = restaurantuser.UserRole == UserBase.RestaurantUserRole.Guest
                                                 ? restaurantuser.UserGuid.ToString()
                                                 : (restaurantuser.FacebookDetail != null &&
                                                    restaurantuser.FacebookDetail.FacebookId > 0 &&
                                                    !restaurantuser.FacebookDetail.OAuthToken.IsNullOrEmpty())
                                                       ? restaurantuser.FacebookDetail.FacebookId.ToString()
                                                       : restaurantuser.EmailId;
                    var membershipuser = membershipusers[membershipusername];
                    if (membershipuser != null)
                    result.Add(new RestaurantUser(restaurantuser.UserId, restaurantuser.Name, restaurantuser.UserGuid,
                                                  restaurantuser.MobileNumber, restaurantuser.LoginExpiresOn,
                                                  restaurantuser.UserRole, restaurantuser.Address, restaurantuser.EmailId,
                                                  restaurantuser.Password, restaurantuser.FacebookDetail,
                                                  membershipuser));
                }

            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            {
                throw;
            } 
            // ReSharper restore EmptyGeneralCatchClause

            return result;
        }

        public bool DeleteUser(string username)
        {
            if (username.IsNullOrEmpty())
                return false;
            return _provider.DeleteUser(username, true);
        }

        public void UpdateUser(RestaurantUser user)
        {
            _provider.UpdateUser(user);
        }

        public bool ChangePassword(Guid userGuid, string oldPassword, string newPassword)
        {
            if (userGuid == Guid.Empty) throw new ArgumentException("Value cannot be null or empty.", "userguid");
            if (String.IsNullOrEmpty(oldPassword)) throw new ArgumentException("Value cannot be null or empty.", "oldPassword");
            if (String.IsNullOrEmpty(newPassword)) throw new ArgumentException("Value cannot be null or empty.", "newPassword");

            // The underlying ChangePassword() will throw an exception rather
            // than return false in certain failure scenarios.
            try
            {
                return _provider.ChangePassword(GetUser(userGuid, false).UserName, oldPassword, newPassword);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (MembershipPasswordException)
            {
                return false;
            }
        }
    }

    public interface IFormsAuthenticationService
    {
        void SignIn(RestaurantUser user, bool createPersistentCookie);
        void SignOut();
    }

    public class FormsAuthenticationService : IFormsAuthenticationService
    {
        public void SignIn(RestaurantUser user, bool createPersistentCookie)
        {
            if (user == null) throw new ArgumentNullException("user", "Value cannot be null or empty");
            FormsAuthenticationHelper.SetAuthCookie(user, createPersistentCookie);
        }

        public void SignOut()
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated && HttpContext.Current.User.Identity is RestaurantUserIdentity)
            {
                var user =
                    new RestaurantUserRepository().Find(
                        ((RestaurantUserIdentity) HttpContext.Current.User.Identity).UserId);
                if (user != null)
                {
                    user.LoginExpiresOn = DateTimeHelper.SqlDbMinDateTime;
                    new RestaurantUserRepository().Update(user);
                }
            }
            FormsAuthentication.SignOut();
        }
    }
    #endregion

    #region Validation
    public static class AccountValidation
    {
        public static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Username or Email provided already exists. Please enter a different user name or Email Id.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A username for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ValidatePasswordLengthAttribute : ValidationAttribute, IClientValidatable
    {
// ReSharper disable InconsistentNaming
        private const string _defaultErrorMessage = "'{0}' must be at least {1} characters long.";
// ReSharper restore InconsistentNaming
        private readonly int _minCharacters = Membership.Provider.MinRequiredPasswordLength;

        public ValidatePasswordLengthAttribute()
            : base(_defaultErrorMessage)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture, ErrorMessageString,
                name, _minCharacters);
        }

        public override bool IsValid(object value)
        {
            var valueAsString = value as string;
            return (valueAsString != null && valueAsString.Length >= _minCharacters);
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            return new[]{
                new ModelClientValidationStringLengthRule(FormatErrorMessage(metadata.GetDisplayName()), _minCharacters, int.MaxValue)
            };
        }
    }
    #endregion

}
