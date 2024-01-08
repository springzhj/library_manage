using Microsoft.Extensions.Options;
using System.Security.Claims;
using PloliticalScienceSystemApi.Extension;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace PloliticalScienceSystemApi.JwtExtension
{
    public class JwtInvorker
    {
        private readonly JWTTokenOptions _JWTTokenOptions;
        public JwtInvorker(IOptionsMonitor<JWTTokenOptions> jwtTokenOptions)
        {
            this._JWTTokenOptions = jwtTokenOptions.CurrentValue;
        }
        /// <summary>
        /// 获取token字符串
        /// </summary>
        /// <returns>Bearer token</returns>
        public string GetTokenStr<T>(T t, Func<T, List<Claim>> GennerateClaimes)
        {
            var credentials = new SigningCredentials(new RsaSecurityKey(RSAFileHelper.GetPrivateKey()), SecurityAlgorithms.RsaSha256);
            var token = new JwtSecurityToken(
                issuer: _JWTTokenOptions.Issuer,
                audience: _JWTTokenOptions.Audience,
                claims: GennerateClaimes(t),
                expires: DateTime.Now.AddSeconds(_JWTTokenOptions.Expiration),
                signingCredentials: credentials);
            var tokenHandler = new JwtSecurityTokenHandler();
            var encodedToken = tokenHandler.WriteToken(token);
            return "Bearer " + encodedToken;
        }
    }
}
