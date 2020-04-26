using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Profiles
{
    public class PointOfInterestProfile : Profile
    {
        public PointOfInterestProfile()
        {
            CreateMap<Entities.PointOfInterest, Models.PointsOfInterestDto>();
            CreateMap<Models.PointsOfInterestForCreationDto, Entities.PointOfInterest>();
            CreateMap<Models.PointsOfInterestForUpdateDto, Entities.PointOfInterest>().ReverseMap();
        }
    }
}
