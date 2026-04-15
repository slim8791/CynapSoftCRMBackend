using Azure;
using CynapCRM.Services.DocAPI.Models.Dto;
using CynapCRM.Services.DocAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.DocAPI.Controllers
{
    [Route("api/document")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        protected ResponseDto _response;
        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
            _response = new();
        }
        [HttpGet]
        public async Task<ActionResult> GetAllDocuments(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var docs = await _documentService.GetAllDocumentsAsync(pageNumber, pageSize);
                if (docs == null || !docs.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucun document trouvé.";
                    return NotFound(_response);
                }
                _response.Result = docs;
                _response.Message = "Documents récupérés avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "Erreur lors de la récupération : " + ex.Message;
                return StatusCode(500, _response);
            }
            
        }

        [HttpGet("{numeroDoc:int}")]
        public async Task<ActionResult> GetDocumentById(int numeroDoc)
        {
            try
            {
                var doc = await _documentService.GetDocumentByIdAsync(numeroDoc);
                if (doc == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Document non trouvé.";
                    return NotFound(_response);
                }
                _response.Result = doc;
                _response.Message = "Document récupéré avec succès.";
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
        public async Task<ActionResult> GetDocumentsByClient(int idClient)
        {
            try
            {
                var docs = await _documentService.GetDocumentsByClientAsync(idClient);
                if (docs == null || !docs.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = $"Aucun document trouvé pour le client {idClient}.";
                    return NotFound(_response);
                }
                _response.Result = docs;
                _response.Message = "Documents récupérés avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
            
        }
        [HttpGet("commande/{idCommande:int}")]
        public async Task<ActionResult> GetDocumentsByCommande(int idCommande)
        {
            try
            {
                var docs = await _documentService.GetDocumentsByCommandeAsync(idCommande);
                if (docs == null || !docs.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = $"Aucun document trouvé pour la commande {idCommande}.";
                    return NotFound(_response);
                }
                _response.Result = docs;
                _response.Message = "Documents récupérés avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);

            }

        }
        [HttpPost("document")]
        public async Task<ActionResult> CreateUpdateDocument([FromBody] DocumentDto docDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données du document invalides.";
                    return BadRequest(_response);
                }
                if (docDto == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données du document invalides.";
                    return BadRequest(_response);
                }
                var result = await _documentService.CreateUpdateDocumentAsync(docDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur lors de la création/mise à jour du document.";
                    return StatusCode(500, _response);
                }
                _response.Result = result;
                _response.Message = "Document créé/mis à jour avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
                    }
        [HttpDelete("{numeroDoc:int}")]
        public async Task<ActionResult> DeleteDocument(int numeroDoc)
        {
            try
            {
                if (numeroDoc <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Numéro de document invalide.";
                    return BadRequest(_response);
                }
                bool isDeleted = await _documentService.DeleteDocumentAsync(numeroDoc);
                if (!isDeleted)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Document introuvable pour suppression.";
                    return NotFound(_response);
                }
                _response.Message = "Document supprimé avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "Erreur lors de la suppression : " + ex.Message;
                return StatusCode(500, _response);
            }

        }
    }
}
