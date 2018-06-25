namespace DealnetPortal.Api.Models.DealerOnboarding
{
    public class DealerInfoKeyDTO
    {
        /// <summary>
        /// dealer info form Id
        /// </summary>
        public int DealerInfoId { get; set; }
        /// <summary>
        /// Form access key
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// last update item Id (uploaded document etc.)
        /// </summary>
        public int? ItemId { get; set; } 
    }
}
