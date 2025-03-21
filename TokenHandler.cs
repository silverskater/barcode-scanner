using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Diagnostics;

public class TokenHandler : DelegatingHandler
{
    private readonly TokenService tokenService;

    public TokenHandler(TokenService tokenService) : base(new HttpClientHandler())
    {
        this.tokenService = tokenService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string token = await tokenService.GetTokenAsync();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        try
        {
            return await base.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"TokenHandler: Exception {ex.Message}");
            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }
    }
}
