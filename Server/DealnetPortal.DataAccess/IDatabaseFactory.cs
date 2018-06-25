using System;

namespace DealnetPortal.DataAccess
{
    public interface IDatabaseFactory : IDisposable
    {
        ApplicationDbContext Get();
    }
}
