using Azure;
using CynapCRM.Services.FieldAPI.Models.Dto;
using CynapCRM.Services.FieldAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.FieldAPI.Controllers
{
    [Route("api/planning")]
    [ApiController]
    public class PlanningVisiteController : ControllerBase
    {
        private readonly IKPIService _fieldService;
        protected ResponseDto _response;
        public PlanningVisiteController(IKPIService fieldService)
        {
            _fieldService = fieldService;
            _response = new ResponseDto();
        }
        [HttpPost("createUpdate")]
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
                var result = await _fieldService.CreateOrUpdatePlanningAsync(planningDto);
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
                return StatusCode(500, _response);
            }
        }
        [HttpPut("ChangeStatus/{id:int}")]
        public async Task<IActionResult> ChangePlanningStatus(int id, [FromQuery] string newStatus)
        {
            try
            {
                if (id <= 0 || string.IsNullOrEmpty(newStatus))
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de planning ou nouveau statut invalide.";
                    return BadRequest(_response);
                }
                var success = await _fieldService.ChangePlanningStatusAsync(id, newStatus);
                if (!success)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Échec du changement de statut du planning.";
                    return NotFound(_response);
                }
                _response.Message = $"Le statut a été mis à jour : {newStatus}";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Une erreur est survenue : {ex.Message}";
                return StatusCode(500, _response);
            }
        }
        [HttpGet("{idPlanning:int}")]
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
                var result = await _fieldService.GetPlanningByIdAsync(idPlanning);
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
                return StatusCode(500, _response);
            }
        }
        [HttpGet("delegue/{idDelegue:int}")]
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
                var result = await _fieldService.GetPlanningByDelegueAsync(idDelegue);
                _response.Result = result;
                _response.Message = "Plannings de visite récupérés avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Une erreur est survenue : {ex.Message}";
                return StatusCode(500, _response);
            }
        }
        [HttpDelete("{idPlanning:int}")]
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
                var success = await _fieldService.DeletePlanningAsync(idPlanning);
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
                return StatusCode(500, _response);
            }
        }




    }
}
