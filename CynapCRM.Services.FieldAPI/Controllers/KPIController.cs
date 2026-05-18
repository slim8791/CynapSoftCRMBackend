using CynapCRM.Services.FieldAPI.Models.Dto;
using CynapCRM.Services.FieldAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.FieldAPI.Controllers
{

    // ═══════════════════════════════════════
    // KPIController.cs
    // ═══════════════════════════════════════

    [ApiController]
    [Route("api/kpi")]
    [Authorize]
    public class KPIController : ControllerBase
    {
        private readonly IKPIService _kpiService;
        protected ResponseDto _response;

        public KPIController(IKPIService kpiService)
        {
            _kpiService = kpiService;
            _response = new ResponseDto();
        }

        // FIX: ajout DELEGUE — peut voir ses propres stats
        [HttpGet("visites-count")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetNombreVisites(
            [FromQuery] int idDelegue,
            [FromQuery] DateTime debut,
            [FromQuery] DateTime fin)
        {
            try
            {
                var result = await _kpiService.GetNombreVisitesAsync(idDelegue, debut, fin);
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

        [HttpGet("has-visite")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> HasVisiteAtDate(
            [FromQuery] int idDelegue,
            [FromQuery] DateTime date)
        {
            try
            {
                var result = await _kpiService.HasVisiteAtDateAsync(idDelegue, date);
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

        [HttpGet("historique/{idDelegue:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetHistoriqueActivite(int idDelegue)
        {
            try
            {
                var result = await _kpiService.GetHistoriqueActiviteAsync(idDelegue);
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

        [HttpGet("client-fidelite/{idClient:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetClientFidelite(int idClient)
        {
            try
            {
                var result = await _kpiService.CalculateClientFideliteAsync(idClient);
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

        // FIX: ajout DELEGUE — peut voir ses propres performances
        [HttpGet("performance/{idDelegue:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetPerformance(int idDelegue)
        {
            try
            {
                var result = await _kpiService.CalculatePerformanceAsync(idDelegue);
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

        // FIX: ajout DELEGUE — peut voir son propre taux
        [HttpGet("performance-rate/{idDelegue:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetPerformanceRate(int idDelegue)
        {
            try
            {
                var result = await _kpiService.GetPerformanceRateAsync(idDelegue);
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

        // FIX: endpoint manquant
        [HttpGet("taux-conversion/{idDelegue:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR,DELEGUE")]
        public async Task<IActionResult> GetTauxConversion(
            int idDelegue,
            [FromQuery] DateTime debut,
            [FromQuery] DateTime fin)
        {
            try
            {
                var result = await _kpiService.GetTauxConversionAsync(idDelegue, debut, fin);
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
    }
}