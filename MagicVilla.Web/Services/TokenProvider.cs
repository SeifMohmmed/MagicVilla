using MagicVilla_Utility;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;

namespace MagicVilla_Web.Services;

public class TokenProvider : ITokenProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TokenProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public void ClearToken()
    {
        _httpContextAccessor.HttpContext?.Response.Cookies.Delete(SD.AccessToken);
        _httpContextAccessor.HttpContext?.Response.Cookies.Delete(SD.RefreshToken);
    }

    public TokenDTO GetToken()
    {
        try
        {
            bool hasAcessToken = _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(SD.AccessToken, out string accessToken);
            bool hasRefreshToken = _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(SD.RefreshToken, out string refreshToken);
            TokenDTO tokenDTO = new TokenDTO()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
            return hasAcessToken ? tokenDTO : null;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public void SetToken(TokenDTO tokenDto)
    {
        var cookiesOptions = new CookieOptions() { Expires = DateTime.Now.AddDays(60) };
        _httpContextAccessor.HttpContext?.Response.Cookies.Append(SD.AccessToken,tokenDto.AccessToken, cookiesOptions);
        _httpContextAccessor.HttpContext?.Response.Cookies.Append(SD.RefreshToken,tokenDto.RefreshToken, cookiesOptions);
    }
}
