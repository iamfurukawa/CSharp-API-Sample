using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities/{cityId}/pointsofinterest")]
    public class PointsOfInterestsController : ControllerBase
    {
        private ILogger<PointsOfInterestsController> _logger;
        private IMailService _localMailService;

        public PointsOfInterestsController(ILogger<PointsOfInterestsController> logger, IMailService localMailService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _localMailService = localMailService ?? throw new ArgumentNullException(nameof(localMailService));
        }

        [HttpGet]
        public IActionResult GetPointsOfInterests(int cityId)
        {

            try
            {
                var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
                if (city == null) return NotFound();

                return Ok(city.PointsOfInterests);
            }
            catch (Exception e)
            {
                _logger.LogCritical($"Exception while getting points of interest for city with id {cityId}", e);
                return StatusCode(500, "A problem happened while handling your request.");
            }
            
        }

        [HttpGet("{id}", Name = "GetPointOfInterest")]
        public IActionResult GetPointOfInterest(int cityId, int id)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null) return NotFound();

            var point = city.PointsOfInterests.FirstOrDefault(p => p.Id == id);
            if (point == null) return NotFound();

            return Ok(point);
        }

        [HttpPost]
        public IActionResult CreatePointOfInterest(int cityId, [FromBody] PointsOfInterestForCreationDto pointOfInterest)
        {
            if (pointOfInterest.Description == pointOfInterest.Name)
            {
                ModelState.AddModelError("Description", "The provided description should be different from the name.");
            }

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null) return NotFound();

            var lastestId = CitiesDataStore.Current.Cities.SelectMany(c => c.PointsOfInterests).Max(p => p.Id);

            var newPointOfInterest = new PointsOfInterestDto()
            {
                Id = ++lastestId,
                Name = pointOfInterest.Name,
                Description = pointOfInterest.Description
            };

            city.PointsOfInterests.Add(newPointOfInterest);

            return CreatedAtRoute("GetPointOfInterest", new { cityId, id = newPointOfInterest.Id }, newPointOfInterest);
        }

        [HttpPut("{id}")]
        public IActionResult UpdatePointOfInterest(int cityId, int id, [FromBody] PointsOfInterestForUpdateDto pointOfInterest)
        {
            if (pointOfInterest.Description == pointOfInterest.Name)
            {
                ModelState.AddModelError("Description", "The provided description should be different from the name.");
            }

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null) return NotFound();

            var point = city.PointsOfInterests.FirstOrDefault(p => p.Id == id);
            if (point == null) return NotFound();

            point.Name = pointOfInterest.Name;
            point.Description = pointOfInterest.Description;

            return NoContent();
        }

        [HttpPatch("{id}")]
        public IActionResult PartiallyUpdatePointOfInterest(int cityId, int id, [FromBody] JsonPatchDocument<PointsOfInterestDto> patchDoc)
        {
           
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null) return NotFound();

            var point = city.PointsOfInterests.FirstOrDefault(p => p.Id == id);
            if (point == null) return NotFound();

            var pointToPatch = new PointsOfInterestDto()
                {
                    Name = point.Name,
                    Description = point.Description
                };

            patchDoc.ApplyTo(pointToPatch, ModelState);

            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (pointToPatch.Description == pointToPatch.Name)
            {
                ModelState.AddModelError("Description", "The provided description should be different from the name.");
            }

            if (!TryValidateModel(pointToPatch)) return BadRequest(ModelState);

            point.Name = pointToPatch.Name;
            point.Description = pointToPatch.Description;

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeletePointOfInterest(int cityId, int id)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null) return NotFound();

            var point = city.PointsOfInterests.FirstOrDefault(p => p.Id == id);
            if (point == null) return NotFound();

            city.PointsOfInterests.Remove(point);

            _localMailService.Send("Point of interest deleted.", $"Point of interest {point.Name} with id {point.Id} was deleted.");

            return NoContent();
        }
    }
}
