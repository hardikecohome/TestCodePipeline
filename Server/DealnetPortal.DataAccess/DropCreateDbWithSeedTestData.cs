using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Domain;

namespace DealnetPortal.DataAccess
{
    public class DropCreateDbWithSeedTestData :
        DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            SetTestUsers(context);
            SetTestEquipmentTypes(context);
        }

        private void SetTestUsers(ApplicationDbContext context)
        {
            var user = new ApplicationUser()
            {
                Email = "user@user.com",
                UserName = "user@user.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                PasswordHash = "AAInS7oMLYVc0Z6tOXbu224LqdIGygS7kGnngFWX8jB4JHjRpZYSYwubaf3D6LknnA==", //Password: 123_Qwe
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773"
            };
            context.Users.Add(user);            
        }

        private void SetTestEquipmentTypes(ApplicationDbContext context)
        {
            var equipmentTypes = new[]
            {
                new EquipmentType {Name = "Air Conditioner"},
                new EquipmentType {Name = "Boiler"},
                new EquipmentType {Name = "Doors"},
                new EquipmentType {Name = "Fireplace"},
                new EquipmentType {Name = "Furnace"},
                new EquipmentType {Name = "HWT"},
                new EquipmentType {Name = "Plumbing"},
                new EquipmentType {Name = "Roofing"},
                new EquipmentType {Name = "Siding"},
                new EquipmentType {Name = "Tankless Water Heater"},
                new EquipmentType {Name = "Windows"},
                new EquipmentType {Name = "Sunrooms"},
                new EquipmentType {Name = "Air Handler"},
                new EquipmentType {Name = "Flooring"},
                new EquipmentType {Name = "Porch Enclosure"},
                new EquipmentType {Name = "Water Treatment System"},
                new EquipmentType {Name = "Heat Pump"},
                new EquipmentType {Name = "HRV"},
                new EquipmentType {Name = "Bathroom"},
                new EquipmentType {Name = "Kitchen"},
                new EquipmentType {Name = "Hepa System"},
                new EquipmentType {Name = "Unknown"},
                new EquipmentType {Name = "Security System"},
                new EquipmentType {Name = "Basement Repair"}
            };
            context.EquipmentTypes.AddRange(equipmentTypes);
        }
    }
}
