using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [CustomRequired]
        [Display(ResourceType = typeof (Resources.Resources), Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }    

    public class ResetPasswordViewModel
    {
        [CustomRequired]
        [EmailAddress]
        [Display(ResourceType = typeof (Resources.Resources), Name = "Email")]
        public string Email { get; set; }

        [CustomRequired]
        [StringLength(100, ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "MustBeAtLeastLong", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof (Resources.Resources), Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof (Resources.Resources), Name = "ConfirmPassword")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "PasswordAndConfirmationNotMatch")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class LoginViewModel
    {
        //[Required(AllowEmptyStrings = true)]
        [Display(ResourceType = typeof(Resources.Resources), Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof (Resources.Resources), Name = "UserName")]
        public string UserName { get; set; }

        [CustomRequired]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof (Resources.Resources), Name = "Password")]
        public string Password { get; set; }

        [Display(ResourceType = typeof (Resources.Resources), Name = "RememberMe")]
        public bool RememberMe { get; set; }
    }

    public class ChangePasswordModel
    {
        [CustomRequired]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof (Resources.Resources), Name = "CurrentPassword")]
        public string OldPassword { get; set; }

        [CustomRequired]
        [StringLength(100, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "MustBeAtLeastLong", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof (Resources.Resources), Name = "NewPassword")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof (Resources.Resources), Name = "ConfirmNewPassword")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "PasswordAndConfirmNotMatch")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordAnonymouslyViewModel : ChangePasswordModel
    {
        [CustomRequired]
        [EmailAddress]
        [Display(ResourceType = typeof(Resources.Resources), Name = "Email")]
        public string Email { get; set; }
    }

    public class RegisterViewModel
    {
        [CustomRequired]
        [EmailAddress]
        [Display(ResourceType = typeof(Resources.Resources), Name = "Email")]
        public string Email { get; set; }

        //[CustomRequired]
        [Display(ResourceType = typeof (Resources.Resources), Name = "UserName")]
        public string UserName { get; set; }

        //[CustomRequired]
        [StringLength(100, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "MustBeAtLeastLong", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof (Resources.Resources), Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof (Resources.Resources), Name = "ConfirmPassword")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "PasswordAndConfirmNotMatch")]
        public string ConfirmPassword { get; set; }

        public string ApplicationId { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [CustomRequired]
        [EmailAddress]
        [Display(ResourceType = typeof(Resources.Resources), Name = "Email")]
        public string Email { get; set; }
    }
}
