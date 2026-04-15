using CynapCRM.Services.OrderAPI.Models;
using CynapCRM.Services.OrderAPI.Models.Dto;
using CynapCRM.Services.OrderAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.OrderAPI.Controllers
{
    [Route("api/order")]
    [ApiController]
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
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                _response.Result = await _orderService.GetAllOrdersAsync();
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "Erreur lors de la récupération : " + ex.Message;
                return StatusCode(500, _response);
            }

        }

        [HttpGet("{idCommande:int}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de commande invalide.";
                    return BadRequest(_response);
                }
                var result = await _orderService.GetOrderByIdAsync(id);
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
        [HttpGet("client/{idClient:int}")]
        public async Task<IActionResult> GetOrdersByClientId(int idClient)
        {
            try
            {
                if (idClient <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de client invalide.";
                    return BadRequest(_response);
                }
                var result = await _orderService.GetOrdersByClientIdAsync(idClient);
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
        [HttpPost("create")]
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
                _response.Message = "Échec de la création : " + ex.Message;
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
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID de commande invalide.";
                    return BadRequest(_response);
                }
                bool IsDeleted = await _orderService.DeleteOrderAsync(id);
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
