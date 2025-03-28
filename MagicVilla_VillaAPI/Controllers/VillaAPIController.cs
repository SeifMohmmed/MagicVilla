using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Dto;
using MagicVilla_VillaAPI.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        private readonly ILogging _logger;

        public VillaAPIController(ILogging logger)
        {
            _logger = logger;
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<VillaDTO>> GetVillas()
        {
            _logger.Log("Getting All Villas!","");
            return Ok(VillaStore.villaList);
        }
        [HttpGet("{id:int}",Name ="GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<VillaDTO> GetVilla(int id)
        {
            if (id == 0)
            {
                _logger.Log("Getting Villa Error with Id: " + id,"error");
                return BadRequest();
            }

            var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == id);
            if (villa == null)
                return NotFound();

            return Ok(villa);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<VillaDTO> AddVilla([FromBody] VillaDTO model)
        {
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            if(VillaStore.villaList.FirstOrDefault(v=>v.Name.ToLower()==model.Name.ToLower())!=null)
            {
                ModelState.AddModelError("Name", "Villa Already Exists!");
                return BadRequest(ModelState);
            }

            if(model==null) 
                return BadRequest(model);
            if (model.Id > 0)
                return StatusCode(StatusCodes.Status500InternalServerError);
            model.Id= VillaStore.villaList.OrderByDescending(v=> v.Id).FirstOrDefault().Id+1;

            VillaStore.villaList.Add(model);

            return CreatedAtAction("GetVilla", new { id = model.Id }, model);
        }
        
        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult DeleteVilla(int id)
        {
            if(id==0)
                return BadRequest();
            var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == id);
            if(villa==null)
                return NotFound();
            VillaStore.villaList.Remove(villa);

            return NoContent();
        }
        
        [HttpPut("{id:int}",Name ="UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult UpdateVilla(int id, [FromBody] VillaDTO model)
        {
            if (model == null || model.Id != id)
                return BadRequest(model);
            var villa = VillaStore.villaList.FirstOrDefault(v=>v.Id== id);
            if(villa==null)
                return NotFound();

            villa.Name = model.Name;
            villa.Occupancy = model.Occupancy;
            villa.Sqft= model.Sqft;

            return NoContent();
        }

        [HttpPatch("{id:int}",Name ="UpdatePartialVilla")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult UpdatePartialVilla(int id,JsonPatchDocument<VillaDTO> model)
        {
            if(model==null||id==0) 
                return BadRequest(model);

            var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == id);

            if(villa==null)
                return NotFound(model);

            model.ApplyTo(villa,ModelState);

            if(!ModelState.IsValid)
                return BadRequest(model);

            return NoContent();
        }
    }
}
