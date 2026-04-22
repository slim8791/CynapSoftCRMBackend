using CynapCRM.Services.OrderAPI.Models;
using CynapCRM.Services.OrderAPI.Models.Dto;
using CynapCRM.Services.OrderAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CynapCRM.Services.OrderAPI.Controllers
{
    [Route("api/orders")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        protected ResponseDto _response;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
            _response = new();
        }


        [HttpGet]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetAllOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {

                if (page <= 0 || pageSize <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Paramètres de pagination invalides.";
                    return BadRequest(_response);
                }

                _response.Result = await _orderService.GetAllOrdersAsync(page, pageSize);
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "Erreur lors de la récupération : " + ex.Message;
                return StatusCode(500, _response);
            }

        }


        [HttpGet("{orderId:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            try
            {
                if (orderId <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de commande invalide.";
                    return BadRequest(_response);
                }
                var result = await _orderService.GetOrderByIdAsync(orderId);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Commande introuvable.";
                    return NotFound(_response);
                }
                _response.Result = result;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "Erreur technique : " + ex.Message;
                return StatusCode(500, _response);
            }
        }

        [HttpGet("by-client/{clientId:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetOrdersByClientId(int clientId)
        {
            try
            {
                if (clientId <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de client invalide.";
                    return BadRequest(_response);
                }
                var result = await _orderService.GetOrdersByClientIdAsync(clientId);
                if (result == null || !result.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "Aucune commande trouvée pour ce client.";
                    return NotFound(_response);
                }
                _response.Result = result;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "Erreur technique : " + ex.Message;
                return StatusCode(500, _response);
            }
        }

        [HttpPost]
        [Authorize(Roles = "CLIENT")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto orderDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données invalides.";
                    return BadRequest(_response);
                }
                // ID CLIENT depuis le JWT (Pharmacien ou Grossiste)
                var clientIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (clientIdClaim == null)
                    return Unauthorized("Identité du client introuvable.");

                orderDto.Id_Client = int.Parse(clientIdClaim.Value);

                var result = await _orderService.CreateOrderAsync(orderDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Échec de la création de la commande.";
                    return BadRequest(_response);
                }
                _response.Message = "Commande créée avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, _response);
            }
        }
        [HttpPut("status")]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusDto statusDto)
        {
            try
            {
               if (!ModelState.IsValid)
               {
                _response.IsSuccess = false;
                _response.Message = "Données invalides.";
                return BadRequest(_response);
                }
                bool isUpdated = await _orderService.UpdateOrderStatusAsync(statusDto);
                if (!isUpdated)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Commande introuvable pour la mise à jour du statut.";
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

        [HttpDelete("{idCommande:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteOrder(int idCommande)
        {
            try
            {
                if (idCommande <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de commande invalide.";
                    return BadRequest(_response);
                }
                bool IsDeleted = await _orderService.DeleteOrderAsync(idCommande);
                if (!IsDeleted)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Commande introuvable.";
                    return NotFound(_response);
                }
                _response.Result = true;
                _response.Message = "Commande supprimée avec succès.";
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
