using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MagicVilla_Web.Models;
using AutoMapper;
using MagicVilla_Web.Services.IServices;
using Newtonsoft.Json;
using MagicVilla_Web.Models.Dto;

namespace MagicVilla_Web.Controllers;

public class HomeController : Controller
{
    private readonly IMapper _mapper;
    private readonly IVillaService _villaService;

    public HomeController(IMapper mapper, IVillaService villaService)
    {
        _mapper = mapper;
        _villaService = villaService;
    }
    public async Task<IActionResult> Index()
    {
        List<VillaDTO> villaList = new();

        var response = await _villaService.GetAllAsync<APIResponse>();
        if (response != null && response.IsSuccess)
        {
            villaList = JsonConvert.DeserializeObject<List<VillaDTO>>(Convert.ToString(response.Result));
        }
        return View(villaList);
    }
}
