using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Models.UserSettings
{
    public class ImageSettingDTO
    {
        public string Key { get; set; }
        public byte[] ValueBytes { get; set; }
    }
}
