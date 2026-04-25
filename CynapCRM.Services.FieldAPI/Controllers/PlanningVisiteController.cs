using Azure;
using CynapCRM.Services.FieldAPI.Models.Dto;
using CynapCRM.Services.FieldAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.FieldAPI.Controllers
{

    [ApiController]
    [Route("api/plannings")]
    [Authorize]

    public class PlanningVisiteController : ControllerBase
    {
        private readonly IPlanningService _planningService;
        protected ResponseDto _response;
        public PlanningVisiteController(IPlanningService planningService)
        {
            _planningService = planningService;
            _response = new ResponseDto();
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> CreateUpdatePlanningVisite([FromBody] PlanningVisiteDto planningDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de planning de visite invalides.";
                    return BadRequest(_response);
                }
                var result = await _planningService.CreateOrUpdatePlanningAsync(planningDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur lors de la création/mise à jour du planning.";
                    return BadRequest(_response);
                }
                _response.Result = result;
                _response.Message = "Planning enregistré avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Une erreur est survenue : {ex.Message}";
                return StatusCode(515, _response);
            }
        }
               [HttpGet("{idPlanning:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetPlanningById(int idPlanning)
        {
            try
            {
                if (idPlanning <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de planning de visite invalide.";
                    return BadRequest(_response);
                }
                var result = await _planningService.GetPlanningByIdAsync(idPlanning);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Planning de visite non trouvé.";
                    return NotFound(_response);
                }
                _response.Result = result;
                _response.Message = "Planning de visite récupéré avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Une erreur est survenue : {ex.Message}";
                return StatusCode(515, _response);
            }
        }

        [HttpGet("by-delegue/{idDelegue:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetPlanningByDelegue(int idDelegue)
        {
            try
            {
                if (idDelegue <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de délégué invalide.";
                    return BadRequest(_response);
                }
                var result = await _planningService.GetPlanningByDelegueAsync(idDelegue);
                _response.Result = result;
                _response.Message = "Plannings de visite récupérés avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Une erreur est survenue : {ex.Message}";
                return StatusCode(515, _response);
            }
        }

        [HttpGet("by-range")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetPlanningsByDateRange([FromQuery] int idDelegue,
                                                                    [FromQuery] DateTime startDate,
                                                                        [FromQuery] DateTime endDate)
        {
            try
            {
                var result = await _planningService
                    .GetPlanningsByDateRangeAsync(idDelegue, startDate, endDate);

                _response.Result = result;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }
        [HttpGet("by-date")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetPlanningByDelegueAndDate([FromQuery] int idDelegue,
                                                                            [FromQuery] DateTime date)
        {
            try
            {
                var result = await _planningService
                    .GetPlanningByDelegueAndDateAsync(idDelegue, date);

                _response.Result = result;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }


        [HttpDelete("{idPlanning:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> DeletePlanning(int idPlanning)
        {
            try
            {
                if (idPlanning <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de planning de visite invalide.";
                    return BadRequest(_response);
                }
                var success = await _planningService.DeletePlanningAsync(idPlanning);
                if (!success)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Échec de la suppression du planning de visite.";
                    return NotFound(_response);
                }
                _response.Message = "Planning de visite supprimé avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Une erreur est survenue : {ex.Message}";
                return StatusCode(515, _response);
            }
        }

        [HttpPut("{idPlanning:int}/validate")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> ValidatePlanning(int idPlanning)
        {
            try
            {
                var result = await _planningService
                    .ValidatePlanningAsync(idPlanning);

                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message =
                        "Validation impossible (planning introuvable ou déjà validé).";
                    return BadRequest(_response);
                }

                _response.Message = "Planning validé avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }
    }
}
