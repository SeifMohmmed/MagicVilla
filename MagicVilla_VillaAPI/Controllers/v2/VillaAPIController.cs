using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace MagicVilla_VillaAPI.Controllers.v2
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("2.0")]
    public class VillaAPIController : ControllerBase
    {
        private readonly APIResponse _response;
        private readonly IVillaRepository _dbVilla;
        private readonly IMapper _mapper;

        public VillaAPIController(IVillaRepository dbVilla, IMapper mapper)
        {
            _dbVilla = dbVilla;
            _mapper = mapper;
            _response = new();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        //[ResponseCache(CacheProfileName = "Default30")]
        public async Task<ActionResult<APIResponse>> GetVillas([FromQuery(Name = "filterOccupancy")] int? occupancy
            , [FromQuery] string? search, int pageSize = 0, int pageNumber = 1)
        {
            try
            {
                IEnumerable<Villa> villaList;

                if (occupancy > 0)
                {
                    villaList = await _dbVilla.GetAllAsync(v => v.Occupancy == occupancy, pageSize: pageSize, pageNumber: pageNumber);
                }
                else
                {
                    villaList = await _dbVilla.GetAllAsync(pageSize: pageSize, pageNumber: pageNumber);
                }
                if (!string.IsNullOrEmpty(search))
                {
                    villaList = villaList.Where(v => v.Name.ToLower().Contains(search));
                }
                Pagination pagination = new() { PageNumber = pageNumber, PageSize = pageSize };

                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(pagination));

                _response.Result = _mapper.Map<List<VillaDTO>>(villaList);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages =
                    new List<string>() { ex.ToString() };
            }
            return _response;
        }
        [HttpGet("{id:int}", Name = "GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        //[ResponseCache(CacheProfileName = "Default30")]
        public async Task<ActionResult<APIResponse>> GetVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    return BadRequest(_response);
                }
                var villa = await _dbVilla.GetAsync(v => v.Id == id);

                if (villa == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    return NotFound(_response);
                }
                _response.Result = _mapper.Map<VillaDTO>(villa);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages =
                    new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<APIResponse>> AddVilla([FromForm] VillaCreateDTO createVillaDTO)
        {
            try
            {
                if (await _dbVilla.GetAsync(v => v.Name.ToLower() == createVillaDTO.Name.ToLower()) != null)
                {
                    ModelState.AddModelError("ErrorMessages", "Villa Already Exists!");
                    return BadRequest(ModelState);
                }

                if (createVillaDTO == null)
                    return BadRequest(createVillaDTO);

                Villa villa = _mapper.Map<Villa>(createVillaDTO);
                #region Before Using AutoMapper
                //Villa model = new()
                //{
                //    Name = createVillaDTO.Name,
                //    Amenity = createVillaDTO.Amenity,
                //    Details = createVillaDTO.Details,
                //    Rate = createVillaDTO.Rate,
                //    Sqft = createVillaDTO.Sqft,
                //    Occupancy = createVillaDTO.Occupancy,
                //    ImageURL = createVillaDTO.ImageUrl,
                //};

                #endregion

                await _dbVilla.CreateAsync(villa);

                if(createVillaDTO.Image!=null)
                {
                    var fileName = villa.Id + Path.GetExtension(createVillaDTO.Image.FileName);
                    string filePath = @"wwwroot\ProductImage\" + fileName;

                    var directoryLocation = Path.Combine(Directory.GetCurrentDirectory(),filePath);

                    FileInfo file = new FileInfo(directoryLocation);

                    if (file.Exists)
                    {
                        file.Delete();
                    }

                    using(var fileStream = new FileStream(directoryLocation,FileMode.Create))
                    {
                        createVillaDTO.Image.CopyTo(fileStream);
                    }

                    var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";

                    villa.ImageUrl = baseUrl+"/ProductImage/"+fileName;
                    villa.ImageLocalPath = filePath;
                }
                else
                {
                    villa.ImageUrl = "https://placehold.co/600x400";
                }

                await _dbVilla.UpdateAsync(villa);

                _response.Result = _mapper.Map<VillaDTO>(villa);
                _response.StatusCode = HttpStatusCode.Created;

                return CreatedAtAction("GetVilla", new { villa.Id }, _response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages =
                    new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<APIResponse>> DeleteVilla(int id)
        {
            try
            {
                if (id == 0)
                    return BadRequest();
                var villa = await _dbVilla.GetAsync(v => v.Id == id);
                if (villa == null)
                    return NotFound();

                await _dbVilla.RemoveAsync(villa);

                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages =
                    new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<APIResponse>> UpdateVilla(int id, [FromBody] VillaUpdateDTO updateVillaDTO)
        {
            try
            {
                if (updateVillaDTO == null || updateVillaDTO.Id != id)
                    return BadRequest(updateVillaDTO);

                Villa villa = _mapper.Map<Villa>(updateVillaDTO);

                #region Before Using AutoMapper
                //Villa villa = new()
                //{
                //    Id = updateVillaDTO.Id,
                //    Name = updateVillaDTO.Name,
                //    Amenity = updateVillaDTO.Amenity,
                //    Details = updateVillaDTO.Details,
                //    Rate = updateVillaDTO.Rate,
                //    Sqft = updateVillaDTO.Sqft,
                //    Occupancy = updateVillaDTO.Occupancy,
                //    ImageURL = updateVillaDTO.ImageUrl,
                //}; 
                #endregion

                await _dbVilla.UpdateAsync(villa);
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages =
                    new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchDTO)
        {
            if (patchDTO == null || id == 0)
                return BadRequest(patchDTO);

            var villa = await _dbVilla.GetAsync(v => v.Id == id, tracked: false);

            if (villa == null)
                return BadRequest();

            VillaUpdateDTO villaDTO = _mapper.Map<VillaUpdateDTO>(villa);


            #region Before Using AutoMapper
            //VillaUpdateDTO villaDTO = new()
            //{
            //    Id = villa.Id,
            //    Name = villa.Name,
            //    Amenity = villa.Amenity,
            //    Details = villa.Details,
            //    Rate = villa.Rate,
            //    Sqft = villa.Sqft,
            //    Occupancy = villa.Occupancy,
            //    ImageUrl = villa.ImageURL,
            //}; 
            #endregion

            patchDTO.ApplyTo(villaDTO, ModelState);

            Villa model = _mapper.Map<Villa>(villaDTO);

            #region Before Using AutoMapper
            //Villa model = new()
            //{
            //    Id = villaDTO.Id,
            //    Name = villaDTO.Name,
            //    Amenity = villaDTO.Amenity,
            //    Details = villaDTO.Details,
            //    Rate = villaDTO.Rate,
            //    Sqft = villaDTO.Sqft,
            //    Occupancy = villaDTO.Occupancy,
            //    ImageURL = villaDTO.ImageUrl,
            //}; 
            #endregion

            await _dbVilla.UpdateAsync(model);

            if (!ModelState.IsValid)
                return BadRequest(patchDTO);

            return NoContent();
        }
    }
}
