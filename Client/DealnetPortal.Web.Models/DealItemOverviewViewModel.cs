using System;

namespace DealnetPortal.Web.Models
{
    public class DealItemOverviewViewModel
    {
        public int Id { get; set; }

        public string CustomerName { get; set; }

        public string Status { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public string Action { get; set; }

        public string Email { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Phone { get; set; }

        public string Date { get; set; }
        public string Equipment { get; set; }
        public string Value { get; set; }
        public string RemainingDescription { get; set; }
    }
}