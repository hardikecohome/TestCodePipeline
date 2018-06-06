using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DealnetPortal.Domain
{
    public class UserSettings
    {
        public UserSettings()
        {
            SettingValues = new HashSet<SettingValue>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public virtual ICollection<SettingValue> SettingValues { get; set; }
    }
}
