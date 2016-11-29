using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Models.Aspire.AspireDb
{
    public class GenericSubDealer
    {
        public string RefType { get; set; }
        public int Oid { get; set; }
        public int SeqNum { get; set; }
        public long FieldNum { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SubmissionValue { get; set; }
    }
}
