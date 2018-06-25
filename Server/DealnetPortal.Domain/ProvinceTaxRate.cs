using System.ComponentModel.DataAnnotations.Schema;

namespace DealnetPortal.Domain
{
    public class ProvinceTaxRate
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Province { get; set; }
        public double Rate { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
    }
}
