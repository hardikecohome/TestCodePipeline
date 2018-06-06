﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DealnetPortal.Api.Common.Enumeration;

namespace DealnetPortal.Domain
{
    public class Email
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public EmailType EmailType { get; set; }

        [MaxLength(256)]
        public string EmailAddress { get; set; }

        public Customer Customer { get; set; }
    }
}
