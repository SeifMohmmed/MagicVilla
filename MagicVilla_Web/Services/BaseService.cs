using AutoMapper.Internal;
using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Services.IServices;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace MagicVilla_Web.Services;

public class BaseService : IBaseService
{
    private readonly IHttpClientFactory _httpClient;
    public APIResponse responseModel { get; set; }
    public BaseService(IHttpClientFactory httpClient)
    {
        responseModel = new();
        _httpClient = httpClient;
    }
    public async Task<T> SendAsync<T>(APIRequest request)
    {
        try
        {
            var client = _httpClient.CreateClient("MagicAPI");
            HttpRequestMessage message = new HttpRequestMessage();
            message.Headers.Add("Accept", "application/json");
            message.RequestUri = new Uri(request.URL);

            if (request.Data != null)
            {
                message.Content = new StringContent(JsonConvert.SerializeObject(request.Data)
                    , Encoding.UTF8, "application/json");
            }
            switch (request.ApiType)
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
            HttpResponseMessage apiResponse = null;

            if (!string.IsNullOrEmpty(request.Token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.Token);
            }

            apiResponse = await client.SendAsync(message);
            var apiContent = await apiResponse.Content.ReadAsStringAsync();
            try
            {
                APIResponse ApiResponse = JsonConvert.DeserializeObject<APIResponse>(apiContent);
                if (ApiResponse!=null&& (apiResponse.StatusCode == HttpStatusCode.BadRequest
                    || apiResponse.StatusCode == HttpStatusCode.NotFound))
                {
                    ApiResponse.StatusCode = HttpStatusCode.BadRequest;
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
        catch (Exception ex)
        {
            var dto = new APIResponse()
            {
                ErrorMessages=new List<string>() { ex.Message.ToString()},
                IsSuccess=false
            };
            var res = JsonConvert.SerializeObject(dto);
            var apiResponse=JsonConvert.DeserializeObject<T>(res);
            return apiResponse;
        }
    }
}
