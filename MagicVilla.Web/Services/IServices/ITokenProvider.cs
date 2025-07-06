using MagicVilla_Web.Models.Dto;

namespace MagicVilla_Web.Services.IServices;

public interface ITokenProvider
{
    void SetToken(TokenDTO tokenDto);
    TokenDTO? GetToken();
    void ClearToken();
}
