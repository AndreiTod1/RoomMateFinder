using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RoomMate_Finder.Entities;

namespace RoomMate_Finder.Common;

public class JwtService
{
    private readonly string _jwtKey;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtService(string jwtKey, string issuer, string audience)
    {
        _jwtKey = jwtKey;
        _issuer = issuer;
        _audience = audience;
    }

    public string GenerateToken(Profile user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7), // Token valid pentru 7 zile
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
