using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Domain.Dealer;
using Microsoft.Practices.ObjectBuilder2;

namespace DealnetPortal.DataAccess.Repositories
{
    public class DealerOnboardingRepository : BaseRepository, IDealerOnboardingRepository
    {
        public DealerOnboardingRepository(IDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

        public DealerInfo GetDealerInfoById(int id)
        {
            return _dbContext.DealerInfos.Find(id);
        }

        public DealerInfo GetDealerInfoByAccessCode(string accessCode)
        {
            return _dbContext.DealerInfos.FirstOrDefault(di => di.AccessCode == accessCode);
        }

        public DealerInfo AddOrUpdateDealerInfo(DealerInfo dealerInfo, string accessCode = null)
        {
            DealerInfo dbDealer = null;

            if (!string.IsNullOrEmpty(accessCode))
            {
                dbDealer = GetDealerInfoByAccessCode(accessCode);
            }
            if (dbDealer == null && dealerInfo.Id != 0)
            {
                dbDealer = GetDealerInfoById(dealerInfo.Id);
            }
            if (dbDealer == null)
            {
                //add new
                dealerInfo.CreationTime = DateTime.Now;
                dealerInfo.LastUpdateTime = DateTime.Now;
                dbDealer = _dbContext.DealerInfos.Add(dealerInfo);
            }
            else
            {
                //update
                UpdateDealerInfo(dbDealer, dealerInfo);
            }

            return dbDealer;
        }

        public bool DeleteDealerInfo(int dealerInfoId)
        {
            throw new NotImplementedException();
        }

        public bool DeleteDealerInfo(string accessCode)
        {
            throw new NotImplementedException();
        }

        private void UpdateDealerInfo(DealerInfo dbDealerInfo, DealerInfo updateDealerInfo)
        {
            UpdateBaseDealerInfo(dbDealerInfo, updateDealerInfo);
            UpdateDealerCompanyInfo(dbDealerInfo, updateDealerInfo);
            dbDealerInfo.LastUpdateTime = DateTime.Now;
        }

        private void UpdateBaseDealerInfo(DealerInfo dbDealerInfo, DealerInfo updateDealerInfo)
        {
            if (dbDealerInfo.AccessCode != updateDealerInfo.AccessCode)
            {
                dbDealerInfo.AccessCode = updateDealerInfo.AccessCode;
            }            
        }

        private void UpdateDealerCompanyInfo(DealerInfo dbDealerInfo, DealerInfo updateDealerInfo)
        {
            if (dbDealerInfo.CompanyInfo == null && updateDealerInfo.CompanyInfo != null)
            {
                dbDealerInfo.CompanyInfo = updateDealerInfo.CompanyInfo;
            }
            else
            if (dbDealerInfo.CompanyInfo != null && updateDealerInfo.CompanyInfo == null)
            {
                _dbContext.CompanyInfos.Remove(dbDealerInfo.CompanyInfo);
            }
            else
            {
                updateDealerInfo.Id = dbDealerInfo.Id;
                var dbCompanyInfo = dbDealerInfo.CompanyInfo;
                var updateCompanyInfo = updateDealerInfo.CompanyInfo;
                if (dbCompanyInfo.FullLegalName != updateCompanyInfo.FullLegalName)
                {
                    dbCompanyInfo.FullLegalName = updateCompanyInfo.FullLegalName;
                }
                if (dbCompanyInfo.EmailAddress != updateCompanyInfo.EmailAddress)
                {
                    dbCompanyInfo.EmailAddress = updateCompanyInfo.EmailAddress;
                }
                if (dbCompanyInfo.OperatingName != updateCompanyInfo.OperatingName)
                {
                    dbCompanyInfo.OperatingName = updateCompanyInfo.OperatingName;
                }
                if (dbCompanyInfo.Phone != updateCompanyInfo.Phone)
                {
                    dbCompanyInfo.Phone = updateCompanyInfo.Phone;
                }
                if (dbCompanyInfo.Website != updateCompanyInfo.Website)
                {
                    dbCompanyInfo.Website = updateCompanyInfo.Website;
                }
                if (dbCompanyInfo.BusinessType != updateCompanyInfo.BusinessType)
                {
                    dbCompanyInfo.BusinessType = updateCompanyInfo.BusinessType;
                }
                if (dbCompanyInfo.NumberOfInstallers != updateCompanyInfo.NumberOfInstallers)
                {
                    dbCompanyInfo.NumberOfInstallers = updateCompanyInfo.NumberOfInstallers;
                }
                if (dbCompanyInfo.NumberOfSales != updateCompanyInfo.NumberOfSales)
                {
                    dbCompanyInfo.NumberOfSales = updateCompanyInfo.NumberOfSales;
                }
                if (dbCompanyInfo.YearsInBusiness != updateCompanyInfo.YearsInBusiness)
                {
                    dbCompanyInfo.YearsInBusiness = updateCompanyInfo.YearsInBusiness;
                }
                if (dbCompanyInfo.CompanyAddress.City != updateCompanyInfo.CompanyAddress?.City)
                {
                    dbCompanyInfo.CompanyAddress.City = updateCompanyInfo.CompanyAddress?.City;
                }
                if (dbCompanyInfo.CompanyAddress.PostalCode != updateCompanyInfo.CompanyAddress?.PostalCode)
                {
                    dbCompanyInfo.CompanyAddress.PostalCode = updateCompanyInfo.CompanyAddress?.PostalCode;
                }
                if (dbCompanyInfo.CompanyAddress.State != updateCompanyInfo.CompanyAddress?.State)
                {
                    dbCompanyInfo.CompanyAddress.State = updateCompanyInfo.CompanyAddress?.State;
                }
                if (dbCompanyInfo.CompanyAddress.Street != updateCompanyInfo.CompanyAddress?.Street)
                {
                    dbCompanyInfo.CompanyAddress.Street = updateCompanyInfo.CompanyAddress?.Street;
                }
                if (dbCompanyInfo.CompanyAddress.Unit != updateCompanyInfo.CompanyAddress?.Unit)
                {
                    dbCompanyInfo.CompanyAddress.Unit = updateCompanyInfo.CompanyAddress?.Unit;
                }

                UpdateCompanyProvinces(dbCompanyInfo, updateCompanyInfo.Provinces);
            }
        }

        private void UpdateCompanyProvinces(CompanyInfo dbCompanyInfo, ICollection<CompanyProvince> updatedProvinces)
        {
            var existingEntities =
                dbCompanyInfo.Provinces.Where(
                    p => updatedProvinces?.Any(up => up.Province == p.Province) ?? false).ToList();
            var entriesForDelete = dbCompanyInfo.Provinces.Except(existingEntities).ToList();
            entriesForDelete.ForEach(e => _dbContext.Entry(e).State = EntityState.Deleted);

            updatedProvinces?.Except(updatedProvinces.Where(up => existingEntities.Any(e => e.Province == up.Province))).
                ForEach(p =>
                {
                    p.CompanyInfo = dbCompanyInfo;
                    dbCompanyInfo.Provinces.Add(p);
                });            
        }
    }
}
