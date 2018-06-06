using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Domain
{
    public class DocumentType
    {
        [Key]
        public int Id { get; set; }
        public string Prefix { get; set; }
        public string Description { get; set; }
        public string DescriptionResource { get; set; }
        public bool IsMandatory { get; set; }
    }
}
