using AutoMapper;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MagicVilla_VillaAPI.Controllers.v2
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("2.0")]
    public class VillaNumberController : ControllerBase
    {
        private readonly IVillaNumberRepository _dbVillaNum;
        private readonly IVillaRepository _dbVilla;
        private readonly IMapper _mapper;
        private readonly APIResponse _response;
        public VillaNumberController(IVillaNumberRepository villaNum, IVillaRepository dbVilla, IMapper mapper)
        {
            _dbVillaNum = villaNum;
            _dbVilla = dbVilla;
            _mapper = mapper;
            _response = new();
        }

        //[MapToApiVersion("2.0")]
        [HttpGet]
        [HttpGet("GetString")]
        public IEnumerable<string> Get()
        {
            return new string[] { "Bhrugen", "DotNetMastery" };
        }
    }
}
