using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Profile;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Models.MyProfile
{
    public class ProfileViewModel
    {
        public IList<EquipmentTypeDTO> EquipmentTypes { get; set; }
        public IList<EquipmentTypeDTO> DealerEquipments { get; set; }
        public IList<DealerAreaDTO> PostalCodes { get; set; }
    }

    //public class CategoryInformation
    //{
    //    public string Type { get; set; }
    //    public string Description { get; set; }
    //}

    public class PostalCodeInformation
    {
        public int Id { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "PostalCode")]
        [StringLength(6, MinimumLength = 5, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "PostalCodeIncorrectFormat")]
        public string Value { get; set; }
    }
}