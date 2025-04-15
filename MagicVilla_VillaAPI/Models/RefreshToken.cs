using System.ComponentModel.DataAnnotations;

namespace MagicVilla_VillaAPI.Models;

public class RefreshToken
{
    [Key]
    public int Id { get; set; }
    public string UserId { get; set; }
    public string JwtTokenId { get; set; } // Unique Token Id that will given to Access Token
    public string Refresh_Token { get; set; }//Actual Refresh Token
    public bool IsValid { get; set; }
    public DateTime ExpiresAt { get; set; }
}
