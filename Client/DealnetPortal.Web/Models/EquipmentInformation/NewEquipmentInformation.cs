﻿using System;
using System.ComponentModel.DataAnnotations;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Infrastructure.Attributes;

namespace DealnetPortal.Web.Models.EquipmentInformation
{                                              
    public class NewEquipmentInformation
    {
        [CustomRequired]
        public string Type { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "Type")]
        public string TypeDescription { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "Type")]
        public EquipmentTypeDTO EquipmentType { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "SubType")]
        public EquipmentSubTypeDTO EquipmentSubType { get; set; }

        [CustomRequired]
        [StringLength(500, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "Description")]
        [RegularExpression(@"^[^'<>&]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SomeSpecialCharactersAreRestricted")]
        public string Description { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "Cost")]
        [RegularExpression(@"^[1-9]\d{0,11}([.,][0-9][0-9]?)?$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "CostIncorrectFormat")]
        public double? Cost { get; set; }

        [CustomRequired]
        [RegularExpression(@"^[1-9]\d{0,5}([.,][0-9][0-9]?)?$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "MonthlyCostIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "MonthlyCost")]
        public double? MonthlyCost { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "MCOReducedDownPayment")]
        public double? MonthlyCostLessDP { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "EstimatedRetailCost")]
        [RegularExpression(@"^[1-9]\d{0,11}([.,][0-9][0-9]?)?$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "EstimatedCostIncorrectFormat")]
        public double? EstimatedRetailCost { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "EstimatedInstallationDate")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? EstimatedInstallationDate { get; set; }

        public int Id { get; set; }
    }
}