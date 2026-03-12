using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WorkshopAPI.Services
{
    public class JwtService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expirationMinutes;
        private readonly int _refreshTokenExpirationDays;

        public JwtService(IConfiguration configuration)
        {
            _secretKey = configuration["Jwt:SecretKey"] ?? "your-super-secret-key-min-256-bits-change-in-production";
            _issuer = configuration["Jwt:Issuer"] ?? "UserApi";
            _audience = configuration["Jwt:Audience"] ?? "UserApi";
            _expirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "30");
            _refreshTokenExpirationDays = int.Parse(configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");
        }

        private string GenerateToken(int userId, string username, int expirationMinutes, string? jti = null, IEnumerable<Claim>? extraClaims = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, jti ?? Guid.NewGuid().ToString())
            };

            // extraClaims คือ claims เพิ่มเติม (เช่น ati) เพื่อผูก refresh token กับ access token jti
            // ช่วยให้ตอน refresh รู้ว่า access token เดิมตัวไหนต้อง revoke
            if (extraClaims != null)
            {
                claims.AddRange(extraClaims);
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



        public (string Token, string Jti) GenerateAccessTokenWithJti(int userId, string username)
        {
            var jti = Guid.NewGuid().ToString();
            var token = GenerateToken(userId, username, _expirationMinutes, jti);
            return (token, jti);
        }

        public int GetAccessTokenExpirationSeconds()
        {
            return _expirationMinutes * 60;
        }



        public string GenerateRefreshToken(int userId, string username, string accessTokenJti)
        {
            // Refresh token ผูกกับ access token jti เพื่อ revoke access token เดิมได้
            var extraClaims = new[] { new Claim("ati", accessTokenJti) };
            return GenerateToken(userId, username, _refreshTokenExpirationDays * 24 * 60, null, extraClaims);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
