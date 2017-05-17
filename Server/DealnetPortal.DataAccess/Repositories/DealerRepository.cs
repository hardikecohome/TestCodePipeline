using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Domain;
using Microsoft.Practices.ObjectBuilder2;

namespace DealnetPortal.DataAccess.Repositories
{
    public class DealerRepository : BaseRepository, IDealerRepository
    {
        public DealerRepository(IDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

        public string GetParentDealerId(string dealerId)
        {
            return base.GetUserById(dealerId).ParentDealerId;
        }

        public string GetUserIdByName(string userName)
        {
            return _dbContext.Users.FirstOrDefault(u => u.UserName == userName)?.Id;
        }

        public string GetDealerNameByCustomerLinkId(int customerLinkId)
        {
            return _dbContext.Users
                .FirstOrDefault(u => u.CustomerLinkId == customerLinkId)?.UserName;
        }

        public DealerProfile GetDealerProfile(string dealerId)
        {
            return _dbContext.DealerProfiles.FirstOrDefault(x => x.DealerId == dealerId);
        }

        public bool UpdateDealerProfile(DealerProfile profile)
        {
            var dbProfile = GetDealerProfile(profile.DealerId);
            var dbEquipments = dbProfile.Equipments;

            UpdateProfileEquipments(profile, dbEquipments);
            UpdateProfileArears(profile, dbProfile.Areas);



            //originEntity.Equipments.ForEach(e=> _dbContext.Entry(e).State = EntityState.Deleted);
            //_dbContext.DealerEquipments.AddRange(profile.Equipments);
            //originEntity.Areas.ForEach(e => _dbContext.Entry(e).State = EntityState.Deleted);
            //_dbContext.DealerArears.AddRange(profile.Areas);
            //_dbContext.Entry(profile).State = profile.Id == 0 ? EntityState.Added : EntityState.Modified;

            return true;
        }

        private void UpdateProfileEquipments(DealerProfile profile, ICollection<DealerEquipment> dbEquipments)
        {
            var existingEntities =
                dbEquipments.Where(
                    a => profile.Equipments.Any(ee => ee.EquipmentId == a.EquipmentId)).ToList();
            var newEntities = new List<DealerEquipment>();

            profile.Equipments.ForEach(equipment =>
            {
                var dbEquipment = dbEquipments.SingleOrDefault(x => x.EquipmentId == equipment.EquipmentId);
                if (dbEquipment != null)
                {
                    equipment.Id = dbEquipment.Id;
                }
                else
                {
                    newEntities.Add(equipment);
                    _dbContext.DealerEquipments.Add(equipment);
                }
            });
            var entriesForDelete = dbEquipments.Except(existingEntities).Except(newEntities).ToList();
            entriesForDelete.ForEach(e => _dbContext.DealerEquipments.Remove(e));
        }

        private void UpdateProfileArears(DealerProfile profile, ICollection<DealerArea> dbArears)
        {
            var existingEntities =
                dbArears.Where(
                    a => profile.Areas.Any(ee => ee.PostalCode == a.PostalCode)).ToList();
            var newEntities = new List<DealerArea>();

            profile.Areas.ForEach(area =>
            {
                var dbEquipment = dbArears.SingleOrDefault(x => x.PostalCode == area.PostalCode);
                if (dbEquipment != null)
                {
                    area.Id = dbEquipment.Id;
                }
                else
                {
                    newEntities.Add(area);
                    _dbContext.DealerArears.Add(area);
                }
            });
            var entriesForDelete = dbArears.Except(existingEntities).Except(newEntities).ToList();
            entriesForDelete.ForEach(e => _dbContext.DealerArears.Remove(e));
        }
    } 

    
}
