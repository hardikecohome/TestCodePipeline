using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Models
{
    public class CommentViewModel
    {
        public int Id { get; set; }
        [CustomRequired]
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public IList<CommentViewModel> Replies { get; set; }
        public int? ParentCommentId { get; set; }
        public int? ContractId { get; set; }
        public bool IsOwn { get; set; }
        public string AuthorName { get; set; }
    }
}
