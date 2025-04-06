using AutoMapper;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MagicVilla_VillaAPI.Controllers.v1
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class VillaNumberController : ControllerBase
    {
        private readonly IVillaNumberRepository _dbVillaNum;
        private readonly IVillaRepository _dbVilla;
        private readonly IMapper _mapper;
        private readonly APIResponse _response;
        public VillaNumberController(IVillaNumberRepository villaNum,IVillaRepository dbVilla, IMapper mapper)
        {
            _dbVillaNum = villaNum;
            _dbVilla = dbVilla;
            _mapper = mapper;
            _response = new();
        }
        //CRUD Operation 
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        //[MapToApiVersion("1.0")]
        public async Task<ActionResult<APIResponse>> GetAll()
        {
            try
            {
                IEnumerable<VillaNumber> villaList = await _dbVillaNum.GetAllAsync(includeProperties:"Villa");
                _response.Result = _mapper.Map<List<VillaNumberDTO>>(villaList);
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

        [HttpGet("{id:int}",Name ="GetVillaNumber")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> GetVillaNumber(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    return BadRequest(_response);
                }
                var villaNo = await _dbVillaNum.GetAsync(v => v.VillaNo == id);
                if (villaNo == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    return NotFound(_response);
                }
                _response.Result = _mapper.Map<VillaNumberDTO>(villaNo);
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
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<APIResponse>> AddVilla([FromBody] VillaNumberCreateDTO createVillaNumDTO)
        {
            try
            {
                if (await _dbVillaNum.GetAsync(v => v.VillaNo == createVillaNumDTO.VillaNo) != null)
                {
                    ModelState.AddModelError("ErrorMessages", "Villa Number Already Exists!");
                    return BadRequest(ModelState);
                }
                if(await _dbVilla.GetAllAsync(v=>v.Id==createVillaNumDTO.VillaNo)==null)
                {
                    ModelState.AddModelError("ErrorMessages", "Villa Number is Invalid!");
                    return BadRequest(ModelState);
                }
                if (createVillaNumDTO == null)
                    return BadRequest(createVillaNumDTO);

                VillaNumber villa = _mapper.Map<VillaNumber>(createVillaNumDTO);

                await _dbVillaNum.CreateAsync(villa);
                _response.Result = _mapper.Map<VillaNumberDTO>(villa);
                _response.StatusCode = HttpStatusCode.Created;

                return CreatedAtAction("GetVillaNumber", new {id=villa.VillaNo }, _response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages =
                    new List<string>() { ex.ToString() };
            }
            return _response;
        }
        [HttpDelete("{id:int}",Name ="DeleteVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<APIResponse>> DeleteVillaNum(int id)
        {
            try
            {
                if (id == 0)
                    return BadRequest();

                VillaNumber villa = await _dbVillaNum.GetAsync(v => v.VillaNo == id);
                if (villa == null)
                    return NotFound();
                await _dbVillaNum.RemoveAsync(villa);
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages =
                    new List<string>() { ex.ToString() };
            }
            return _response;
        }
        [HttpPut("{id:int}",Name ="UpdateVillaNumber")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<APIResponse>> UpdateVillaNum(int id,[FromBody] VillaNumberUpdateDTO villaNumUpdateDTO)
        {
            try
            {
                if(villaNumUpdateDTO == null||id!=villaNumUpdateDTO.VillaNo)
                    return BadRequest(villaNumUpdateDTO);
                if (await _dbVilla.GetAllAsync(v => v.Id == villaNumUpdateDTO.VillaNo) == null)
                {
                    ModelState.AddModelError("ErrorMessages", "Villa Number is Invalid!");
                    return BadRequest(ModelState);
                }
                VillaNumber villa = _mapper.Map<VillaNumber>(villaNumUpdateDTO);
                await _dbVillaNum.UpdateAsync(villa);
                _response.StatusCode=HttpStatusCode.OK;
                _response.IsSuccess=true;

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
    }
   
}
