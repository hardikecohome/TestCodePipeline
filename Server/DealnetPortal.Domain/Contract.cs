using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;

namespace DealnetPortal.Domain
{
    public class Contract
    {
        public Contract()
        {
            SecondaryCustomers = new HashSet<Customer>();
            HomeOwners = new HashSet<Customer>();
            InitialCustomers = new HashSet<Customer>();
            Details = new ContractDetails();
            Documents = new HashSet<ContractDocument>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]        
        public int Id { get; set; }

        public string DealerId { get; set; }
        [ForeignKey("DealerId")]
        public virtual ApplicationUser Dealer { get; set; }

        public ContractState ContractState { get; set; }        

        public DateTime CreationTime { get; set; }

        public DateTime? LastUpdateTime { get; set; }
        
        public Customer PrimaryCustomer { get; set; }

        /// <summary>
        /// Installation address are linked to a deal
        /// </summary>
        //public Location InstallationAddress { get; set; }

        /// <summary>
        /// Aspire dealer for contract
        /// </summary>
        public string ExternalSubDealerName { get; set; }
        public string ExternalSubDealerId { get; set; }

        public ICollection<Customer> SecondaryCustomers { get; set; }

        /// <summary>
        /// Same customers or borrower can be home owner
        /// </summary>
        public ICollection<Customer> HomeOwners { get; set; }

        /// <summary>
        /// customers initially added to contract before decline: the same enties as Primary and Secondary customers
        /// </summary>
        public ICollection<Customer> InitialCustomers { get; set; }

        public virtual PaymentInfo PaymentInfo { get; set; }
        
        public EquipmentInfo Equipment { get; set; }

        public ContractDetails Details { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
        
        public ICollection<ContractDocument> Documents { get; set; }

        public int? CustomerContractInfoId { get; set; }
        [ForeignKey("CustomerContractInfoId")]
        public CustomerContractInfo CustomerContractInfo { get; set; }

        /// <summary>
        /// true, if current contract was declined early or currently in declined state
        /// </summary>
        public bool? WasDeclined { get; set; }
        /// <summary>
        /// true, if it's a new contract created by customer and isn't edited by dealer
        /// </summary>
        public bool? IsCreatedByCustomer { get; set; }
    }
}
