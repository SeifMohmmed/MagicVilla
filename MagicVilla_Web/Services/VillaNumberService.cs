using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;

namespace MagicVilla_Web.Services;

public class VillaService : BaseService, IVillaService
{
    private readonly IHttpClientFactory _clientFactory;
    private string _villaUrl;

    public VillaService(IHttpClientFactory clientFactory,IConfiguration configuration) : base(clientFactory)
    {
        _clientFactory = clientFactory;
        _villaUrl = configuration.GetValue<string>("ServiceUrls:VillaAPI");
    }
    public Task<T> CreateAsync<T>(VillaCreateDTO dto)
    {
        return SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.POST,
            Data = dto,
            URL = _villaUrl+"/api/villaApi"
        });
    }


    public Task<T> DeleteAsync<T>(int id)
    {
        return SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.DELETE,
            URL = _villaUrl + "/api/villaApi/"+id
        });
    }

    public Task<T> GetAllAsync<T>()
    {
        return SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.GET,
            URL = _villaUrl + "/api/villaApi"
        });
    }

    public Task<T> GetAsync<T>(int id)
    {
        return SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.GET,
            URL = _villaUrl + "/api/villaApi/"+id
        });
    }

    public Task<T> UpdateAsync<T>(VillaUpdateDTO dto)
    {
        return SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.PUT,
            Data = dto,
            URL = _villaUrl + "/api/villaApi/"+dto.Id
        });
    }
}
