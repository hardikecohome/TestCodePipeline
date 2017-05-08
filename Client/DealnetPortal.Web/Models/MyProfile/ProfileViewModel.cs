﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Models.MyProfile
{
    public class ProfileViewModel
    {
        public IList<EquipmentTypeDTO> EquipmentTypes { get; set; }
        public IList<CategoryInformation> Categories { get; set; }
        public IList<PostalCodeInformation> PostalCodes { get; set; }
    }

    public class CategoryInformation
    {
        public int Id { get; set; }
        public string Type { get; set; }
    }

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