using AutoMapper.Internal;
using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using static MagicVilla_Utility.SD;

namespace MagicVilla_Web.Services;

public class BaseService : IBaseService
{
    private readonly IHttpClientFactory _httpClient;
    private readonly ITokenProvider _tokenProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IApiMessageRequestBuilder _apiMessageRequestBuilder;
    private readonly string VillaApiUrl;
    public APIResponse responseModel { get; set; }
    public BaseService(IHttpClientFactory httpClient, ITokenProvider tokenProvider, IConfiguration configuration
        , IHttpContextAccessor httpContextAccessor, IApiMessageRequestBuilder apiMessageRequestBuilder)
    {
        responseModel = new();
        _httpClient = httpClient;
        _tokenProvider = tokenProvider;
        _httpContextAccessor = httpContextAccessor;
        _apiMessageRequestBuilder = apiMessageRequestBuilder;
        VillaApiUrl = configuration.GetValue<string>("ServiceUrls:VillaAPI");
    }
    public async Task<T> SendAsync<T>(APIRequest apiRequest, bool withBearer = true)
    {
        try
        {
            var client = _httpClient.CreateClient("MagicAPI");

            //defining a factory that creates a brand-new HttpRequestMessage every time you need it
            var messageFactory = () =>
            {
              return _apiMessageRequestBuilder.Build(apiRequest);
            };

            HttpResponseMessage httpResponseMessage = null;



            httpResponseMessage = await SendWithRefreshToken(client, messageFactory, withBearer);

            APIResponse FinalApiResponse = new()
            {
                IsSuccess = false
            };


            try
            {

                switch (httpResponseMessage.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        FinalApiResponse.ErrorMessages = new List<string>() { "Not Found" };
                        break;
                    case HttpStatusCode.Unauthorized:
                        FinalApiResponse.ErrorMessages = new List<string>() { "Unauthorized" };
                        break;
                    case HttpStatusCode.Forbidden:
                        FinalApiResponse.ErrorMessages = new List<string>() { "Access Denied" };
                        break;
                    case HttpStatusCode.InternalServerError:
                        FinalApiResponse.ErrorMessages = new List<string>() { "Internal Server Error" };
                        break;
                    default:
                        var apiContent = await httpResponseMessage.Content.ReadAsStringAsync();
                        FinalApiResponse.IsSuccess = true;
                        FinalApiResponse = JsonConvert.DeserializeObject<APIResponse>(apiContent);
                        break;
                }
            }

            catch (Exception e)
            {
                FinalApiResponse.ErrorMessages = new List<string>() { "Error Encountered", e.Message.ToString() };
            }
            var res = JsonConvert.SerializeObject(FinalApiResponse);
            var returnObj = JsonConvert.DeserializeObject<T>(res);
            return returnObj;
        }
        catch (AuthException)
        {
            throw;
        }
        catch (Exception e)
        {
            var dto = new APIResponse
            {
                ErrorMessages = new List<string> { Convert.ToString(e.Message) },
                IsSuccess = false
            };
            var res = JsonConvert.SerializeObject(dto);
            var APIResponse = JsonConvert.DeserializeObject<T>(res);
            return APIResponse;
        }
    }

    public async Task<HttpResponseMessage> SendWithRefreshToken(HttpClient httpClient,
        Func<HttpRequestMessage> httpRequestMessageFactory,
        bool withBearer = true)
    {
        if (!withBearer)
        {
            return await httpClient.SendAsync(httpRequestMessageFactory());
        }
        else
        {
            TokenDTO tokenDTO = _tokenProvider.GetToken();

            if (tokenDTO != null && !string.IsNullOrEmpty(tokenDTO.AccessToken))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDTO.AccessToken);
            }
            try
            {
                var response = await httpClient.SendAsync(httpRequestMessageFactory());
                if (response.IsSuccessStatusCode)
                    return response;

                // IF this fails then we can pass refresh token!
                if (!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    //GENERATE NEW Token from Refresh token / Sign in with that new token and then retry
                    await InvokeRefreshTokenEndpoint(httpClient, tokenDTO.AccessToken, tokenDTO.RefreshToken);
                    response = await httpClient.SendAsync(httpRequestMessageFactory());
                    return response;

                }
                return response;
            }
            catch (AuthException)
            {
                throw;
            }
            catch (HttpRequestException httpRequestException)
            {
                if (httpRequestException.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // refresh token and retry the request
                    await InvokeRefreshTokenEndpoint(httpClient, tokenDTO.AccessToken, tokenDTO.RefreshToken);
                    return await httpClient.SendAsync(httpRequestMessageFactory());
                }
                throw;
            }
        }
    }
    public async Task InvokeRefreshTokenEndpoint(HttpClient httpClient,
        string existingAcessToken, string existingRefreshToken)
    {
        HttpRequestMessage message = new();
        message.Headers.Add("Accept", "application/json");
        //message.RequestUri = new Uri($"{VillaApiUrl}/api/v{SD.CurrentAPIVersion}/UsersAuth/refresh");
        message.RequestUri = new Uri($"{VillaApiUrl}/api/{SD.CurrentAPIVersion}/User/refresh");
        message.Method = HttpMethod.Post;
        message.Content = new StringContent(JsonConvert.SerializeObject(new TokenDTO()
        {
            AccessToken = existingAcessToken,
            RefreshToken = existingRefreshToken
        }), Encoding.UTF8, "application/json");
        var response = await httpClient.SendAsync(message);
        var content = await response.Content.ReadAsStringAsync();
        var apiRespose = JsonConvert.DeserializeObject<APIResponse>(content);

        if (apiRespose?.IsSuccess != true)
        {
            await _httpContextAccessor.HttpContext.SignOutAsync();
            _tokenProvider.ClearToken();
            throw new AuthException();
        }
        else
        {
            var tokenDataStr = JsonConvert.SerializeObject(apiRespose);
            var tokenDto = JsonConvert.DeserializeObject<TokenDTO>(tokenDataStr);

            if (tokenDto != null && !string.IsNullOrEmpty(tokenDto.AccessToken))
            {

                //New method to sign in with the new token that we receive
                await SignInWithNewToken(tokenDto);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDto.AccessToken);
            }
        }
    }
    private async Task SignInWithNewToken(TokenDTO tokenDTO)
    {

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokenDTO.AccessToken);

        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        identity.AddClaim(new Claim(ClaimTypes.Name, jwt.Claims.FirstOrDefault(u => u.Type == "unique_name").Value));
        identity.AddClaim(new Claim(ClaimTypes.Role, jwt.Claims.FirstOrDefault(u => u.Type == "role").Value));
        var principal = new ClaimsPrincipal(identity);
        await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        _tokenProvider.SetToken(tokenDTO);

    }
}
