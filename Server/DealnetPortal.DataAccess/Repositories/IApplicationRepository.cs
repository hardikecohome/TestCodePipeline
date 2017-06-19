using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Domain;

namespace DealnetPortal.DataAccess.Repositories
{
    public interface IApplicationRepository
    {
        Application GetApplication(string id);
    }
}
