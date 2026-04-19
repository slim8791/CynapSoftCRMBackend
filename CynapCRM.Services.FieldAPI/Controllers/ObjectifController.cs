using CynapCRM.Services.FieldAPI.Models.Dto;
using CynapCRM.Services.FieldAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.FieldAPI.Controllers
{

    [ApiController]
    [Route("api/objectifs")]
    [Authorize]

    public class ObjectifController : Controller
    {

        private readonly IObjectifService _objectifService;
        protected ResponseDto _response;

        public ObjectifController(IObjectifService objectifService)
        {
            _objectifService = objectifService;
            _response = new ResponseDto();
        }

        // ==================================================
        // ✅ CREATE / UPDATE OBJECTIF
        // ==================================================
        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> CreateOrUpdateObjectif(
            [FromBody] ObjectifDelegueDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Données de l’objectif invalides.";
                    return BadRequest(_response);
                }

                var result = await _objectifService.CreateOrUpdateObjectifAsync(dto);

                if (result == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Impossible de créer ou modifier l’objectif.";
                    return BadRequest(_response);
                }

                _response.Result = result;
                _response.Message = "Objectif enregistré avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        // ==================================================
        // ✅ GET OBJECTIFS BY DÉLÉGUÉ
        // ==================================================
        [HttpGet("by-delegue/{idDelegue:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> GetObjectifsByDelegue(int idDelegue)
        {
            try
            {
                var result = await _objectifService.GetObjectifsByDelegueAsync(idDelegue);

                _response.Result = result;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        // ==================================================
        // ✅ UPDATE OBJECTIF VALUE
        // ==================================================
        [HttpPut("{idObjectif:int}/value")]
        [Authorize(Roles = "ADMIN,SUPERVISEUR")]
        public async Task<IActionResult> UpdateObjectifValue(
            int idObjectif,
            [FromQuery] int nouvelleValeur)
        {
            try
            {
                var result = await _objectifService.UpdateObjectifValueAsync(idObjectif, nouvelleValeur);

                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Mise à jour de la valeur impossible.";
                    return BadRequest(_response);
                }

                _response.Message = "Valeur de l’objectif mise à jour avec succès.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }
        }

        // ==================================================
        // ✅ DELETE OBJECTIF
        // ==================================================
        [HttpDelete("{idObjectif:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteObjectif(int idObjectif)
        {
            try
            {
                var result = await _objectifService.DeleteObjectifAsync(idObjectif);

                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Suppression impossible (objectif introuvable).";
                    return BadRequest(_response);
                }

                _response.Message = "Objectif supprimé avec succès.";
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

