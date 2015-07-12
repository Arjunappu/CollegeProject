using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using RestaurantBookingSystem.Helpers;
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace RestaurantBookingSystem.Models
{
    public class RegisterEmployeeModel
    {
        [Required]
        [UIHint("JuiRadios")]
        [Display(Name = "Employee Role")]
        public EmployeeUserRole UserRole { get; set; }

        [Required]
        [Display(Name = "Employee name")]
        public string EmployeeName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Id")]
        [Email]
        public string Email { get; set; }

        [Digit(ErrorMessage = "A Mobile number can only have numeric values")]
        [Display(Name = "Mobile Number:  +91")]
        [StringLength(10, ErrorMessage = "A Mobile Number Needs to be Exactly 10 digits", MinimumLength = 10)]
        public string MobileNumber { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Your Address")]
        [StringLength(300, MinimumLength = 10, ErrorMessage = "A Valid Address is Expected to be atleast 10 characters")]
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

    public class UpdateEmployeeModel
    {
        [Required]
        [UIHint("JuiRadios")]
        [Display(Name = "Employee Role")]
        public EmployeeUserRole UserRole { get; set; }

        [Required]
        [Display(Name = "Employee name")]
        public string EmployeeName { get; set; }

        [Digit(ErrorMessage = "A Mobile number can only have numeric values")]
        [Display(Name = "Mobile Number:  +91")]
        [StringLength(10, ErrorMessage = "A Mobile Number Needs to be Exactly 10 digits", MinimumLength = 10)]
        public string MobileNumber { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Your Address")]
        [StringLength(300, MinimumLength = 10, ErrorMessage = "A Valid Address is Expected to be atleast 10 characters")]
        public string Address { get; set; }

        [Required]
        [HiddenInput(DisplayValue = false)]
        public Guid UserGuid { get; set; }
    }

    public enum EmployeeUserRole
    {
        Employee = 2,
        Admin = 3
    }
}
// ReSharper restore UnusedAutoPropertyAccessor.Global