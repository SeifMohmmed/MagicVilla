using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;

namespace MagicVilla_Web.Services;

public class VillaNumberService : BaseService, IVillaNumberService
{
    private string _villaUrl;
    private readonly IHttpClientFactory _clientFactory;

    public VillaNumberService(IHttpClientFactory clientFactory,IConfiguration configuration) : base(clientFactory)
    {
        _clientFactory = clientFactory;
        _villaUrl = configuration.GetValue<string>("ServiceUrls:VillaAPI");

    }
    public Task<T> CreateAsync<T>(VillaNumberCreateDTO dto)
    {
        return SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.POST,
            Data = dto,
            URL = _villaUrl + $"/api/{SD.CurrentAPIVersion}/villaNumber",
        });
    }


    public Task<T> DeleteAsync<T>(int id)
    {
        return SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.DELETE,
            URL = _villaUrl + $"/api/{SD.CurrentAPIVersion}/villaNumber/" + id,
        });
    }

    public Task<T> GetAllAsync<T>()
    {
        return SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.GET,
            URL = _villaUrl + $"/api/{SD.CurrentAPIVersion}/villaNumber",
        });
    }

    public Task<T> GetAsync<T>(int id)
    {
        return SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.GET,
            URL = _villaUrl + $"/api/{SD.CurrentAPIVersion}/villaNumber/" + id,
        });
    }

    public Task<T> UpdateAsync<T>(VillaNumberUpdateDTO dto)
    {
        return SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.PUT,
            Data = dto,
            URL = _villaUrl + $"/api/{SD.CurrentAPIVersion}/villaNumber/" + dto.VillaNo,
        });
    }
}
