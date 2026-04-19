using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.InventoryAPI.Controllers
{

    [Route("api/inventory/business")]
    [ApiController]
    [Authorize]

    public class InventoryBusinessController : ControllerBase
    {
        private readonly IInventoryBusinessService _inventoryBusinessService;
        protected ResponseDto _response;

        public InventoryBusinessController(IInventoryBusinessService inventoryBusinessService)
        {
            _inventoryBusinessService = inventoryBusinessService;
            _response = new ResponseDto();
        }

        // 5. Disponibilité rapide (pour vérification UI avant validation)

        [HttpGet("check-availability")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> CheckStockAvailability([FromQuery] int idStock, [FromQuery] int qte)
        {
            try
            {
                if (idStock <= 0 || qte <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de vérification invalides.";
                    return BadRequest(_response);
                }
                bool available = await _inventoryBusinessService.CheckStockAvailabilityAsync(idStock, qte);
                if (!available)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Stock disponible insuffisant pour la quantité demandée.";
                    return BadRequest(_response);
                }
                _response.Result = available;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        [HttpPost("distribute-echantillon")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> DistributeEchantillon(
                    [FromQuery] int idDelegue,
                    [FromQuery] int idPharmacien,
                    [FromQuery] int idMedecin,
                    [FromQuery] int idStock,
                    [FromQuery] int qte)
        {
            try
            {
                if (idDelegue <= 0 || idPharmacien <= 0 || idMedecin <= 0 || idStock <= 0 || qte <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de distribution invalides.";
                    return BadRequest(_response);
                }
                var result = await _inventoryBusinessService
                    .DistributeEchantillonAsync(
                        idDelegue,
                        idPharmacien,
                        idMedecin,
                        idStock,
                        qte);

                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Distribution impossible (stock insuffisant ou données invalides).";
                    return BadRequest(_response);
                }

                _response.Message = "Distribution effectuée avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }



        // 3. Appliquer la règle de gratuité (Promotion)

        [HttpPost("apply-gratuite")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> ApplyGratuite(
                    [FromQuery] int idStock,
                    [FromQuery] int quantiteAchetee,
                    [FromQuery] int seuilPromo)
        {

            try
            {
                if (idStock <= 0 || quantiteAchetee <= 0 || seuilPromo <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de promotion invalides.";
                    return BadRequest(_response);
                }
                bool result = await _inventoryBusinessService.ApplyGratuiteAsync(idStock, quantiteAchetee, seuilPromo   );
                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Erreur lors de l'application de la gratuité.";
                    return BadRequest(_response);
                }
                _response.Message = "Gratuité calculée et ajoutée au stock.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }
        // 2. Réserver du stock (bloquer une quantité)

        [HttpPost("reserve-stock")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> ReserveStock(
                    [FromQuery] int idStock,
                    [FromQuery] int quantite)
        {

            try
            {
               if (idStock <= 0 || quantite <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de réservation invalides.";
                    return BadRequest(_response);
                }

                bool result = await _inventoryBusinessService.ReserveStockAsync(idStock, quantite);

                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Réservation impossible : Stock disponible insuffisant.";
                    return BadRequest(_response);
                }
                _response.Message = "Quantité réservée avec succès.";
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
