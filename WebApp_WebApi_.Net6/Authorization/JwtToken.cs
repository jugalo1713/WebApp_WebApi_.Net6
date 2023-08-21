using Newtonsoft.Json;

namespace WebApp_WebApi_.Net6.Authorization
{
    public class JwtToken
    {
        [JsonProperty("access_token")]
        public string Access { get; set; }
        [JsonProperty("expires_at")]
        public DateTime ExpiresAt { get; set; }
    }
}
