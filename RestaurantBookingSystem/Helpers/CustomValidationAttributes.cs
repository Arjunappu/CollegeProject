using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace RestaurantBookingSystem.Helpers
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,AllowMultiple = false)]
    public class DigitAttribute : ValidationAttribute, IClientValidatable 
    {
        public override bool IsValid(object value)
        {
            if (value == null)
                return true;
            ulong res;
            return UInt64.TryParse(value.ToString(), out res);
        }

        #region Implementation of IClientValidatable

        /// <summary>
        /// When implemented in a class, returns client validation rules for that class.
        /// </summary>
        /// <returns>
        /// The client validation rules for this validator.
        /// </returns>
        /// <param name="metadata">The model metadata.</param><param name="context">The controller context.</param>
        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            return new []
                       {
                           new ModelClientValidationRule
                               {
                                   ValidationType = "digits",
                                   ErrorMessage = ErrorMessage
                               }
                       };
        }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,AllowMultiple = false)]
    public class EmailAttribute : RegularExpressionAttribute, IClientValidatable 
    {
        private const string ValidEmailRegEx = @"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$";

        public EmailAttribute(): base(ValidEmailRegEx)
        {
            ErrorMessage = "Enter a valid email address";
        }

        #region Implementation of IClientValidatable

        /// <summary>
        /// When implemented in a class, returns client validation rules for that class.
        /// </summary>
        /// <returns>
        /// The client validation rules for this validator.
        /// </returns>
        /// <param name="metadata">The model metadata.</param><param name="context">The controller context.</param>
        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            return new []
                       {
                           new ModelClientValidationRule
                               {
                                   ValidationType = "email",
                                   ErrorMessage = ErrorMessage
                               }
                       };
        }

        #endregion
    }
}