using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DealnetPortal.Domain.Dealer
{
    public class CompanyProvince
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int CompanyInfoId { get; set; }

        public string Province { get; set; }

        [ForeignKey(nameof(CompanyInfoId))]
        [Required]
        public CompanyInfo CompanyInfo { get; set; }
    }
}
