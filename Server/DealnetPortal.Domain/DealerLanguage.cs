using System.ComponentModel.DataAnnotations.Schema;

namespace DealnetPortal.Domain
{
    public class DealerLanguage
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int LanguageId { get; set; }
        [ForeignKey("LanguageId")]
        public virtual Language Language { get; set; }
        public int CustomerLinkId { get; set; }
        [ForeignKey("CustomerLinkId")]
        public CustomerLink CustomerLink { get; set; }
    }
}
