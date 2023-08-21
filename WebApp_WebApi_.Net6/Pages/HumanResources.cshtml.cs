using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using WebApp_WebApi_.Net6.Authorization;
using WebApp_WebApi_.Net6.Dto;

namespace WebApp_.Net6.Pages
{
    [Authorize(Policy = "BelongHumanResources")]
    public class HumanResourcesModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public List<WeatherForecastDTO>? WeatherForecastItems { get; set; }

        public HumanResourcesModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task OnGetAsync()
        {

            WeatherForecastItems = await InvokeEndPoint<List<WeatherForecastDTO>>("OurWebAPI", "WeatherForecast");
        }

        private async Task<T> InvokeEndPoint<T>(string clientName, string url)
        {
            JwtToken token = null;

            var strToken = HttpContext.Session.GetString("access_token");

            if (string.IsNullOrEmpty(strToken))
            {
                token = await Authenticate();
            }
            else
            {
                token = JsonConvert.DeserializeObject<JwtToken>(strToken);
            }

            if (token == null || string.IsNullOrEmpty(token.Access) || token.ExpiresAt <= DateTime.UtcNow)
            {
                token = await Authenticate();
            }


            var httpClient = _httpClientFactory.CreateClient(clientName);

            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Access);

            return await httpClient.GetFromJsonAsync<T>(url);
        }

        private async Task<JwtToken> Authenticate()
        {
            var httpClient = _httpClientFactory.CreateClient("OurWebAPI");
            var resp = await httpClient.PostAsJsonAsync("api/auth", new Credential { UserName = "jugalo1713", Password = "123" });

            resp.EnsureSuccessStatusCode();

            string jwt = await resp.Content.ReadAsStringAsync();
            HttpContext.Session.SetString("access_token", jwt);
            var test = JsonConvert.DeserializeObject<JwtToken>(jwt);

            return JsonConvert.DeserializeObject<JwtToken>(jwt);
        }
    }
}
