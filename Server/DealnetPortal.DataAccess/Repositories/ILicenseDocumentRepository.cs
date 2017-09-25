using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Domain;
using DealnetPortal.Domain.Dealer;

namespace DealnetPortal.DataAccess.Repositories
{
    public interface ILicenseDocumentRepository
    {
        IList<LicenseDocument> GetAllLicenseDocuments();
    }
}
