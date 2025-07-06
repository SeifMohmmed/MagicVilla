using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicVilla_VillaAPI.Repository;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMapper _mapper;
    private readonly string secretKey;

    public UserRepository(ApplicationDbContext context, IConfiguration configuration,
        UserManager<ApplicationUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _mapper = mapper;
        secretKey = configuration.GetValue<string>("ApiSettings:Secret");
        _roleManager = roleManager;
    }

    public bool IsUnique(string username)
    {
        var user = _context.ApplicationUsers.FirstOrDefault(u => u.UserName == username);
        if (user == null)
        {
            return true;
        }
        return false;
    }

    public async Task<TokenDTO> Login(LoginRequestDTO loginRequestDTO)
    {
        var user = await _context.ApplicationUsers
            .FirstOrDefaultAsync(u => u.UserName.ToLower() == loginRequestDTO.UserName.ToLower());

        bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);
        if (user == null || isValid == false)
        {
            return new TokenDTO()
            {
                AccessToken = "",
            };

        }
        var jwtTokenId = $"JTI{Guid.NewGuid()}";
        var accessToken = await GetAccessToken(user, jwtTokenId);
        var refreshToken = await CreateNewRefreshToken(user.Id, jwtTokenId);
        TokenDTO tokenDto = new()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        };
        return tokenDto;
    }

    public async Task<UserDTO> Register(RegisterationRequestDTO registerationRequestDTO)
    {
        ApplicationUser user = new()
        {
            UserName = registerationRequestDTO.UserName,
            Email = registerationRequestDTO.UserName,
            NormalizedEmail = registerationRequestDTO.UserName.ToUpper(),
            Name = registerationRequestDTO.Name,
        };
        try
        {
            var result = await _userManager.CreateAsync(user, registerationRequestDTO.Password);
            if (result.Succeeded)
            {
                if (!_roleManager.RoleExistsAsync(registerationRequestDTO.Role).GetAwaiter().GetResult())
                {
                    await _roleManager.CreateAsync(new IdentityRole(registerationRequestDTO.Role));
                }

                await _userManager.AddToRoleAsync(user, registerationRequestDTO.Role);
                var userToReturn = _context.ApplicationUsers
                    .FirstOrDefault(u => u.Name == registerationRequestDTO.UserName);
                return _mapper.Map<UserDTO>(userToReturn);
            }
        }
        catch (Exception ex)
        {

        }
        return new UserDTO();
    }
    public async Task<string> GetAccessToken(ApplicationUser user, string jwtTokenId)
    {
        //if user was found Generate JWT Token
        var roles = await _userManager.GetRolesAsync(user);
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secretKey);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, user.UserName.ToString()),
                new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                new Claim(JwtRegisteredClaimNames.Jti, jwtTokenId),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Aud, "dotnetmastery.com"),

            }),
            Expires = DateTime.Now.AddMinutes(1),
            Issuer = "https://localhost:7001",
            Audience = "https://localhost:4200",
            SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenStr = tokenHandler.WriteToken(token);
        return tokenStr;
    }

    public async Task<TokenDTO> RefreshAccessToken(TokenDTO tokenDTO)
    {
        // Find an existing refresh token
        var existingRefreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(u => u.Refresh_Token == tokenDTO.RefreshToken);
        if (existingRefreshToken == null)
        {
            return new TokenDTO();
        }

        // Compare data from existing refresh and access token provided and if there is any missmatch
        // then consider it as a fraud
        var isTokenValid = GetAccessTokenData(tokenDTO.AccessToken, existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
        if (!isTokenValid)
        {
            await MarkTokenAsInvalid(existingRefreshToken);
            return new TokenDTO();
        }

        // When someone tries to use not valid refresh token, fraud possible
        if (!existingRefreshToken.IsValid)
        {
            await MarkAllTokenInChainAsInvalid(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
        }
        // If just expired then mark as invalid and return empty
        if (existingRefreshToken.ExpiresAt < DateTime.UtcNow)
        {
            await MarkTokenAsInvalid(existingRefreshToken);
            return new TokenDTO();
        }
        // replace old refresh with a new one with updated expire date
        var newRefreshToken = await CreateNewRefreshToken(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);

        // revoke existing refresh token
        await MarkTokenAsInvalid(existingRefreshToken);


        // generate new access token
        var applicationUser = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == existingRefreshToken.UserId);
        if (applicationUser == null)
        {
            return new TokenDTO();
        }

        var newAccessToken = await GetAccessToken(applicationUser, existingRefreshToken.JwtTokenId);

        return new TokenDTO()
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
        };
    }
    public async Task RevokeRefreshToken(TokenDTO tokenDTO)
    {
        var existingRefreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(u => u.Refresh_Token == tokenDTO.RefreshToken);
        if (existingRefreshToken == null)
            return;

        // Compare data from existing refresh and access token provided and
        // if there is any missmatch then we should do nothing with refresh token
        var isTokenValid = GetAccessTokenData(tokenDTO.AccessToken, existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
        if (!isTokenValid)
        {
            return;
        }
        await MarkAllTokenInChainAsInvalid(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
    }
    private async Task<string> CreateNewRefreshToken(string userId, string tokenId)
    {
        RefreshToken refreshToken = new RefreshToken()
        {
            IsValid = true,
            UserId = userId,
            JwtTokenId = tokenId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(2),
            Refresh_Token = Guid.NewGuid() + "-" + Guid.NewGuid(),
        };
        await _context.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
        return refreshToken.Refresh_Token;
    }
    private bool GetAccessTokenData(string accessToken, string expectedUserId, string expectedTokenId)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(accessToken);
            var jwtTokenId = jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
            var userId = jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value;
            return userId == expectedUserId && jwtTokenId == expectedTokenId;
        }
        catch
        {
            return false;
        }
    }
    private async Task MarkAllTokenInChainAsInvalid(string userId, string tokenId)
    {
        await _context.RefreshTokens.Where(u => u.UserId == userId
        && u.JwtTokenId == tokenId)
            .ExecuteUpdateAsync(u => u.SetProperty(refreshToken => refreshToken.IsValid, false));
    }
    private async Task MarkTokenAsInvalid(RefreshToken refreshToken)
    {
        refreshToken.IsValid = false;
        await _context.SaveChangesAsync();
    }


}
