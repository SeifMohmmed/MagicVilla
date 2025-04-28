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
    private readonly string VillaApiUrl;
    public APIResponse responseModel { get; set; }
    public BaseService(IHttpClientFactory httpClient, ITokenProvider tokenProvider, IConfiguration configuration
        , IHttpContextAccessor httpContextAccessor)
    {
        responseModel = new();
        _httpClient = httpClient;
        _tokenProvider = tokenProvider;
        _httpContextAccessor = httpContextAccessor;
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
                HttpRequestMessage message = new();
                if (apiRequest.ContentType == ContentType.MultipartFormData)
                {
                    message.Headers.Add("Accept", "*/*");
                }
                else
                {
                    message.Headers.Add("Accept", "application/json");
                }
                message.RequestUri = new Uri(apiRequest.URL);

                if (withBearer && _tokenProvider.GetToken() != null)
                {
                    var token = _tokenProvider.GetToken();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
                }

                if (apiRequest.ContentType == ContentType.MultipartFormData)
                {
                    var content = new MultipartFormDataContent();

                    foreach (var prop in apiRequest.Data.GetType().GetProperties())
                    {
                        var value = prop.GetValue(apiRequest.Data);
                        if (value is FormFile)
                        {
                            var file = (FormFile)value;
                            if (file != null)
                            {
                                content.Add(new StreamContent(file.OpenReadStream()), prop.Name, file.FileName);
                            }
                        }
                        else
                        {
                            content.Add(new StringContent(value == null ? "" : value.ToString()), prop.Name);
                        }
                    }

                    message.Content = content;
                }
                else
                {
                    if (apiRequest.Data != null)
                    {
                        message.Content = new StringContent(JsonConvert.SerializeObject(apiRequest.Data),
                            Encoding.UTF8, "application/json");
                    }
                }


                switch (apiRequest.ApiType)
                {
                    case SD.ApiType.POST:
                        message.Method = HttpMethod.Post;
                        break;
                    case SD.ApiType.PUT:
                        message.Method = HttpMethod.Put;
                        break;
                    case SD.ApiType.DELETE:
                        message.Method = HttpMethod.Delete;
                        break;
                    default:
                        message.Method = HttpMethod.Get;
                        break;

                }
                return message;
            };

            HttpResponseMessage apiResponse = null;

            if (!string.IsNullOrEmpty(apiRequest.Token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiRequest.Token);
            }
            apiResponse = await SendWithRefreshToken(client, messageFactory, withBearer);

            var apiContent = await apiResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"API Response: {apiContent}"); // Already logged

            try
            {
                APIResponse ApiResponse = JsonConvert.DeserializeObject<APIResponse>(apiContent);
                if (ApiResponse != null && (apiResponse.StatusCode == System.Net.HttpStatusCode.BadRequest
                    || apiResponse.StatusCode == System.Net.HttpStatusCode.NotFound))
                {
                    ApiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    ApiResponse.IsSuccess = false;
                    var res = JsonConvert.SerializeObject(ApiResponse);
                    var returnObj = JsonConvert.DeserializeObject<T>(res);
                    return returnObj;
                }
            }
            catch (Exception e)
            {
                var exceptionResponse = JsonConvert.DeserializeObject<T>(apiContent);
                return exceptionResponse;
            }
            var APIResponse = JsonConvert.DeserializeObject<T>(apiContent);
            return APIResponse;

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
                if(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    //GENERATE NEW Token from Refresh token / Sign in with that new token and then retry
                    await InvokeRefreshTokenEndpoint(httpClient, tokenDTO.AccessToken,tokenDTO.RefreshToken);
                    response = await httpClient.SendAsync(httpRequestMessageFactory());
                    return response;

                }
                return response;
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
        message.RequestUri = new Uri($"{VillaApiUrl}/api/v{SD.CurrentAPIVersion}/UserAuth/refresh");
        message.Method = HttpMethod.Post;
        message.Content = new StringContent(JsonConvert.SerializeObject(new TokenDTO()
        {
            AccessToken = existingAcessToken,
            RefreshToken = existingRefreshToken
        }), Encoding.UTF8, "application/json");
        var response = await httpClient.SendAsync(message);
        var content = await response.Content.ReadAsStringAsync();
        var apiRespose = JsonConvert.DeserializeObject<APIResponse>(content);

        if (apiRespose?.IsSuccess != null)
        {
            await _httpContextAccessor.HttpContext.SignOutAsync();
            _tokenProvider.ClearToken();
        }
        else
        {
            var tokenDataStr = JsonConvert.SerializeObject(apiRespose);
            var tokenDto = JsonConvert.DeserializeObject<TokenDTO>(tokenDataStr);

            if (tokenDto != null && !String.IsNullOrEmpty(tokenDto.AccessToken))
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
