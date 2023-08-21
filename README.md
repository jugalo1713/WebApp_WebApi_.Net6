# Authentication Project 

## Description

This project was build in Udemy course [Master ASP.NET Core Identity: Authentication & Authorization](https://www.udemy.com/course/complete-guide-to-aspnet-core-identity/).
This project is an easy and first approach to Authentication and authorization in .Net using token approach using a webapp and an API

## Stack
- .Net 6
- Razor pages
- Bootstrap

# Configuration in API

Configuration is added to Program.cs

- AddAuthentication() adds authentication configuration 
    - Sets jwt as default schema
    - Adds Jwt configuration 
- AddAuthorization() Adds authorization configuration  includind Admin only policy

```c#
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration.GetValue<string>("SecretKey"))),
        ValidateLifetime = true,
        ValidateAudience = false,
        ValidateIssuer = false,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim("Admin"));
});

//---
app.UseAuthentication();
app.UseAuthorization();
```

Then WeatherForecast controller has [Authorize(policy: "AdminOnly")]

## Configuration in Web App

Human Resources is the only page that includes a call to the API, these packages are needed:
- Microsoft.AspNetCore.Http.Extensions (Note that is deprecated)
- Newtonsoft.Json To manage json objects


Configure Program.cs
- AddHttpClient() configures HttpClient using a base address
- AddSession() Creates the session so that when browser is closed continues to work
 ```c#
 builder.Services.AddHttpClient("OurWebAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:44323/");
});

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.IsEssential = true;
});
///----
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
 ```

 HumanResources page includes [Authorize(Policy = "BelongHumanResources")]

 Generic methods to handle authentication in webapp

 - Authenticate() 
    - Instantiate httpClient
    - makes a request to auth api with credentials
    - reads jwt from response
    - Sets session with jwt
    - Deserialize object into a class

- InvokeEndPoint is a generics method that receives client name and url to request access
    - Looks for token in session
    - If not found calls for authenticate()
    - If found retrieves token 
    - Adds authorization headers to request including access token
    - Makes the request to the url

```c#
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

        return JsonConvert.DeserializeObject<JwtToken>(jwt);
    }
 ```