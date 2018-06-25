using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Domain
{
    public class Language
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(2)]
        public string Code { get; set; }
        [MaxLength(20)]
        public string Name { get; set; }
    }
}
