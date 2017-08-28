using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Domain
{
    public class DocumentBase
    {
        public string DocumentName { get; set; }

        public virtual byte[] DocumentBytes { get; set; }

        public DateTime CreationDate { get; set; }
    }
}
