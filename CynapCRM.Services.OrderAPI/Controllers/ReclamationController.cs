using Azure;
using CynapCRM.Services.OrderAPI.Models.Dto;
using CynapCRM.Services.OrderAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CynapCRM.Services.OrderAPI.Controllers
{
    [Route("api/reclamation")]
    [ApiController]
    public class ReclamationController : ControllerBase
    {
        private readonly IOrderService _orderService;
        protected ResponseDto _response;
        public ReclamationController(IOrderService orderService)
        {
            _orderService = orderService;
            _response = new();

        }
        [HttpGet]
        public async Task<IActionResult> GetAllReclamations()
        {
            try
            {
                var result = await _orderService.GetAllReclamationsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "Erreur lors de la récupération : " + ex.Message;
                return StatusCode(500, _response);
            }
        }
        [HttpGet("order/{orderId:int}")]
        public async Task<IActionResult> GetReclamationsByOrder(int orderId)
        {
            try
            {
                if (orderId <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de commande invalide.";
                    return BadRequest(_response);
                }
                var result = await _orderService.GetReclamationsByOrderAsync(orderId);
                if (result == null || !result.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucune réclamation trouvée pour cette commande.";
                    return NotFound(_response);
                }
                _response.Result = result;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "Erreur lors de la récupération : " + ex.Message;
                return StatusCode(500, _response);
            }
        }
        [HttpGet("client/{idClient:int}")]
        public async Task<IActionResult> GetReclamationsByClient(int idClient)
        {
            try
            {
                if (idClient <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de client invalide.";
                    return BadRequest(_response);
                }
                var result = await _orderService.GetReclamationsByClientAsync(idClient);
                if (result == null || !result.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucune réclamation trouvée pour ce client.";
                    return NotFound(_response);
                }
                _response.Result = result;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "Erreur lors de la récupération : " + ex.Message;
                return StatusCode(500, _response);
            }
        }
        [HttpGet("{idReclamation:int}")]
        public async Task<IActionResult> GetReclamationById(int idReclamation)
        {
            try
            {
                if (idReclamation <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de réclamation invalide.";
                    return BadRequest(_response);
                }
                var result = await _orderService.GetReclamationByIdAsync(idReclamation);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Réclamation introuvable.";
                    return NotFound(_response);
                }
                _response.Result = result;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "Erreur lors de la récupération : " + ex.Message;
                return StatusCode(500, _response);
            }
        }
        [HttpPost("createUpdate")]
        public async Task<IActionResult> CreateUpdateReclamation([FromBody] ReclamationDto reclamationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données invalides.";
                    return BadRequest(_response);
                }

                var result = await _orderService.CreateUpdateReclamationAsync(reclamationDto);

                if (result == null)
                {
                    _response.IsSuccess = false;
                    if (reclamationDto.Id_Rec == 0)
                    {
                        _response.Message = "Échec de la création. Vérifiez les références de commande et de ligne.";
                    }
                    else
                    {
                        _response.Message = "Réclamation introuvable pour mise à jour.";
                    }

                    return BadRequest(_response);
                }

                _response.Result = result;
                if (reclamationDto.Id_Rec == 0)
                {
                    _response.Message = "Réclamation créée avec succès.";
                }
                else
                {
                    _response.Message = "Réclamation modifiée avec succès.";
                }
                return Ok(_response);

                
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "Erreur lors de l'opération : " + ex.Message;
                return StatusCode(500, _response);
            }
        }
        
        [HttpPost("status")]
        public async Task<IActionResult> UpdateReclamationStatus([FromBody] UpdateOrderStatusDto orderStatus)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données invalides pour la mise à jour du statut.";
                    return BadRequest(_response);
                }

                bool isUpdated = await _orderService.UpdateOrderStatusAsync(orderStatus);
                if (!isUpdated)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Réclamation introuvable pour la mise à jour du statut.";
                    return NotFound(_response);
                }
                _response.Message = "Mise à jour réussie.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "Erreur lors de la modification : " + ex.Message;
                return StatusCode(500, _response);
            }

        }
        [HttpDelete("{idReclamation:int}")]
        public async Task<IActionResult> DeleteReclamation(int idReclamation)
        {
            try
            {
                if (idReclamation <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de réclamation invalide.";
                    return BadRequest(_response);
                }
                bool isDeleted = await _orderService.DeleteReclamationAsync(idReclamation);
                if (!isDeleted)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Réclamation introuvable pour suppression.";
                    return NotFound(_response);
                }
                _response.Message = "Réclamation supprimée avec succès.";
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
