using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Inventa.WebAPI.Clients;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Inventa.WebAPI.Tests;

public sealed class PublicContactRateLimitTests : IClassFixture<ContactRateLimitTestFactory>
{
    private readonly HttpClient _client;
    private readonly FakePassPlatClientForContact _fakePassPlat;

    public PublicContactRateLimitTests(ContactRateLimitTestFactory factory)
    {
        _fakePassPlat = factory.Fake;
        _fakePassPlat.Reset();
        _client = factory.CreateClient();
    }

    private static object ValidRequest(string website = "")
        => new
        {
            nombre = "Juan Pérez",
            email = "juan@ejemplo.cl",
            telefono = "+56912345678",
            mensaje = "Hola, quisiera información sobre sus productos.",
            consentimiento = true,
            productoId = (long?)null,
            website = website
        };

    [Fact]
    public async Task RateLimitPerIP_Returns429ConRetryAfter()
    {
        for (int i = 0; i < 5; i++)
        {
            var r = await _client.PostAsJsonAsync("api/public/contacto", ValidRequest(), Xunit.TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.Accepted, r.StatusCode);
        }

        var resp = await _client.PostAsJsonAsync("api/public/contacto", ValidRequest(), Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.TooManyRequests, resp.StatusCode);

        Assert.True(resp.Headers.TryGetValues("Retry-After", out var values));
        var retryAfter = values.FirstOrDefault();
        Assert.NotNull(retryAfter);
        Assert.True(int.TryParse(retryAfter, out var seconds));
        Assert.True(seconds > 0);
    }
}
