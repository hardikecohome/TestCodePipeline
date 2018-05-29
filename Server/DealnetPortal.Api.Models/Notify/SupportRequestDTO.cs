﻿namespace DealnetPortal.Api.Models.Notify
{
    public class SupportRequestDTO
    {
        public int Id { get; set; }
        public string DealerName { get; set; }
        public string YourName { get; set; }
        public string LoanNumber { get; set; }
        public string SupportType { get; set; }
        public string HelpRequested { get; set; }
        public string BestWay { get; set; }
        public string ContactDetails { get; set; }
    }
}
