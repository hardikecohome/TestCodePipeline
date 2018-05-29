using System.Collections.Generic;
using System.Linq;
using DealnetPortal.Domain;
using DealnetPortal.Domain.Repositories;

namespace DealnetPortal.DataAccess.Repositories
{
    public class LicenseDocumentRepository : BaseRepository, ILicenseDocumentRepository
    {
        public LicenseDocumentRepository(IDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

        public IList<LicenseDocument> GetAllLicenseDocuments()
        {
            return _dbContext.LicenseDocuments.ToList();
        }
    }
}
