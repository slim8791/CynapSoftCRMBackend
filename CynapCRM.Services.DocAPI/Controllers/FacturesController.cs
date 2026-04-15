using CynapCRM.Services.DocAPI.Models.Dto;
using CynapCRM.Services.DocAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.DocAPI.Controllers
{
    [Route("api/facture")]
    [ApiController]
    public class FacturesController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        protected ResponseDto _response;

        public FacturesController(IDocumentService documentService)
        {
            _documentService = documentService;
            _response = new ResponseDto();
        }
        [HttpGet("{id:int")]
        public async Task<IActionResult> GetFactureById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "id invalide.";
                    return BadRequest();
                }
                var facture = await _documentService.GetFactureByIdAsync(id);
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
                return StatusCode(500, _response);
            }
        }
        [HttpGet("client/{idClient:int}")]
        public async Task<IActionResult> GetFacturesByClient(int idClient)
        {
            try
            {
                if (idClient <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "id invalide.";
                    return BadRequest();
                }
                var factures = await _documentService.GetFacturesByClientAsync(idClient);
                if (factures == null || !factures.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucune facutre trouvée pour ce client.";
                    return NotFound(_response);
                }
                _response.Result = factures;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        // 3. Créer ou Mettre à jour une facture
        [HttpPost("createUpdate")]
        public async Task<IActionResult> CreateUpdateFacture([FromBody] FactureDto factureDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données invalides.";
                    return BadRequest(_response);
                }

                var result = await _documentService.CreateUpdateFactureAsync(factureDto);
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
                return StatusCode(500, _response);
            }
        }
    }
}