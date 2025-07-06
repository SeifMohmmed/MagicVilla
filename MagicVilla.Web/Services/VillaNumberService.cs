using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;

namespace MagicVilla_Web.Services;

public class VillaNumberService : IVillaNumberService
{
    private string _villaUrl;
    private readonly IHttpClientFactory _clientFactory;
    private readonly IBaseService _baseService;

    public VillaNumberService(IHttpClientFactory clientFactory, IConfiguration configuration, IBaseService baseService)
    {
        _clientFactory = clientFactory;
        _baseService = baseService;
        _villaUrl = configuration.GetValue<string>("ServiceUrls:VillaAPI");

    }
    public async Task<T> CreateAsync<T>(VillaNumberCreateDTO dto)
    {
        return await _baseService.SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.POST,
            Data = dto,
            URL = _villaUrl + $"/api/{SD.CurrentAPIVersion}/villaNumber",
        });
    }


    public async Task<T> DeleteAsync<T>(int id)
    {
        return await _baseService.SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.DELETE,
            URL = _villaUrl + $"/api/{SD.CurrentAPIVersion}/villaNumber/" + id,
        });
    }

    public async Task<T> GetAllAsync<T>()
    {
        return await _baseService.SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.GET,
            URL = _villaUrl + $"/api/{SD.CurrentAPIVersion}/villaNumber",
        });
    }

    public async Task<T> GetAsync<T>(int id)
    {
        return await _baseService.SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.GET,
            URL = _villaUrl + $"/api/{SD.CurrentAPIVersion}/villaNumber/" + id,
        });
    }

    public async Task<T> UpdateAsync<T>(VillaNumberUpdateDTO dto)
    {
        return await _baseService.SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.PUT,
            Data = dto,
            URL = _villaUrl + $"/api/{SD.CurrentAPIVersion}/villaNumber/" + dto.VillaNo,
        });
    }
}
