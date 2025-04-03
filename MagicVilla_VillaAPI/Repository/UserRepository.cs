using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicVilla_VillaAPI.Repository;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly string secretKey;

    public UserRepository(ApplicationDbContext context,IConfiguration configuration)
    {
        _context = context;
        secretKey = configuration.GetValue<string>("ApiSettings:Secret");
    }

    public bool IsUnique(string username)
    {
        var user = _context.LocalUsers.FirstOrDefault(u => u.UserName == username);
        if (user == null)
        {
            return true;
        }
        return false;
    }

    public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
    {
        var user = await _context.LocalUsers.FirstOrDefaultAsync(u=>u.UserName.ToLower() == loginRequestDTO.UserName.ToLower()
        && u.Password==loginRequestDTO.Password);
        if (user == null)
        {
            return new LoginResponseDTO()
            {
                Token = "",
                LocalUser = null
            };  
        
        }
        //Generate JWT Token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secretKey);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new Claim []
            {
                new Claim(ClaimTypes.Name, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires=DateTime.Now.AddDays(7),
            SigningCredentials = new(new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256Signature),
        };
        var token=tokenHandler.CreateToken(tokenDescriptor);
        LoginResponseDTO loginResponseDTO = new LoginResponseDTO() 
        { 
        LocalUser = user,
        Token=tokenHandler.WriteToken(token),
        };
        return loginResponseDTO;
    }

    public async Task<LocalUser> Register(RegisterationRequestDTO registerationRequestDTO)
    {
        LocalUser user = new()
        {
            UserName = registerationRequestDTO.UserName,
            Password = registerationRequestDTO.Password,
            Name = registerationRequestDTO.Name,
            Role = registerationRequestDTO.Role,
        };
        await _context.LocalUsers.AddAsync(user);
        await _context.SaveChangesAsync();
        user.Password = "";
        return user;
    }
}
