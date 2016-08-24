using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using AutoMapper.Mappers;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Scanning;
using DealnetPortal.Web.Models;

namespace DealnetPortal.Web.App_Start
{
    public class AutoMapperConfig
    {
        public static void Configure()
        {
            MapModels();            
        }

        private static void MapModels()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<DriverLicenseData, RecognizedLicense>();
                cfg.CreateMap<Tuple<DriverLicenseData, IList<Alert>>, DriverLicenseViewModel>()
                    .ForMember(x => x.DriverLicense, o => o.MapFrom(src => src.Item1))
                    .ForMember(x => x.RecognitionErrors, o => o.ResolveUsing(src =>
                    src.Item2.Select(e => e.Header).ToList()));
            });


        }
    }
}