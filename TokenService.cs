using EBScan;
using System.Net.Http;
using Microsoft.Win32;
using System;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Diagnostics;

public class TokenService
{
    private string bearerToken = "";
    private DateTime bearerTokenExpires = DateTime.MinValue;
    private readonly HttpClient httpClient;
    private readonly string username = "clienttest";
    private readonly string password = "testing";

    public TokenService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<string> GetTokenAsync()
    {
        Debug.WriteLine($"DEBUG GetTokenAsync(): {bearerTokenExpires.ToString()}");

        if (DateTime.Now >= bearerTokenExpires)
        {
            await RefreshBearerTokenAsync();
        }
        return bearerToken;
    }

    private async Task RefreshBearerTokenAsync()
    {
        Debug.WriteLine($"DEBUG RefreshBearerTokenAsync(): {username}");
        string url = "https://api.fancourier.ro/login?username=" + username + "&password=" + password;
        using (var response = await httpClient.PostAsync(url, null))
        {
            response.EnsureSuccessStatusCode();
            string jsonData = await response.Content.ReadAsStringAsync();
            JavaScriptSerializer js = new JavaScriptSerializer();
            dynamic data = js.Deserialize<dynamic>(jsonData);
            if (data["status"] == "success")
            {
                bearerToken = data["data"]["token"];
                bearerTokenExpires = DateTime.Parse(data["data"]["expiresAt"]);

                Debug.WriteLine($"DEBUG RefreshBearerTokenAsync(): {bearerToken}");
        Debug.WriteLine($"DEBUG RefreshBearerTokenAsync(): {bearerTokenExpires.ToString()}");
            }
        }
    }
}
