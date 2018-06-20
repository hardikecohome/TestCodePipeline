﻿using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models.Enumeration
{

    public enum BusinessType
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "SoleProp")]
        SoleProp,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Partnership")]
        Partnership,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Incorporated")]
        Incorporated,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Limited")]
        Limited
    }
    
}
