namespace DealnetPortal.Web.Models.Dealer
{
    public class DocumentResponseViewModel
    {
        public int? ItemId { get; set; }

        public int DealerInfoId { get; set; }

        public string AccessKey { get; set; }

        public bool IsSuccess { get; set; } = true;

        public string AggregatedError { get; set; }
    }
}