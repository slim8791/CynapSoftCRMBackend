using CynapCRM.Services.FieldAPI.Models.Dto;
using CynapCRM.Services.FieldAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.FieldAPI.Controllers
{
    [Route("api/visites")]
    [ApiController]
    public class VisitesController : ControllerBase
    {
        private readonly IKPIService _fieldService;
        protected ResponseDto _response;
        public VisitesController(IKPIService fieldService)
        {
            _fieldService = fieldService;
            _response = new ResponseDto();
        }
        [HttpPost("createUpdate")]
        public async Task<IActionResult> CreateUpdateVisite([FromBody] VisiteDto visiteDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de visite invalides.";
                    return BadRequest(_response);
                }
                var result = await _fieldService.CreateOrUpdateVisiteAsync(visiteDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur lors de la création/mise à jour de la visite.";
                    return BadRequest(_response);
                }
                _response.Result = result;
                _response.Message = "Visite enregistrée avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Une erreur est survenue : {ex.Message}";
                return StatusCode(500, _response);
            }
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetVisiteById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de visite invalide.";
                    return BadRequest(_response);
                }
                var visite = await _fieldService.GetVisiteByIdAsync(id);
                if (visite == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Visite non trouvée.";
                    return NotFound(_response);
                }
                _response.Result = visite;
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
        public async Task<IActionResult> GetVisitesByDelegueId(int idDelegue)
        {
            try
            {
                if (idDelegue <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de délégué invalide.";
                    return BadRequest(_response);
                }
                var visites = await _fieldService.GetVisitesByDelegueAsync(idDelegue);
                if (visites == null || !visites.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucune visite trouvée pour ce délégué.";
                    return NotFound(_response);
                }
                _response.Result = visites;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Une erreur est survenue : {ex.Message}";
                return StatusCode(500, _response);
            }
        }
        [HttpGet("tournee/{idTournee:int}")]
        public async Task<IActionResult> GetVisitesByTourneeId(int idTournee)
        {
            try
            {
                if (idTournee <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de tournée invalide.";
                    return BadRequest(_response);
                }
                var visites = await _fieldService.GetVisitesByTourneeAsync(idTournee);
                if (visites == null || !visites.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucune visite trouvée pour cette tournée.";
                    return NotFound(_response);
                }
                _response.Result = visites;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Une erreur est survenue : {ex.Message}";
                return StatusCode(500, _response);
            }
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteVisite(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de visite invalide.";
                    return BadRequest(_response);
                }
                var success = await _fieldService.DeleteVisiteAsync(id);
                if (!success)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Échec de la suppression de la visite.";
                    return NotFound(_response);
                }
                _response.Message = "Visite supprimée avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Une erreur est survenue : {ex.Message}";
                return StatusCode(500, _response);
            }
        }
        [HttpPatch("AffectToTournee/{idVisite:int}/{idTournee:int}")]
        public async Task<IActionResult> AffectVisiteToTournee(int idVisite, int idTournee)
        {
            try
            {
                if (idVisite <= 0 || idTournee <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de visite ou de tournée invalide.";
                    return BadRequest(_response);
                }
                var success = await _fieldService.AffectVisiteToTourneeAsync(idVisite, idTournee);
                if (!success)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Échec de l'affectation de la visite à la tournée.";
                    return NotFound(_response);
                }
                _response.Message = "Visite affectée à la tournée avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Une erreur est survenue : {ex.Message}";
                return StatusCode(500, _response);
            }


        }
        [HttpPatch("complete/{id:int}")]
        public async Task<IActionResult> CompleteVisite(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de visite invalide.";
                    return BadRequest(_response);
                }
                var success = await _fieldService.CompleteVisiteAsync(id);
                if (!success)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Échec de la complétion de la visite.";
                    return NotFound(_response);
                }
                _response.Message = "Visite complétée avec succès.";
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
