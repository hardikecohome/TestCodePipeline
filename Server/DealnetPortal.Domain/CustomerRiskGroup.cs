using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DealnetPortal.Domain
{
    public class CustomerRiskGroup
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string GroupName { get; set; }

        public int BeaconScoreFrom { get; set; }

        public int BeaconScoreTo { get; set; }

        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }

        public bool CapOutMaxAmt { get; set; }
    }
}
