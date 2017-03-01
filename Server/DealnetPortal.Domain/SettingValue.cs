using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Domain
{
    public class SettingValue
    {
        [Key]
        [Column(Order = 0)]
        public int UserSettingsId { get; set; }
        [ForeignKey("UserSettingsId")]
        public UserSettings UserSettings { get; set; }
        [Key]
        [Column(Order = 1)]
        public int KeyId { get; set; }
        [ForeignKey("KeyId")]
        public SettingKey Key { get; set; }
        public string Value { get; set; }
    }
}
