using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DealnetPortal.Domain.Dealer
{
    public class ProductService
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int EquipmentId { get; set; }
        [ForeignKey("EquipmentId")]
        public virtual EquipmentType Equipment { get; set; }

        public int ProductInfoId { get; set; }
        [ForeignKey(nameof(ProductInfoId))]
        [Required]
        public ProductInfo ProductInfo { get; set; }
    }
}
