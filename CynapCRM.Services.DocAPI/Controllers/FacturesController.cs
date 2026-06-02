using CynapCRM.Services.DocAPI.Models.Dto;
using CynapCRM.Services.DocAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.DocAPI.Controllers
{
    [Route("api/factures")]
    [ApiController]
    [Authorize]
    public class FacturesController : ControllerBase
    {
        private readonly IFactureService _factureService;
        protected ResponseDto _response;

        public FacturesController(IFactureService factureService)
        {
            _factureService = factureService;
            _response = new ResponseDto();
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetAllFactures(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _factureService.GetAllFacturesAsync(pageNumber, pageSize);
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

        [HttpGet("{id:int}")]
        // FIX: ajout rôles CLIENT
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]
        public async Task<IActionResult> GetFactureById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id invalide.";
                    return BadRequest(_response);
                }
                var facture = await _factureService.GetFactureByIdAsync(id);
                if (facture == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Facture introuvable.";
                    return NotFound(_response);
                }
                _response.Result = facture;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpGet("client/{idClient:int}")]
        // FIX: ajout rôles CLIENT
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]
        public async Task<IActionResult> GetFacturesByClient(int idClient)
        {
            try
            {
                if (idClient <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id invalide.";
                    return BadRequest(_response);
                }
                var factures = await _factureService.GetFacturesByClientAsync(idClient);
                _response.Result = factures;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        // FIX: endpoint manquant
        [HttpGet("commande/{idCommande:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]
        public async Task<IActionResult> GetFacturesByCommande(int idCommande)
        {
            try
            {
                if (idCommande <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id invalide.";
                    return BadRequest(_response);
                }
                var factures = await _factureService.GetFacturesByCommandeAsync(idCommande);
                _response.Result = factures;
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
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetFacturesByDate(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    _response.IsSuccess = false;
                    _response.Message = "La date de début doit être antérieure à la date de fin.";
                    return BadRequest(_response);
                }
                var result = await _factureService.GetFacturesByDateAsync(startDate, endDate);
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

        [HttpPost("createUpdate")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> CreateOrUpdateFacture(
            [FromBody] FactureDto factureDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données invalides.";
                    return BadRequest(_response);
                }
                var result = await _factureService.CreateOrUpdateFactureAsync(factureDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur lors de l'enregistrement de la facture.";
                    return BadRequest(_response);
                }
                _response.Result = result;
                _response.Message = "Facture enregistrée avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        // FIX: endpoint manquant
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteFacture(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id invalide.";
                    return BadRequest(_response);
                }
                var result = await _factureService.DeleteFactureAsync(id);
                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Facture introuvable.";
                    return NotFound(_response);
                }
                _response.Message = "Facture supprimée avec succès.";
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