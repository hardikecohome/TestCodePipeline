namespace DealnetPortal.Api.Models.Storage
{
    public class AgreementDocument
    {
        public string DocumentId { get; set; }
        public string Name { get; set; }
        public byte[] DocumentRaw { get; set; }
        public string Type { get; set; }
    }
}
