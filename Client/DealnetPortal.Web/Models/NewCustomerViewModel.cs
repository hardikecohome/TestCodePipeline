﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Web.Models
{
    public class NewCustomerViewModel
    {
        public ApplicantPersonalInfo HomeOwner { get; set; }

        public CustomerContactInfoViewModel HomeOwnerContactInfo { get; set; }
        public AddressInformation ImprovmentLocation { get; set; }
        [Display(ResourceType = typeof(Resources.Resources), Name = "EstimatedMoveInDate")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? EstimatedMoveInDate { get; set; }
        public bool IsLessThenSix { get; set; }
        public bool IsUnknownAddress { get; set; }
        public bool IsLiveInCurrentAddress { get; set; }
                                                                             
        [Display(ResourceType = typeof(Resources.Resources), Name = "HomeImprovementType")]
        public List<EquipmentTypeDTO> SelectedEquipmentTypes { get; set; }

        [StringLength(500, ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "CustomersComment")]
        public string CustomerComment { get; set; }

        public IList<ProvinceTaxRateDTO> ProvinceTaxRates { get; set; }
        public IList<VarificationIdsDTO> VerificationIds { get; set; }
        public IList<EquipmentTypeDTO> EquipmentTypes { get; set; }
        public SelectList ContactMethods { get; set; }
    }
}
