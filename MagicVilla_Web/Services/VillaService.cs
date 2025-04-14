using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;

namespace MagicVilla_Web.Services;

public class VillaService : IVillaService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly IBaseService _baseService;
    private string _villaUrl;
    public VillaService(IHttpClientFactory clientFactory, IConfiguration configuration, IBaseService baseService)
    {
        _clientFactory = clientFactory;
        _baseService = baseService;
        _villaUrl = configuration.GetValue<string>("ServiceUrls:VillaAPI");

    }
    public async Task<T> CreateAsync<T>(VillaCreateDTO dto)
    {
        return await _baseService.SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.POST,
            Data = dto,
            URL = _villaUrl + $"/api/{SD.CurrentAPIVersion}/villaAPI",
            ContentType = SD.ContentType.MultipartFormData
        });
    }


    public async Task<T> DeleteAsync<T>(int id)
    {
        return await _baseService.SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.DELETE,
            URL = _villaUrl + $"/api/{SD.CurrentAPIVersion}/villaAPI/" + id,
        });
    }

    public async Task<T> GetAllAsync<T>()
    {
        return await _baseService.SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.GET,
            URL = _villaUrl + $"/api/{SD.CurrentAPIVersion}/villaAPI",
        });
    }

    public async Task<T> GetAsync<T>(int id)
    {
        return await _baseService.SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.GET,
            URL = _villaUrl + $"/api/{SD.CurrentAPIVersion}/villaAPI/" + id,
        });
    }

    public async Task<T> UpdateAsync<T>(VillaUpdateDTO dto)
    {
        return await _baseService.SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.PUT,
            Data = dto,
            URL = _villaUrl + $"/api/{SD.CurrentAPIVersion}/villaAPI/" + dto.Id,
            ContentType = SD.ContentType.MultipartFormData
        });
    }
}
