using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Dto;
using MagicVilla_VillaAPI.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public VillaAPIController(ApplicationDbContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VillaDTO>>> GetVillas()
        {
            IEnumerable<Villa> villaList = await _context.Villas.ToListAsync();

            return Ok(_mapper.Map<List<VillaDTO>>(villaList));
        }
        [HttpGet("{id:int}",Name ="GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task< ActionResult<VillaDTO>> GetVilla(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }

            var villa = await _context.Villas.FirstOrDefaultAsync(v => v.Id == id);
            if (villa == null)
                return NotFound();

            return Ok(_mapper.Map<VillaDTO>(villa));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VillaDTO>> AddVilla([FromBody] VillaCreateDTO createVillaDTO)
        {
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            if(await _context.Villas.FirstOrDefaultAsync(v=>v.Name.ToLower()==createVillaDTO.Name.ToLower())!=null)
            {
                ModelState.AddModelError("Name", "Villa Already Exists!");
                return BadRequest(ModelState);
            }

            if(createVillaDTO==null) 
                return BadRequest(createVillaDTO);

            Villa model = _mapper.Map<Villa>(createVillaDTO);
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

            await _context.Villas.AddAsync(model);
           await _context.SaveChangesAsync();

            return CreatedAtAction("GetVilla", new { model.Id }, model);
        }
        
        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteVilla(int id)
        {
            if(id==0)
                return BadRequest();
            var villa = await _context.Villas.FirstOrDefaultAsync(v => v.Id == id);
            if(villa==null)
                return NotFound();
            _context.Villas.Remove(villa);
           await _context.SaveChangesAsync();

            return NoContent();
        }
        
        [HttpPut("{id:int}",Name ="UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateVilla(int id, [FromBody] VillaUpdateDTO updateVillaDTO)
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

            _context.Update(villa);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id:int}",Name ="UpdatePartialVilla")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdatePartialVilla(int id,JsonPatchDocument<VillaUpdateDTO> patchDTO)
        {
            if(patchDTO==null||id==0) 
                return BadRequest(patchDTO);

            var villa = await _context.Villas.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);

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

            _context.Update(model);
            await _context.SaveChangesAsync();

            if(!ModelState.IsValid)
                return BadRequest(patchDTO);

            return NoContent();
        }
    }
}
