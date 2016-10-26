using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;

namespace DealnetPortal.Domain
{
    public class ContractDetails
    {
        public AgreementType? AgreementType { get; set; }

        public double? HouseSize { get; set; }

        /// <summary>
        /// Aspire Transaction ID
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// State of contract in Aspire
        /// </summary>
        public string IntegrationState { get; set; }

        public string SignatureTransactionId { get; set; }

        public string SignatureDocumentId { get; set; }

        public DateTime? SignatureInitiatedTime { get; set; }

        public SignatureStatus? SignatureStatus { get; set; }

        public DateTime? SignatureTime { get; set; }
    }
}
