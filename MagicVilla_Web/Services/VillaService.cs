using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;

namespace MagicVilla_Web.Services;

public class VillaService : BaseService, IVillaService
{
    private readonly IHttpClientFactory _clientFactory;
    private string _villaUrl;
    public VillaService(IHttpClientFactory clientFactory, IConfiguration configuration) :base(clientFactory)
    {
        _clientFactory = clientFactory;
        _villaUrl = configuration.GetValue<string>("ServiceUrls:VillaAPI");

    }
    public Task<T> CreateAsync<T>(VillaCreateDTO dto, string token)
    {
        return SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.POST,
            Data = dto,
            URL = _villaUrl+ $"/api/{SD.CurrentAPIVersion}/villaAPI",
            Token = token,
            ContentType=SD.ContentType.MultipartFormData
        });
    }


    public Task<T> DeleteAsync<T>(int id, string token)
    {
        return SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.DELETE,
            URL = _villaUrl + $"/api/{SD.CurrentAPIVersion}/villaAPI/" + id,
            Token = token
        });
    }

    public Task<T> GetAllAsync<T>(string token)
    {
        return SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.GET,
            URL = _villaUrl + $"/api/{SD.CurrentAPIVersion}/villaAPI",
            Token = token
        });
    }

    public Task<T> GetAsync<T>(int id, string token)
    {
        return SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.GET,
            URL = _villaUrl + $"/api/{SD.CurrentAPIVersion}/villaAPI/" + id,
            Token = token
        });
    }

    public Task<T> UpdateAsync<T>(VillaUpdateDTO dto, string token)
    {
        return SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.PUT,
            Data = dto,
            URL = _villaUrl + $"/api/{SD.CurrentAPIVersion}/villaAPI/" + dto.Id,
            Token = token,
            ContentType = SD.ContentType.MultipartFormData
        });
    }
}
