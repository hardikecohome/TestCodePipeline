﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Integration.Services
{
    public interface IPersonalizedMessageService
    {
        Task<HttpResponseMessage> SendMessage(string phonenumber, string messagebody);
    }
}