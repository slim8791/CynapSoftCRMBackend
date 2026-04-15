using CynapCRM.Services.DocAPI.Models.Dto;
using CynapCRM.Services.DocAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.DocAPI.Controllers
{
    [Route("api/bonLivraison")]
    [ApiController]
    public class BonsLivraisonsController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        protected ResponseDto _response;

        public BonsLivraisonsController(IDocumentService documentService)
        {
            _documentService = documentService;
            _response = new ResponseDto();
        }

        // 1. Récupérer un Bon de Livraison par son ID technique (Id_BL)
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBonLivraisonById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "id invalide.";
                    return BadRequest();
                }
                var bl = await _documentService.GetBonLivraisonByIdAsync(id);
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
                return StatusCode(500, _response);
            }
        }

        // 2. Récupérer tous les Bons de Livraison d'un client spécifique
        [HttpGet("ByClient/{idClient:int}")]
        public async Task<IActionResult> GetBonsLivraisonByClient(int idClient)
        {
            try
            {
                if (idClient <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "id invalide.";
                    return BadRequest();
                }
                var list = await _documentService.GetBonsLivraisonByClientAsync(idClient);
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

        // 3. Créer ou Mettre à jour un Bon de Livraison
        [HttpPost("createUpdate")]
        public async Task<IActionResult> CreateUpdateBonLivraison([FromBody] BonLivraisonDto blDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Les données du bon de livraison sont incorrectes.";
                    return BadRequest(_response);
                }

                var result = await _documentService.CreateUpdateBonLivraisonAsync(blDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Une erreur est survenue lors du traitement du bon de livraison.";
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
                return StatusCode(500, _response);
            }
        }
    }
}
        
