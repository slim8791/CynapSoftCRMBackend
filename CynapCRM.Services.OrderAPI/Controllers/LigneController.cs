using Azure;
using CynapCRM.Services.OrderAPI.Models;
using CynapCRM.Services.OrderAPI.Models.Dto;
using CynapCRM.Services.OrderAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.OrderAPI.Controllers
{
    [Route("api/ligne")]
    [ApiController]
    public class LigneController : ControllerBase
    {
        private readonly IOrderService _orderService;
        protected ResponseDto _response;
        public LigneController(IOrderService orderService)
        {
            _orderService = orderService;
            _response = new();
        }
        [HttpPost("createUpdate")]
        public async Task<IActionResult> CreateUpdateLigneCommande([FromBody] LigneCommandeDto ligneDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de ligne de commande invalides.";
                    return BadRequest(_response);
                }

                var result = await _orderService.CreateUpdateLigneCommandeAsync(ligneDto);

                if (result == null)
                {
                    _response.IsSuccess = false;

                    if (ligneDto.Id_Ligne == 0)
                    {
                        _response.Message = "Échec de la création. Vérifiez la commande associée.";
                    }
                    else
                    {
                        _response.Message = "Ligne de commande introuvable pour mise à jour.";
                    }

                    return BadRequest(_response);
                }

                _response.Result = result;
                if (ligneDto.Id_Ligne == 0)
                {
                    _response.Message = "Ligne de commande créée avec succès.";
                }
                else
                {
                    _response.Message = "Ligne de commande modifiée avec succès.";
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
        [HttpDelete("ligne/{id:int}")]
         public async Task<IActionResult> DeleteLigneCommande(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "ID ligne commande invalide.";
                    return BadRequest(_response);
                }
                bool isDeleted = await _orderService.RemoveLigneCommandeAsync(id);
                if (!isDeleted)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Ligne de commande introuvable pour suppression.";
                    return NotFound(_response);
                }
                _response.Message = "Ligne de commande supprimée avec succès.";
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
