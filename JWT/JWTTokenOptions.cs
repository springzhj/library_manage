namespace LibraryManageSystemApi.JwtExtension
{
    public class JWTTokenOptions
    {
        public string? Audience { get; set; }

        public string? Issuer { get; set; }

        public string? DefaultScheme { get; set; }
        public string? PolicyName { get; set; }
        public bool IsHttps { get; set; }
        /// <summary>
        /// 失效时间(s)
        /// </summary>
        public int Expiration { get; set; }
        /// <summary>
        /// 刷新时间(s)
        /// </summary>
        public int ReExpiration { get; set; }
    }
}
