using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Xunmei.IdentityServer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using IdentityModel.Client;
using System.Globalization;

namespace IdentityServer.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public async Task<IActionResult> Api()
        {
            ViewData["Message"] = "调用API Server返回值";
             var token = await HttpContext.GetTokenAsync("access_token");
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.BaseAddress = new Uri("http://localhost:5001");
                ViewData["ApiResult"] = await client.GetStringAsync("/api/identity");
            }
            
            return View();
        }

        [Authorize]
        public async Task RefreshToken()
        {
            var authorizationServerInfo = await DiscoveryClient.GetAsync("http://localhost:5000/");
            var client = new TokenClient(authorizationServerInfo.TokenEndpoint, "mvc_code", "12345");
            var refreshToken = await HttpContext.GetTokenAsync("refresh_token");
            var response = await client.RequestRefreshTokenAsync(refreshToken);
            var identityToken = await HttpContext.GetTokenAsync("identity_token");
            var expiresAt = DateTime.UtcNow + TimeSpan.FromSeconds(response.ExpiresIn);
            var tokens = new[]
            {
                new AuthenticationToken
                {
                    Name = OpenIdConnectParameterNames.IdToken,
                    Value = identityToken
                },
                new AuthenticationToken
                {
                    Name = OpenIdConnectParameterNames.AccessToken,
                    Value = response.AccessToken
                },
                new AuthenticationToken
                {
                    Name = OpenIdConnectParameterNames.RefreshToken,
                    Value = response.RefreshToken
                },
                new AuthenticationToken
                {
                    Name = "expires_at",
                    Value = expiresAt.ToString("o", CultureInfo.InvariantCulture)
                }
            };
            var authenticationInfo = await HttpContext.AuthenticateAsync("Cookies");
            authenticationInfo.Properties.StoreTokens(tokens);
            await HttpContext.SignInAsync("Cookies", authenticationInfo.Principal, authenticationInfo.Properties);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            await HttpContext.SignOutAsync("oidc");
        }
    }
}
