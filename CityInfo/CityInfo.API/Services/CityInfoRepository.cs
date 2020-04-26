using CityInfo.API.Context;
using CityInfo.API.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Services
{
    public class CityInfoRepository : ICityInfoRepository
    {
        private CityInfoContext _ctx;

        public CityInfoRepository(CityInfoContext ctx)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
        }

        public IEnumerable<City> GetCities()
        {
            return _ctx.Cities.OrderBy(c => c.Name).ToList();
        }

        public City GetCity(int cityId, bool includePointOfInterest)
        {
            if (includePointOfInterest)
                return _ctx.Cities.Include(c => c.PointsOfInterests).Where(c => c.Id == cityId).FirstOrDefault();

            return _ctx.Cities.Where(c => c.Id == cityId).FirstOrDefault();
        }

        public PointOfInterest GetPointOfInterestForCity(int cityId, int pointOfInterestId)
        {
            return _ctx.PointOfInterests.Where(p => p.CityId == cityId && p.Id == pointOfInterestId).FirstOrDefault();
        }

        public IEnumerable<PointOfInterest> GetPointsOfInterestForCity(int cityId)
        {
            return _ctx.PointOfInterests.Where(p => p.CityId == cityId).ToList();
        }

        public bool CityExists(int cityId)
        {
            return _ctx.Cities.Any(c => c.Id == cityId);
        }

        public void AddPointOfInterestForCity(int cityId, PointOfInterest pointOfInterest)
        {
            var city = GetCity(cityId, false);
            city.PointsOfInterests.Add(pointOfInterest);
        }

        public void UpdatePointOfInterestForCity(int cityId, PointOfInterest poi)
        {

        }
        public void DeletePointOfInterest(PointOfInterest pointOfInterest)
        {
            _ctx.PointOfInterests.Remove(pointOfInterest);
        }

        public bool Save()
        {
            return _ctx.SaveChanges() >= 0;
        }
    }
}
