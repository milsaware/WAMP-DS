using System;
using System.Collections.Generic;
using System.Linq;
using WAMP_DS.Models;

namespace WAMP_DS.Managers
{
    public class MagentoTimezoneManager
    {
        public List<MagentoTimezone> GetTimezones()
        {
            var mapper = new MagentoTimezoneMapper();

            return TimeZoneInfo
                .GetSystemTimeZones()
                .Select(x => new MagentoTimezone
                {
                    Name = $"{x.DisplayName} ({mapper.Convert(x.Id)})",
                    Code = mapper.Convert(x.Id)
                })
                .GroupBy(x => x.Code)
                .Select(x => x.First())
                .OrderBy(x => x.Name)
                .ToList();
        }
    }
}