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
        // Validate incoming key early with clear error messages
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new ArgumentException("JWT key is not set. Ensure JWT_KEY is present in your environment or .env file.", nameof(jwtKey));
        }

        // Validate minimum key length for HS256 (128 bits = 16 bytes). For UTF8 strings, check bytes length.
        var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
        if (keyBytes.Length < 16)
        {
            throw new ArgumentException($"JWT key is too short: {keyBytes.Length} bytes. HS256 requires at least 16 bytes (128 bits). Provide a longer secret.", nameof(jwtKey));
        }

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
