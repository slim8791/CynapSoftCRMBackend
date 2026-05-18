using CynapCRM.Services.OrderAPI.Models;
using CynapCRM.Services.OrderAPI.Models.Dto;
using CynapCRM.Services.OrderAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CynapCRM.Services.OrderAPI.Controllers
{
    // ═══════════════════════════════════════
    // OrderController.cs
    // ═══════════════════════════════════════

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

        // FIX: ajout filtres statut + dates + rôles CLIENT
        [HttpGet]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetAllOrders(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] EtatCommande? statut = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                if (page <= 0 || pageSize <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Paramètres de pagination invalides.";
                    return BadRequest(_response);
                }

                _response.Result = await _orderService
                    .GetAllOrdersAsync(page, pageSize, statut, startDate, endDate);
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        // FIX: ajout rôles CLIENT pour voir leurs propres commandes
        [HttpGet("{orderId:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]
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
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        // FIX: ajout rôles CLIENT + pagination
        [HttpGet("by-client/{clientId:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]
        public async Task<IActionResult> GetOrdersByClientId(
            int clientId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (clientId <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de client invalide.";
                    return BadRequest(_response);
                }
                var result = await _orderService
                    .GetOrdersByClientIdAsync(clientId, page, pageSize);
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

        // FIX: endpoint manquant — filtre par statut
        [HttpGet("by-status")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]
        public async Task<IActionResult> GetOrdersByStatus(
            [FromQuery] EtatCommande statut,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _orderService
                    .GetOrdersByStatusAsync(statut, page, pageSize);
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

        // FIX: endpoint manquant — filtre par plage de dates
        [HttpGet("by-date")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetOrdersByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (startDate > endDate)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Date de début doit être antérieure à la date de fin.";
                    return BadRequest(_response);
                }
                var result = await _orderService
                    .GetOrdersByDateRangeAsync(startDate, endDate, page, pageSize);
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

        // FIX: endpoint manquant — dashboard KPIs
        [HttpGet("dashboard")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetOrdersDashboard()
        {
            try
            {
                var result = await _orderService.GetOrdersDashboardAsync();
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

        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE,PHARMACIEN,GROSSISTE,CLIENT")]
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

                var clientIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (clientIdClaim == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Identité du client introuvable.";
                    return Unauthorized(_response);
                }

                orderDto.Id_Client = int.Parse(clientIdClaim.Value);

                var result = await _orderService.CreateOrderAsync(orderDto);
                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Échec de la création de la commande.";
                    return BadRequest(_response);
                }
                _response.Result = result;
                _response.Message = "Commande créée avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(515, _response);
            }
        }

        // FIX: ajout restriction de rôle + message précis sur transition invalide
        [HttpPut("status")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> UpdateOrderStatus(
            [FromBody] UpdateOrderStatusDto statusDto)
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
                    _response.Message = "Transition de statut invalide ou commande introuvable.";
                    return BadRequest(_response); // FIX: BadRequest plus approprié que NotFound
                }
                _response.Message = "Statut mis à jour avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
            }
        }

        // FIX: endpoint annulation dédié pour CLIENT
        [HttpPut("{idCommande:int}/cancel")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,PHARMACIEN,GROSSISTE,CLIENT")]
        public async Task<IActionResult> CancelOrder(
            int idCommande,
            [FromQuery] string? motif = null)
        {
            try
            {
                if (idCommande <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de commande invalide.";
                    return BadRequest(_response);
                }
                bool result = await _orderService.CancelOrderAsync(idCommande, motif ?? "");
                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Annulation impossible (commande livrée ou déjà annulée).";
                    return BadRequest(_response);
                }
                _response.Message = "Commande annulée avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(515, _response);
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
                bool isDeleted = await _orderService.DeleteOrderAsync(idCommande);
                if (!isDeleted)
                {
                    _response.IsSuccess = false;
                    // FIX: message précis — règle métier
                    _response.Message = "Suppression impossible (commande non supprimable ou introuvable).";
                    return BadRequest(_response);
                }
                _response.Message = "Commande supprimée avec succès.";
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
