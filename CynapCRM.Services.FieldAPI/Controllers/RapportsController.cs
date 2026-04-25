using CynapCRM.Services.FieldAPI.Models.Dto;
using CynapCRM.Services.FieldAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.FieldAPI.Controllers
{

    [ApiController]
    [Route("api/rapports")]
    [Authorize]

    public class RapportsController : ControllerBase
    {
        private readonly IRapportService _rapportService;
        protected ResponseDto _response;
        public RapportsController(IRapportService rapportService)
        {
            _rapportService = rapportService;
            _response = new ResponseDto();
        }
        [HttpPost("createUpdate")]
        public async Task<IActionResult> CreateOrUpdateRapport([FromBody] RapportVisiteDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de rapport invalides.";
                    return BadRequest(_response);
                }
                var result = await _rapportService.CreateOrUpdateRapportAsync(dto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur lors de la création/mise à jour du rapport.";
                    return BadRequest(_response);
                }
                _response.Result = result;
                _response.Message = "Rapport enregistré avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Une erreur est survenue : {ex.Message}";
                return StatusCode(515, _response);
            }
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetRapportById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de rapport invalide.";
                    return BadRequest(_response);
                }
                var rapport = await _rapportService.GetRapportByIdAsync(id);
                if (rapport == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucun rapport trouvé pour cet ID.";
                    return NotFound(_response);
                }
                _response.Result = rapport;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Une erreur est survenue : {ex.Message}";
                return StatusCode(515, _response);
            }
        }

        [HttpGet("by-visite/{idVisite:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetRapportsByVisiteId(int idVisite)
        {
            try
            {
                if (idVisite <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de visite invalide.";
                    return BadRequest(_response);
                }
                var rapports = await _rapportService.GetRapportByVisiteAsync(idVisite);
                if (rapports == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucun rapport trouvé pour cette visite.";
                    return NotFound(_response);
                }
                _response.Result = rapports;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Une erreur est survenue : {ex.Message}";
                return StatusCode(515, _response);
            }
        }

        [HttpDelete("{idRapport:int}")]
        [Authorize(Roles = "DELEGUE,ADMIN")]
        public async Task<IActionResult> DeleteRapport(int idRapport)
        {
            try
            {
                if (idRapport <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de rapport invalide.";
                    return BadRequest(_response);
                }
                var success = await _rapportService.DeleteRapportAsync(idRapport);
                if (!success)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Échec de la suppression du rapport. Rapport introuvable.";
                    return NotFound(_response);
                }
                _response.Message = "Rapport supprimé avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Une erreur est survenue : {ex.Message}";
                return StatusCode(515, _response);
            }
        }

        [HttpPut("{idRapport:int}/validate")]
        [Authorize(Roles = "SUPERVISEUR")]
        public async Task<IActionResult> ValidateRapport(int idRapport,[FromQuery] int idSuperviseur)
        {
            try
            {
                var result = await _rapportService.ValidateRapportAsync(idRapport, idSuperviseur);

                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message =
                        "Validation impossible (rapport introuvable ou visite invalide).";
                    return BadRequest(_response);
                }

                _response.Message = "Rapport validé et visite clôturée avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }
        [HttpGet("can-create/{idVisite:int}")]
        [Authorize(Roles = "DELEGUE")]
        public async Task<IActionResult> CanCreateRapport(int idVisite)
        {
            try
            {
                var result = await _rapportService.CanCreateRapportAsync(idVisite);

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
        [HttpGet("has-rapport/{idVisite:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> HasRapport(int idVisite)
        {
            try
            {
                var result = await _rapportService.HasRapportAsync(idVisite);

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

        [HttpGet("all")]
        public async Task<IActionResult> GetAllRapports()
        {
            try
            {
                var rapports = await _rapportService.GetAllRapportsAsync();
                if (rapports == null || !rapports.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucun rapport trouvé.";
                    return NotFound(_response);
                }
                _response.Result = rapports;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Une erreur est survenue : {ex.Message}";
                return StatusCode(515, _response);
            }
        }

    }
}
