namespace DealnetPortal.Api.Models.Contract
{
    public class ContractDocumentDTO : DocumentDTO
    {
        public int Id { get; set; }
        public int DocumentTypeId { get; set; }                

        public int ContractId { get; set; }        
    }
}
