using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Inventa.Store.Services;
using Microsoft.Extensions.Logging.Abstractions;
using RichardSzalay.MockHttp;
using Xunit;

namespace Inventa.Store.Tests;

/// <summary>S79.2: ContactApiService real — mapeo amable de errores (nunca JSON crudo ni mensajes técnicos).</summary>
public sealed class ContactApiServiceTests
{
    private static ContactSendRequest ValidRequest() => new()
    {
        Nombre = "Juan Pérez",
        Email = "juan@ejemplo.cl",
        Mensaje = "Hola, quisiera información sobre sus productos.",
        Consentimiento = true,
        Website = ""
    };

    private static ContactApiService Sut(MockHttpMessageHandler h) =>
        new(new HttpClient(h) { BaseAddress = new Uri("https://api.test/") }, NullLogger<ContactApiService>.Instance);

    [Fact]
    public async Task Problema503_MuestraTitle_NoJsonCrudo()
    {
        var h = new MockHttpMessageHandler();
        h.When(HttpMethod.Post, "https://api.test/api/public/contacto")
            .Respond(HttpStatusCode.ServiceUnavailable, "application/problem+json",
                """{"type":"https://tools.ietf.org/html/rfc9110#section-15.6.4","title":"Contacto no configurado","status":503,"detail":"CONTACTO_NO_CONFIGURADO","code":"CONTACTO_NO_CONFIGURADO"}""");

        var r = await Sut(h).SendAsync(ValidRequest(), Xunit.TestContext.Current.CancellationToken);

        Assert.False(r.Success);
        Assert.Equal("CONTACTO_NO_CONFIGURADO", r.Error!.Code);
        // S96.1 D3: texto amable propio, nunca el detail técnico del servidor.
        Assert.Equal("El formulario de contacto no está disponible por ahora. Intenta más tarde.", r.Error!.Detail);
    }

    // S96.1 D3: CONTACTO_VALIDACION muestra el detail del servidor (mensaje pensado para el usuario).
    [Fact]
    public async Task Problema400Validacion_MuestraDetail()
    {
        var h = new MockHttpMessageHandler();
        h.When(HttpMethod.Post, "https://api.test/api/public/contacto")
            .Respond(HttpStatusCode.BadRequest, "application/problem+json",
                """{"title":"Solicitud inválida","status":400,"detail":"Consentimiento: Debe aceptar el tratamiento de datos.","code":"CONTACTO_VALIDACION"}""");

        var r = await Sut(h).SendAsync(ValidRequest(), Xunit.TestContext.Current.CancellationToken);

        Assert.False(r.Success);
        Assert.Equal("CONTACTO_VALIDACION", r.Error!.Code);
        Assert.Equal("Consentimiento: Debe aceptar el tratamiento de datos.", r.Error!.Detail);
    }

    [Fact]
    public async Task Problema429_PropagaRetryAfterConMensajeAmable()
    {
        var h = new MockHttpMessageHandler();
        h.When(HttpMethod.Post, "https://api.test/api/public/contacto")
            .Respond(req =>
            {
                var resp = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
                {
                    Content = new StringContent("""{"title":"Demasiadas solicitudes","status":429,"code":"CONTACTO_RATE_LIMITED"}""")
                };
                resp.Headers.TryAddWithoutValidation("Retry-After", "45");
                return resp;
            });

        var r = await Sut(h).SendAsync(ValidRequest(), Xunit.TestContext.Current.CancellationToken);

        Assert.False(r.Success);
        Assert.Equal("CONTACTO_RATE_LIMITED", r.Error!.Code);
        Assert.Equal(45, r.Error!.RetryAfter);
        Assert.DoesNotContain("{", r.Error!.Detail);
    }

    [Fact]
    public async Task CuerpoNoJson_UsaDefectoAmableSinFiltrar()
    {
        var h = new MockHttpMessageHandler();
        h.When(HttpMethod.Post, "https://api.test/api/public/contacto")
            .Respond(HttpStatusCode.BadGateway, "text/plain", "<html>proxy error secreto</html>");

        var r = await Sut(h).SendAsync(ValidRequest(), Xunit.TestContext.Current.CancellationToken);

        Assert.False(r.Success);
        Assert.Equal("CONTACTO_UPSTREAM", r.Error!.Code);
        Assert.DoesNotContain("proxy", r.Error!.Detail);
        Assert.DoesNotContain("<", r.Error!.Detail);
    }

    [Fact]
    public async Task ExcepcionRed_MensajeAmableSinDetalleTecnico()
    {
        var h = new MockHttpMessageHandler();
        h.When(HttpMethod.Post, "https://api.test/api/public/contacto")
            .Throw(new HttpRequestException("Connection refused 10.0.0.99"));

        var r = await Sut(h).SendAsync(ValidRequest(), Xunit.TestContext.Current.CancellationToken);

        Assert.False(r.Success);
        Assert.Equal("CONTACTO_UPSTREAM", r.Error!.Code);
        Assert.DoesNotContain("10.0.0.99", r.Error!.Detail);
        Assert.DoesNotContain("refused", r.Error!.Detail);
    }
}

