using JWT.Algorithms;
using JWT.Exceptions;
using JWT.Serializers;
using JWT;
using Newtonsoft.Json;
using static Infrastructure.Services.JwtService;

namespace WebUI.Services
{
    public class CurrentUserService
    {
        public static string ValidateJwtToken(string JWT)
        {
            try
            {
                IJsonSerializer serializer = new JsonNetSerializer();
                var provider = new UtcDateTimeProvider();
                IJwtValidator validator = new JwtValidator(serializer, provider);
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
                IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);

                var json = decoder.Decode(JWT, secret, verify: true);
                var myClass = JsonConvert.DeserializeObject<Data>(json);

                return myClass.Roles;
            }
            catch (TokenExpiredException)
            {
                throw new Exception("تاریخ انقضای توکن به پایان رسیده است");
            }
            catch (SignatureVerificationException)
            {
                throw new Exception("توکن نامعتبر");
            }
        }

        public static string GetUserId(HttpContext HttpContext)
        {
            return ValidateJwtToken(HttpContext.Request.Headers["TokenId"].ToString());
        }

        public static string GetUserId(string Token)
        {
            return ValidateJwtToken(Token);
        }
    }
}