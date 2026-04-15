using CynapCRM.Services.DocAPI.Models.Dto;
using CynapCRM.Services.DocAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.DocAPI.Controllers
{
    [Route("api/bonCommande")]
    [ApiController]
    public class BonsCommandesController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        protected ResponseDto _response;

        public BonsCommandesController(IDocumentService documentService)
        {
            _documentService = documentService;
            _response = new ResponseDto();
        }

        // 1. Récupérer un Bon de Commande par son ID technique (Id_BC)
        [HttpGet("{id:int}")]
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
                var bc = await _documentService.GetBonCommandeByIdAsync(id);
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
                return StatusCode(500, _response);
            }
        }

        // 2. Récupérer tous les Bons de Commande d'un client
        [HttpGet("client/{idClient:int}")]
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
                var list = await _documentService.GetBonsCommandeByClientAsync(idClient);
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
                return StatusCode(500, _response);
            }
        }

        // 3. Créer ou Mettre à jour un Bon de Commande
        [HttpPost("createUpdate")]
        public async Task<IActionResult> CreateUpdateBonCommande([FromBody] BonCommandeDto bcDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Les données saisies sont invalides.";
                    return BadRequest(_response);
                }

                var result = await _documentService.CreateUpdateBonCommandeAsync(bcDto);
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
                return StatusCode(500, _response);
            }
        }
    }
}