﻿using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Domain;

namespace DealnetPortal.Api.Integration.Services
{
    public interface IUsersService
    {
        IList<Claim> GetUserClaims(string userId);
        Task<IList<Alert>> SyncAspireUser(ApplicationUser user);
    }
}