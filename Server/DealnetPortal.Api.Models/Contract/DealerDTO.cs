namespace DealnetPortal.Api.Models.Contract
{
    public class DealerDTO : CustomerDTO
    {
        public string ParentDealerUserName { get; set; }
        public string ProductType { get; set; }
        public string ChannelType { get; set; }
        public string Ratecard { get; set; }
        public string LeaseRatecard { get; set; }
        public string Role { get; set; }
    }
}
