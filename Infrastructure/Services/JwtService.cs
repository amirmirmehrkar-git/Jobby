using Application.Common.Interfaces;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services
{
    public class JwtService : IJwtService
    {
        protected ISender Mediator;

        public JwtService(ISender mediator)
        {
            Mediator = mediator;
        }

        // A committed signing key must never be reused. Configure a fresh secret externally.
        public static string secret => ReadSigningKey();

        private static string ReadSigningKey()
        {
            var key = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY");
            if (string.IsNullOrWhiteSpace(key) || key.Length < 32 || Encoding.UTF8.GetByteCount(key) != key.Length)
            {
                throw new InvalidOperationException(
                    "JWT_SIGNING_KEY must be set to a new, cryptographically random secret of at least 32 ASCII characters.");
            }
            return key;
        }

        public class Data
        {
            public string Roles { get; set; }
        }
        public string GenerateJwtToken(string Roles)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);
            var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("Roles", Roles) }),
                Expires = DateTime.UtcNow.AddHours(5),
                //Expires = DateTime.UtcNow.AddSeconds(2),
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
