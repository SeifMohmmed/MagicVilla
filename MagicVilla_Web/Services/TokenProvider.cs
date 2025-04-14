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
    }

    public TokenDTO GetToken()
    {
        try
        {
            bool hasAcessToken = _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(SD.AccessToken, out string accessToken);
            TokenDTO tokenDTO = new TokenDTO()
            {
                Token = accessToken,
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
        _httpContextAccessor.HttpContext?.Response.Cookies.Append(SD.AccessToken,tokenDto.Token, cookiesOptions);
    }
}
