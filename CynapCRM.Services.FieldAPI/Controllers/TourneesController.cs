using CynapCRM.Services.FieldAPI.Models.Dto;
using CynapCRM.Services.FieldAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.FieldAPI.Controllers
{
    [Route("api/tournees")]
    [ApiController]
    public class TourneesController : ControllerBase
    {
        private readonly IKPIService _fieldService;
        protected ResponseDto _response;
        public TourneesController(IKPIService fieldService)
        {
            _fieldService = fieldService;
            _response = new ResponseDto();
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetTourneeById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de tournée invalide.";
                    return BadRequest(_response);
                }
                var tournee = await _fieldService.GetTourneeByIdAsync(id);
                if (tournee == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Tournée non trouvée.";
                    return NotFound(_response);
                }
                _response.Result = tournee;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Une erreur est survenue : {ex.Message}";
                return StatusCode(500, _response);
            }
        }
        [HttpGet("planning/{idPlanning:int}")]
        public async Task<IActionResult> GetTourneesByPlanningId(int idPlanning)
        {
            try
            {
                if (idPlanning <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de planning invalide.";
                    return BadRequest(_response);
                }
                var tournees = await _fieldService.GetTourneesByPlanningAsync(idPlanning);
                if (tournees == null || !tournees.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucune tournée trouvée pour ce planning.";
                    return NotFound(_response);
                }
                _response.Result = tournees;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Une erreur est survenue : {ex.Message}";
                return StatusCode(500, _response);
            }
        }
        [HttpPost("createUpdate")]
        public async Task<IActionResult> CreateUpdateTournee([FromBody] TourneeDto tourneeDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de tournée invalides.";
                    return BadRequest(_response);
                }
                var result = await _fieldService.CreateOrUpdateTourneeAsync(tourneeDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur lors de la création/mise à jour de la tournée.";
                    return BadRequest(_response);
                }
                _response.Result = result;
                _response.Message = "Tournée enregistrée avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Une erreur est survenue : {ex.Message}";
                return StatusCode(500, _response);
            }

        }
        [HttpPost("start/{id:int}")]
        public async Task<IActionResult> StartTournee(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de tournée invalide.";
                    return BadRequest(_response);
                }
                var success = await _fieldService.StartTourneeAsync(id);
                if (!success)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Impossible de démarrer la tournée. Vérifiez son statut actuel.";
                    return BadRequest(_response);
                }
                _response.Message = "Tournée démarrée avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Une erreur est survenue : {ex.Message}";
                return StatusCode(500, _response);
            }
        }
        [HttpPost("end/{id:int}")]
        public async Task<IActionResult> EndTournee(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de tournée invalide.";
                    return BadRequest(_response);
                }
                var success = await _fieldService.EndTourneeAsync(id);
                if (!success)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Impossible de terminer la tournée. Vérifiez son statut actuel.";
                    return BadRequest(_response);
                }
                _response.Message = "Tournée terminée avec succès.";
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
