using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace EBScan
{
    public class TokenService
    {
        private string _bearerToken = "";
        private DateTime _bearerTokenExpires = DateTime.MinValue;
        private readonly HttpClient _httpClient;

        public TokenService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetTokenAsync()
        {
            if (DateTime.Now >= _bearerTokenExpires)
            {
                await RefreshBearerTokenAsync();
            }
            return _bearerToken;
        }

        private async Task RefreshBearerTokenAsync()
        {
            string username = string.IsNullOrEmpty(Properties.Settings.Default.FanAuthUsername)
                ? Properties.Settings.Default.AuthUsername
                : Properties.Settings.Default.FanAuthUsername;
            string password = string.IsNullOrEmpty(Properties.Settings.Default.FanAuthPassword)
                ? Properties.Settings.Default.AuthPassword
                : Properties.Settings.Default.FanAuthPassword;
            string url = $"https://api.fancourier.ro/login?username={username}&password={password}";
            using (var response = await _httpClient.PostAsync(url, null))
            {
                response.EnsureSuccessStatusCode();
                string jsonData = await response.Content.ReadAsStringAsync();
                var js = new JavaScriptSerializer();
                dynamic data = js.Deserialize<dynamic>(jsonData);
                if (data["status"] == "success")
                {
                    _bearerToken = data["data"]["token"];
                    _bearerTokenExpires = DateTime.Parse(data["data"]["expiresAt"]);

                    Debug.WriteLine($"DEBUG RefreshBearerTokenAsync(): {_bearerToken}");
                    Debug.WriteLine($"DEBUG RefreshBearerTokenAsync(): {_bearerTokenExpires}");
                }
            }
        }
    }
}
