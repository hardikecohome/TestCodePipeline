using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Models.Contract
{
    public class CommentDTO
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public IList<CommentDTO> Replies { get; set; }
        public int? CommentId { get; set; }
        public int? ContractId { get; set; }
        public bool IsOwn { get; set; }
    }
}
