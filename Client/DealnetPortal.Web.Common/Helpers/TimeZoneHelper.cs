using System;

namespace DealnetPortal.Web.Common.Helpers
{
    public static class TimeZoneHelper
    {
        private static int? ClientOffset { get; set; }

        public static void SetOffset(int? offset)
        {
            ClientOffset = offset;
        }

        public static int? GetOffset()
        {
            return ClientOffset;
        }

        public static DateTime TryConvertToLocalUserDate(this DateTime time)
        {
            if (time == default(DateTime)) return time;

            return ClientOffset == null ? time : time.AddMinutes(-1 * ClientOffset.Value);
        }
    }
}
