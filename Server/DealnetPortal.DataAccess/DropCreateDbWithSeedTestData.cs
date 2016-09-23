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
                new EquipmentType {Name = "Air Conditioner", Type = "ECO1"},
                new EquipmentType {Name = "Boiler", Type = "ECO2"},
                new EquipmentType {Name = "Doors", Type = "ECO3"},
                new EquipmentType {Name = "Fireplace", Type = "ECO4"},
                new EquipmentType {Name = "Furnace", Type = "ECO5"},
                new EquipmentType {Name = "HWT", Type = "ECO6"},
                new EquipmentType {Name = "Plumbing", Type = "ECO7"},
                new EquipmentType {Name = "Roofing", Type = "ECO9"},
                new EquipmentType {Name = "Siding", Type = "ECO10"},
                new EquipmentType {Name = "Tankless Water Heater", Type = "ECO11"},
                new EquipmentType {Name = "Windows", Type = "ECO13"},
                new EquipmentType {Name = "Sunrooms", Type = "ECO38"},
                new EquipmentType {Name = "Air Handler", Type = "ECO40"},
                new EquipmentType {Name = "Flooring", Type = "ECO42"},
                new EquipmentType {Name = "Porch Enclosure", Type = "ECO43"},
                new EquipmentType {Name = "Water Treatment System", Type = "ECO44"},
                new EquipmentType {Name = "Heat Pump", Type = "ECO45"},
                new EquipmentType {Name = "HRV", Type = "ECO46"},
                new EquipmentType {Name = "Bathroom", Type = "ECO47"},
                new EquipmentType {Name = "Kitchen", Type = "ECO48"},
                new EquipmentType {Name = "Hepa System", Type = "ECO49"},
                new EquipmentType {Name = "Unknown", Type = "ECO50"},
                new EquipmentType {Name = "Security System", Type = "ECO52"},
                new EquipmentType {Name = "Basement Repair", Type = "ECO55"}
            };
            context.EquipmentTypes.AddRange(equipmentTypes);
        }
    }
}
