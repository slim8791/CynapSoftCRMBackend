using CynapCRM.Services.FieldAPI.Models.Dto;
using CynapCRM.Services.FieldAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.FieldAPI.Controllers
{
    [Route("api/visites")]
    [ApiController]
    public class VisitesController : ControllerBase
    {
        private readonly IVisiteService _visiteService;
        protected ResponseDto _response;
        public VisitesController(IVisiteService visiteService)
        {
            _visiteService = visiteService;
            _response = new ResponseDto();
        }

        [HttpPost]
        [Authorize(Roles = "DELEGUE,ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> CreateOrUpdateVisite([FromBody] VisiteDto visiteDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de visite invalides.";
                    return BadRequest(_response);
                }
                var result = await _visiteService.CreateOrUpdateVisiteAsync(visiteDto);
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

        [HttpGet("{idVisite:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetVisiteById(int idVisite)
        {
            try
            {
                if (idVisite <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de visite invalide.";
                    return BadRequest(_response);
                }
                var visite = await _visiteService.GetVisiteByIdAsync(idVisite);
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

        [HttpGet("by-delegue/{idDelegue:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
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
                var visites = await _visiteService.GetVisitesByDelegueAsync(idDelegue);
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

        [HttpGet("by-planning/{idPlanning:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetVisitesByPlanning(int idPlanning)
        {
            try
            {
                if (idPlanning <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de tournée invalide.";
                    return BadRequest(_response);
                }
                var visites = await _visiteService.GetVisitesByPlanningAsync(idPlanning);
                if (visites == null || !visites.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucune visite trouvée pour ce planning.";
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

        [HttpDelete("{idVisite:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> DeleteVisite(int idVisite)
        {
            try
            {
                if (idVisite <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de visite invalide.";
                    return BadRequest(_response);
                }
                var success = await _visiteService.DeleteVisiteAsync(idVisite);
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

        [HttpPut("{idVisite:int}/planning/{idPlanning:int}")]
        [Authorize(Roles = "DELEGUE")]
        public async Task<IActionResult> AffectVisiteToPlanning(int idVisite, int idPlanning)
        {
            try
            {
                if (idVisite <= 0 )
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de visite invalide.";
                    return BadRequest(_response);
                }
                if (idPlanning <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de planning invalide.";
                    return BadRequest(_response);
                }
                var success = await _visiteService.AffectVisiteToPlanningAsync(idVisite, idPlanning);
                if (!success)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Affectation impossible (visite ou planning invalide).";
                    return NotFound(_response);
                }
                _response.Message = "Visite affectée au planning avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Une erreur est survenue : {ex.Message}";
                return StatusCode(500, _response);
            }
        }
        [HttpPut("{idVisite:int}/complete")]
        [Authorize(Roles = "DELEGUE")]
        public async Task<IActionResult> CompleteVisite(int idVisite)
        {
            try
            {
                if (idVisite <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de visite invalide.";
                    return BadRequest(_response);
                }
                var success = await _visiteService.CompleteVisiteAsync(idVisite);
                if (!success)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Impossible de clôturer la visite (rapport manquant ou visite introuvable).";
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
