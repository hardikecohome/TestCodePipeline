﻿using System.ComponentModel.DataAnnotations;


namespace DealnetPortal.Web.Models.Enumeration
{
    public enum RelationshipStructure
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "Independent")]
        Independent,
        [Display(ResourceType = typeof(Resources.Resources), Name = "OEM")]
        OEM,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Distributor")]
        Distributor

    }
}
