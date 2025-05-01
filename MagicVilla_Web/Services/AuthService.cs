using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;

namespace MagicVilla_Web.Services;

public class AuthService : IAuthService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly IBaseService _baseService;
    private string _villaUrl;

    public AuthService(IHttpClientFactory clientFactory, IConfiguration configuration, IBaseService baseService)
    {
        _clientFactory = clientFactory;
        _baseService = baseService;
        _villaUrl = configuration.GetValue<string>("ServiceUrls:VillaAPI");
    }

    public async Task<T> LoginAsync<T>(LoginRequestDTO obj)
    {
        return await _baseService.SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.POST,
            Data = obj,
            URL = _villaUrl + $"/api/{SD.CurrentAPIVersion}/User/login"
        }, withBearer: false);
    }
    public async Task<T> RegisterAsync<T>(RegisterationRequestDTO obj)
    {
        return await _baseService.SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.POST,
            Data = obj,
            URL = _villaUrl + $"/api/{SD.CurrentAPIVersion}/User/register"
        }, withBearer: false);
    }
    public async Task<T> LogoutAsync<T>(TokenDTO obj)
    {
        return await _baseService.SendAsync<T>(new APIRequest
        {
            ApiType = SD.ApiType.POST,
            Data = obj,
            URL = _villaUrl + $"/api/{SD.CurrentAPIVersion}/User/revoke"
        });
    }

}
