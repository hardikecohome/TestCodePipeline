﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public byte[] UserIcon { get; set; }

        public byte[] Favicon { get; set; }

        public ICollection<SettingValue> SettingValues { get; set; }
    }
}
