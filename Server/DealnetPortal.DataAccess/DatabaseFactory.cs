using System;

namespace DealnetPortal.DataAccess
{
    public class DatabaseFactory : IDatabaseFactory
    {
        private ApplicationDbContext _dataContext;
        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Get().Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public ApplicationDbContext Get()
        {
            return _dataContext ?? (_dataContext = new ApplicationDbContext());
        }
    }
}
