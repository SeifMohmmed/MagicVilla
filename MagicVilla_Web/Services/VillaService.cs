using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;

namespace MagicVilla_Web.Services;

public class VillaNumberService : BaseService, IVillaNumberService
{
    private readonly IHttpClientFactory _clientFactory;
    private string _villaUrl;

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
            URL = _villaUrl+ "/api/villaNumber"
        });
    }


    public Task<T> DeleteAsync<T>(int id)
    {
        return SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.DELETE,
            URL = _villaUrl + "/api/villaNumber/" + id
        });
    }

    public Task<T> GetAllAsync<T>()
    {
        return SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.GET,
            URL = _villaUrl + "/api/villaNumber"
        });
    }

    public Task<T> GetAsync<T>(int id)
    {
        return SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.GET,
            URL = _villaUrl + "/api/villaNumber/" + id
        });
    }

    public Task<T> UpdateAsync<T>(VillaNumberUpdateDTO dto)
    {
        return SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.PUT,
            Data = dto,
            URL = _villaUrl + "/api/villaNumber/" +dto.VillaNo
        });
    }
}
