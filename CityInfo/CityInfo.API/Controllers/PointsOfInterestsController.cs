using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities/{cityId}/pointsofinterests")]
    public class PointsOfInterestsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetPointsOfInterests(int cityId)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null) return NotFound();

            return Ok(city.PointsOfInterests);
        }

        [HttpGet("{id}")]
        public IActionResult GetPointOfInterests(int cityId, int id)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null) return NotFound();

            var point = city.PointsOfInterests.FirstOrDefault(p => p.Id == id);
            if (point == null) return NotFound();

            return Ok(point);
        }
    }
}
