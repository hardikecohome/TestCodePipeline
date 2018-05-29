﻿namespace DealnetPortal.DataAccess
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDatabaseFactory _databaseFactory;
        private ApplicationDbContext _dataContext;

        public UnitOfWork(IDatabaseFactory databaseFactory)
        {
            _databaseFactory = databaseFactory;
        }

        private ApplicationDbContext DataContext => _dataContext ?? (_dataContext = _databaseFactory.Get());

        public void Save()
        {
            DataContext.SaveChanges();
        }
    }
}
