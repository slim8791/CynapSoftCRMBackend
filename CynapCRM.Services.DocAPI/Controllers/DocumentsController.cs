using Azure;
using CynapCRM.Services.DocAPI.Models.Dto;
using CynapCRM.Services.DocAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.DocAPI.Controllers
{
    // ═══════════════════════════════════════
    // DocumentsController.cs
    // ═══════════════════════════════════════

    [Route("api/documents")]
    [ApiController]
    [Authorize]
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
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetAllDocuments(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _documentService.GetAllDocumentsAsync(pageNumber, pageSize);
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

        [HttpGet("{numeroDoc:int}")]
        // FIX: ajout rôles CLIENT
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]
        public async Task<IActionResult> GetDocumentById(int numeroDoc)
        {
            try
            {
                var doc = await _documentService.GetDocumentByIdAsync(numeroDoc);
                if (doc == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Document introuvable.";
                    return NotFound(_response);
                }
                _response.Result = doc;
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
        public async Task<IActionResult> GetDocumentsByClient(int idClient)
        {
            try
            {
                var docs = await _documentService.GetDocumentsByClientAsync(idClient);
                _response.Result = docs;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpGet("commande/{idCommande:int}")]
        // FIX: ajout rôles CLIENT
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]
        public async Task<IActionResult> GetDocumentsByCommande(int idCommande)
        {
            try
            {
                var docs = await _documentService.GetDocumentsByCommandeAsync(idCommande);
                _response.Result = docs;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        // FIX: endpoint manquant — filtre par type
        [HttpGet("type/{typeDocument}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]
        public async Task<IActionResult> GetDocumentsByType(
            string typeDocument,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(typeDocument))
                {
                    _response.IsSuccess = false;
                    _response.Message = "Type de document invalide.";
                    return BadRequest(_response);
                }
                var docs = await _documentService.GetDocumentsByTypeAsync(
                    typeDocument, pageNumber, pageSize);
                _response.Result = docs;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        // FIX: endpoint manquant — filtre par client + type
        [HttpGet("client/{idClient:int}/type/{typeDocument}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]
        public async Task<IActionResult> GetDocumentsByClientAndType(
            int idClient,
            string typeDocument)
        {
            try
            {
                if (idClient <= 0 || string.IsNullOrWhiteSpace(typeDocument))
                {
                    _response.IsSuccess = false;
                    _response.Message = "Paramètres invalides.";
                    return BadRequest(_response);
                }
                var docs = await _documentService.GetDocumentsByClientAndTypeAsync(
                    idClient, typeDocument);
                _response.Result = docs;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        // FIX: route cohérente createUpdate
        [HttpPost("createUpdate")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> CreateUpdateDocument([FromBody] DocumentDto docDto)
        {
            try
            {
                if (!ModelState.IsValid || docDto == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données du document invalides.";
                    return BadRequest(_response);
                }
                var result = await _documentService.CreateOrUpdateDocumentAsync(docDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur lors de la création/mise à jour du document.";
                    return BadRequest(_response);
                }
                _response.Result = result;
                _response.Message = "Document créé/mis à jour avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        [HttpDelete("{numeroDoc:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteDocument(int numeroDoc)
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
                    _response.Message = "Suppression impossible. Document introuvable.";
                    return NotFound(_response);
                }
                _response.Message = "Document supprimé avec succès.";
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