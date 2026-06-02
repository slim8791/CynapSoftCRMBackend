using CynapCRM.Services.DocAPI.Models.Dto;
using CynapCRM.Services.DocAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.DocAPI.Controllers
{

    [Route("api/bons-livraison")]
    [ApiController]
    [Authorize]
    public class BonsLivraisonsController : ControllerBase
    {
        private readonly IBLService _bLService;
        protected ResponseDto _response;

        public BonsLivraisonsController(IBLService bLService)
        {
            _bLService = bLService;
            _response = new ResponseDto();
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetAllBonsLivraison(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (pageNumber <= 0 || pageSize <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Paramètres de pagination invalides.";
                    return BadRequest(_response);
                }
                var result = await _bLService.GetAllBonsLivraisonAsync(pageNumber, pageSize);
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
        public async Task<IActionResult> GetBonLivraisonById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id invalide.";
                    return BadRequest(_response);
                }
                var bl = await _bLService.GetBonLivraisonByIdAsync(id);
                if (bl == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Bon de livraison introuvable.";
                    return NotFound(_response);
                }
                _response.Result = bl;
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
        public async Task<IActionResult> GetBonsLivraisonByClient(int idClient)
        {
            try
            {
                if (idClient <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id invalide.";
                    return BadRequest(_response);
                }
                var list = await _bLService.GetBonsLivraisonByClientAsync(idClient);
                _response.Result = list;
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
        public async Task<IActionResult> GetBonsLivraisonByCommande(int idCommande)
        {
            try
            {
                if (idCommande <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id invalide.";
                    return BadRequest(_response);
                }
                var list = await _bLService.GetBonsLivraisonByCommandeAsync(idCommande);
                _response.Result = list;
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
        public async Task<IActionResult> GetBonsLivraisonByDate(
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
                var result = await _bLService.GetBonsLivraisonByDateAsync(startDate, endDate);
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
        public async Task<IActionResult> CreateOrUpdateBonLivraison(
            [FromBody] BonLivraisonDto blDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Les données du bon de livraison sont incorrectes.";
                    return BadRequest(_response);
                }
                var result = await _bLService.CreateOrUpdateBonLivraisonAsync(blDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur lors du traitement du bon de livraison.";
                    return BadRequest(_response);
                }
                _response.Result = result;
                _response.Message = "Bon de livraison enregistré avec succès.";
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
        public async Task<IActionResult> DeleteBonLivraison(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Id invalide.";
                    return BadRequest(_response);
                }
                var result = await _bLService.DeleteBonLivraisonAsync(id);
                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Bon de livraison introuvable.";
                    return NotFound(_response);
                }
                _response.Message = "Bon de livraison supprimé avec succès.";
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