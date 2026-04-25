using CynapCRM.Services.DocAPI.Models.Dto;
using CynapCRM.Services.DocAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.DocAPI.Controllers
{
    [Route("api/bons-commandes")]
    [ApiController]
    [Authorize]

    public class BonsCommandesController : ControllerBase
    {
        private readonly IBCService _bCService;
        protected ResponseDto _response;
        public BonsCommandesController(IBCService bCService)
        {
            _bCService = bCService;
            _response = new ResponseDto();
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetAllBonsCommande([FromQuery] int pageNumber = 1,
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

                var result = await _bCService
                    .GetAllBonsCommandeAsync(pageNumber, pageSize);

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
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetBonCommandeById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "id invalide.";
                    return BadRequest();
                }
                var bc = await _bCService.GetBonCommandeByIdAsync(id);
                if (bc == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Bon de commande introuvable.";
                    return NotFound(_response);
                }
                _response.Result = bc;
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

        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]

        public async Task<IActionResult> GetBonsCommandeByClient(int idClient)
        {
            try
            {
                if (idClient < 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "id invalide.";
                    return BadRequest();
                }
                var list = await _bCService.GetBonsCommandeByClientAsync(idClient);
                if (list == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucune bon commande trouvée pour ce client.";
                    return NotFound(_response);
                }
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
        public async Task<IActionResult> GetBonsCommandeByDate([FromQuery] DateTime startDate,
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

                var result = await _bCService
                    .GetBonsCommandeByDateAsync(startDate, endDate);

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

        public async Task<IActionResult> CreateOrUpdateBonCommande([FromBody] BonCommandeDto bcDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Les données saisies sont invalides.";
                    return BadRequest(_response);
                }

                var result = await _bCService.CreateOrUpdateBonCommandeAsync(bcDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur lors de l'enregistrement du bon de commande.";
                    return BadRequest(_response);
                }

                _response.Result = result;
                _response.Message = "Bon de commande traité avec succès.";
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