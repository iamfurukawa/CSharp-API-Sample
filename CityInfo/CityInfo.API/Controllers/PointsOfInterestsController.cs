using AutoMapper;
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
        private ICityInfoRepository _cityInfoRepository;
        private IMapper _mapper;

        public PointsOfInterestsController(ILogger<PointsOfInterestsController> logger, IMailService localMailService, ICityInfoRepository cityInfoRepository, IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _localMailService = localMailService ?? throw new ArgumentNullException(nameof(localMailService));
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(localMailService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public IActionResult GetPointsOfInterests(int cityId)
        {

            try
            {
                if(!_cityInfoRepository.CityExists(cityId))
                {
                    _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interests.");
                    return NotFound();
                }

                var pointsOfInterestsForCity = new List<PointsOfInterestDto>();

                return Ok(_mapper.Map<IEnumerable<PointsOfInterestDto>>(pointsOfInterestsForCity));
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
            if (!_cityInfoRepository.CityExists(cityId)) return NotFound();

            var cityPoi = _cityInfoRepository.GetPointOfInterestForCity(cityId, id);

            if (cityPoi == null) return NotFound();

            return Ok(_mapper.Map<PointsOfInterestDto>(cityPoi));
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
            if (!_cityInfoRepository.CityExists(cityId)) return NotFound();

            var finalPOI = _mapper.Map<Entities.PointOfInterest>(pointOfInterest);
            _cityInfoRepository.AddPointOfInterestForCity(cityId, finalPOI);
            _cityInfoRepository.Save();
            var createdPOIToReturn = _mapper.Map<Models.PointsOfInterestDto>(finalPOI);

            return CreatedAtRoute("GetPointOfInterest", new { cityId, id = createdPOIToReturn.Id }, createdPOIToReturn);
        }

        [HttpPut("{id}")]
        public IActionResult UpdatePointOfInterest(int cityId, int id, [FromBody] PointsOfInterestForUpdateDto pointOfInterest)
        {
            if (pointOfInterest.Description == pointOfInterest.Name)
            {
                ModelState.AddModelError("Description", "The provided description should be different from the name.");
            }

            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!_cityInfoRepository.CityExists(cityId)) return NotFound();

            var point = _cityInfoRepository.GetPointOfInterestForCity(cityId, id);
            if (point == null) return NotFound();

            _mapper.Map(pointOfInterest, point);
            _cityInfoRepository.UpdatePointOfInterestForCity(cityId, point);
            _cityInfoRepository.Save();

            return NoContent();
        }

        [HttpPatch("{id}")]
        public IActionResult PartiallyUpdatePointOfInterest(int cityId, int id, [FromBody] JsonPatchDocument<PointsOfInterestDto> patchDoc)
        {

            if (!_cityInfoRepository.CityExists(cityId)) return NotFound();

            var poiEntity = _cityInfoRepository.GetPointOfInterestForCity(cityId, id);

            if (poiEntity == null) return NotFound();

            var poiToPatch = _mapper.Map<PointsOfInterestDto>(poiEntity);

            patchDoc.ApplyTo(poiToPatch, ModelState);

            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (poiToPatch.Description == poiToPatch.Name)
            {
                ModelState.AddModelError("Description", "The provided description should be different from the name.");
            }

            if (!TryValidateModel(poiToPatch)) return BadRequest(ModelState);

            _mapper.Map(poiToPatch, poiEntity);
            _cityInfoRepository.UpdatePointOfInterestForCity(cityId, poiEntity);
            _cityInfoRepository.Save();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeletePointOfInterest(int cityId, int id)
        {
            if (!_cityInfoRepository.CityExists(cityId)) return NotFound();

            var pointEntity = _cityInfoRepository.GetPointOfInterestForCity(cityId, id);
            if (pointEntity == null) return NotFound();

            _cityInfoRepository.DeletePointOfInterest(pointEntity);

            _localMailService.Send("Point of interest deleted.", $"Point of interest {pointEntity.Name} with id {pointEntity.Id} was deleted.");

            return NoContent();
        }
    }
}
