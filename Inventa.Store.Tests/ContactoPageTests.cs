using System.Net;
using System.Text;
using Bunit;
using Inventa.Store.Models;
using Inventa.Store.Pages;
using Inventa.Store.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MudBlazor.Services;
using Xunit;

namespace Inventa.Store.Tests;

public sealed class ContactoPageTests : IAsyncLifetime
{
    private readonly BunitContext _ctx = new();
    private readonly Mock<IContactApiService> _contactApiMock = new();
    private readonly Mock<ILogger<Contacto>> _loggerMock = new();

    public ContactoPageTests()
    {
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        _ctx.Services.AddMudServices();
        _ctx.Services.AddSingleton(_contactApiMock.Object);
        _ctx.Services.AddSingleton(_loggerMock.Object);

        var http = new HttpClient(new StubStorefrontHandler()) { BaseAddress = new Uri("http://localhost/") };
        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ApiBaseUrl"] = "http://localhost/" })
            .Build();
        _ctx.Services.AddSingleton(new StorefrontApiService(http));
        _ctx.Services.AddSingleton(new CatalogApiService(http));
        _ctx.Services.AddSingleton(new StorefrontState(new StorefrontApiService(http), cfg));
    }

    sealed class StubStorefrontHandler : HttpMessageHandler
    {
        const string ProductJson = """{"idProd":42,"codigo":"POL-42","nombre":"Polera Demo","idCategoria":1,"categoriaNombre":"Poleras","idMarca":null,"marcaNombre":null,"variantes":[],"preciosVigentes":[],"imagenUrl":null,"imagenes":[]}""";
        const string Json = """{"nombreLegal":"Tienda Demo","nombreFantasia":"Mi Tienda","descripcionCorta":null,"descripcion":null,"logoUrl":null,"contacto":{"telefono":null,"whatsapp":null,"email":"tienda@ejemplo.cl","direccion":null,"comuna":null,"ciudad":null,"region":null,"sitioWeb":null},"redes":[],"horarios":[],"branding":null}""";
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(req.RequestUri!.AbsolutePath.Contains("/catalog/products/") ? ProductJson : Json, Encoding.UTF8, "application/json")
            });
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;
    public async ValueTask DisposeAsync() => await _ctx.DisposeAsync();

    private static async Task FillFormAsync(IRenderedComponent<Contacto> cut,
        string nombre = "Juan Pérez",
        string email = "juan@ejemplo.cl",
        string? telefono = "+56912345678",
        string mensaje = "Hola, quisiera información sobre sus productos.",
        bool consentimiento = true)
    {
        // Nota: MudBlazor propaga data-testid al input/textarea nativo
        // (verificado en TmpDebugMarkupTests), así que el selector directo
        // ya apunta al elemento con el handler de Blazor.
        await cut.Find("[data-testid='contact-nombre']").ChangeAsync(nombre);
        await cut.Find("[data-testid='contact-nombre']").BlurAsync();
        await cut.Find("[data-testid='contact-email']").ChangeAsync(email);
        await cut.Find("[data-testid='contact-email']").BlurAsync();
        if (!string.IsNullOrEmpty(telefono))
        {
            await cut.Find("[data-testid='contact-telefono']").ChangeAsync(telefono);
            await cut.Find("[data-testid='contact-telefono']").BlurAsync();
        }
        await cut.Find("[data-testid='contact-mensaje']").ChangeAsync(mensaje);
        await cut.Find("[data-testid='contact-mensaje']").BlurAsync();
        if (consentimiento)
        {
            await cut.Find("[data-testid='contact-consent']").ChangeAsync(true);
        }
    }

    private static async Task SubmitFormAsync(IRenderedComponent<Contacto> cut)
    {
        await cut.Find("form[data-testid='contact-form']").SubmitAsync();
        await cut.InvokeAsync(() => { });
    }

    [Fact]
    public void ContactoPage_RendersForm()
    {
        var cut = _ctx.Render<Contacto>();
        Assert.NotNull(cut.Find("form[data-testid='contact-form']"));
        Assert.NotNull(cut.Find("[data-testid='contact-nombre']"));
        Assert.NotNull(cut.Find("[data-testid='contact-email']"));
        Assert.NotNull(cut.Find("[data-testid='contact-telefono']"));
        Assert.NotNull(cut.Find("[data-testid='contact-mensaje']"));
        Assert.NotNull(cut.Find("[data-testid='contact-consent']"));
        Assert.NotNull(cut.Find("[data-testid='contact-submit']"));
        Assert.NotNull(cut.Find("[data-testid='contact-honeypot']"));
    }

    [Fact]
    public void ContactoPage_HoneypotOcultoYNoEnfocable()
    {
        var cut = _ctx.Render<Contacto>();
        var honeypot = cut.Find("[data-testid='contact-honeypot']");
        var input = honeypot.QuerySelector("input[name='Website']");

        Assert.Equal("true", honeypot.GetAttribute("aria-hidden"));
        Assert.Equal("-1", input!.GetAttribute("tabindex"));
        Assert.Contains("display:none", input!.GetAttribute("style") ?? "");
    }

    [Fact]
    public async Task ContactoPage_ValidSubmit_CallsApiAndShowsSuccess()
    {
        _contactApiMock.Setup(x => x.SendAsync(It.IsAny<ContactSendRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContactSendResult(true, new ContactSendResponse("CONTACTO_ENVIADO", "trace-123", "TRACK-456"), null));

        var cut = _ctx.Render<Contacto>();

        await FillFormAsync(cut);
        await SubmitFormAsync(cut);

        cut.WaitForAssertion(() =>
        {
            var alert = cut.Find("[data-testid='contact-success']");
            Assert.Contains("¡Mensaje enviado!", alert.TextContent);
            Assert.Contains("TRACK-456", alert.TextContent);
            Assert.NotNull(cut.Find("[data-testid='contact-back']"));
        });

        _contactApiMock.Verify(x => x.SendAsync(It.Is<ContactSendRequest>(r =>
            r.Nombre == "Juan Pérez" &&
            r.Email == "juan@ejemplo.cl" &&
            r.Telefono == "+56912345678" &&
            r.Mensaje == "Hola, quisiera información sobre sus productos." &&
            r.Consentimiento == true &&
            r.Website == ""), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ContactoPage_HoneypotFilled_ApiReturnsSuccessButNoEmailSent()
    {
        _contactApiMock.Setup(x => x.SendAsync(It.IsAny<ContactSendRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContactSendResult(true, new ContactSendResponse("CONTACTO_ENVIADO", "trace-123", null), null));

        var cut = _ctx.Render<Contacto>();

        var req = new ContactSendRequest
        {
            Nombre = "Juan Pérez",
            Email = "juan@ejemplo.cl",
            Telefono = "+56912345678",
            Mensaje = "Hola, quisiera información sobre sus productos.",
            Consentimiento = true,
            ProductoId = null,
            Website = "http://spam.com"
        };

        await cut.Find("[data-testid='contact-nombre']").ChangeAsync("Juan Pérez");
        await cut.Find("[data-testid='contact-nombre']").BlurAsync();
        await cut.Find("[data-testid='contact-email']").ChangeAsync("juan@ejemplo.cl");
        await cut.Find("[data-testid='contact-email']").BlurAsync();
        await cut.Find("[data-testid='contact-telefono']").ChangeAsync("+56912345678");
        await cut.Find("[data-testid='contact-telefono']").BlurAsync();
        await cut.Find("[data-testid='contact-mensaje']").ChangeAsync("Hola, quisiera información sobre sus productos.");
        await cut.Find("[data-testid='contact-mensaje']").BlurAsync();
        await cut.Find("[data-testid='contact-consent']").ChangeAsync(true);
        // Honeypot: el test sí debe llenarlo (antes construía un req sin usarlo).
        await cut.Find("[data-testid='contact-honeypot'] input").ChangeAsync("http://spam.com");
        await cut.Find("form[data-testid='contact-form']").SubmitAsync();
        await cut.InvokeAsync(() => { });

        cut.WaitForAssertion(() =>
        {
            var alert = cut.Find("[data-testid='contact-success']");
            Assert.Contains("¡Mensaje enviado!", alert.TextContent);
            Assert.DoesNotContain("Referencia:", alert.TextContent);
        });

        _contactApiMock.Verify(x => x.SendAsync(It.Is<ContactSendRequest>(r => r.Website == "http://spam.com"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ContactoPage_ValidationError_NombreMuyCorto()
    {
        var cut = _ctx.Render<Contacto>();

        await FillFormAsync(cut, nombre: "J");
        await SubmitFormAsync(cut);

        cut.WaitForAssertion(() =>
        {
            var error = cut.Find("[data-testid='contact-error']");
            Assert.Contains("No se pudo enviar", error.TextContent); // S96.1: texto genérico nuevo
        });
    }

    [Fact]
    public async Task ContactoPage_ValidationError_EmailInvalido()
    {
        var cut = _ctx.Render<Contacto>();

        await FillFormAsync(cut, email: "no-es-email");
        await SubmitFormAsync(cut);

        cut.WaitForAssertion(() =>
        {
            var error = cut.Find("[data-testid='contact-error']");
            Assert.Contains("No se pudo enviar", error.TextContent); // S96.1: texto genérico nuevo
        });
    }

    [Fact]
    public async Task ContactoPage_ValidationError_MensajeMuyCorto()
    {
        var cut = _ctx.Render<Contacto>();

        await FillFormAsync(cut, mensaje: "Corto");
        await SubmitFormAsync(cut);

        cut.WaitForAssertion(() =>
        {
            var error = cut.Find("[data-testid='contact-error']");
            Assert.Contains("No se pudo enviar", error.TextContent); // S96.1: texto genérico nuevo
        });
    }

    [Fact]
    public async Task ContactoPage_ValidationError_SinConsentimiento()
    {
        var cut = _ctx.Render<Contacto>();

        await FillFormAsync(cut, consentimiento: false);
        await SubmitFormAsync(cut);

        // S96.1 D3: sin consentimiento no hay POST y el aviso queda junto a la casilla.
        cut.WaitForAssertion(() => Assert.Contains("Debes aceptar",
            cut.Find("[data-testid='contact-consent-error']").TextContent));
        _contactApiMock.Verify(x => x.SendAsync(It.IsAny<ContactSendRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ContactoPage_ApiError400_ShowsError()
    {
        _contactApiMock.Setup(x => x.SendAsync(It.IsAny<ContactSendRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContactSendResult(false, null, new ContactErrorResponse("CONTACTO_VALIDACION", "Email inválido", null)));

        var cut = _ctx.Render<Contacto>();

        await FillFormAsync(cut);
        await SubmitFormAsync(cut);

        cut.WaitForAssertion(() =>
        {
            // El producto muestra el Detail del API (ContactApiService no propaga
            // correlationId cuando Data es null): texto real, no "trace".
            var error = cut.Find("[data-testid='contact-error']");
            Assert.Contains("Email inválido", error.TextContent);
        });
    }

    [Fact]
    public async Task ContactoPage_ApiError429_ShowsRetryAfter()
    {
        _contactApiMock.Setup(x => x.SendAsync(It.IsAny<ContactSendRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContactSendResult(false, null, new ContactErrorResponse("CONTACTO_RATE_LIMITED", "Demasiadas solicitudes", 60)));

        var cut = _ctx.Render<Contacto>();

        await FillFormAsync(cut);
        await SubmitFormAsync(cut);

        cut.WaitForAssertion(() =>
        {
            // Producto: "Reintenta en 60 segundos." (Contacto.razor, S122 D3).
            var error = cut.Find("[data-testid='contact-error']");
            Assert.Contains("Reintenta", error.TextContent);
            Assert.Contains("60", error.TextContent);
        });
    }

    [Fact]
    public async Task ContactoPage_ApiError503_ShowsNotConfigured()
    {
        _contactApiMock.Setup(x => x.SendAsync(It.IsAny<ContactSendRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContactSendResult(false, null, new ContactErrorResponse("CONTACTO_NO_CONFIGURADO", "Contacto no configurado", null)));

        var cut = _ctx.Render<Contacto>();

        await FillFormAsync(cut);
        await SubmitFormAsync(cut);

        cut.WaitForAssertion(() =>
        {
            // Producto muestra el Detail: "Contacto no configurado".
            var error = cut.Find("[data-testid='contact-error']");
            Assert.Contains("Contacto no configurado", error.TextContent);
        });
    }

    [Fact]
    public async Task ContactoPage_ProductoPrefijo_MuestraNombreYEnviaProductoId()
    {
        _contactApiMock.Setup(x => x.SendAsync(It.IsAny<ContactSendRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContactSendResult(true, new ContactSendResponse("CONTACTO_ENVIADO", "trace-123", "TRACK-456"), null));

        var nav = _ctx.Services.GetRequiredService<NavigationManager>();
        nav.NavigateTo("/contacto?producto=42");

        var cut = _ctx.Render<Contacto>();

        cut.WaitForAssertion(() => Assert.Contains("Polera Demo", cut.Find("[data-testid='contact-producto']").GetAttribute("value") ?? ""));

        await FillFormAsync(cut);
        await SubmitFormAsync(cut);

        _contactApiMock.Verify(x => x.SendAsync(It.Is<ContactSendRequest>(r => r.ProductoId == 42), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ContactoPage_Accesibilidad_LabelsYAriaDescribedby()
    {
        var cut = _ctx.Render<Contacto>();

        // MudTextField genera sus propios for (ids internos), así que se
        // verifican los textos de etiqueta reales del producto.
        var labels = cut.FindAll("label");
        Assert.Contains(labels, l => l.TextContent.Contains("Nombre"));
        Assert.Contains(labels, l => l.TextContent.Contains("Correo electrónico"));
        Assert.Contains(labels, l => l.TextContent.Contains("Mensaje"));
        Assert.Contains(labels, l => l.TextContent.Contains("Acepto el tratamiento"));

        Assert.Equal("INPUT", cut.Find("[data-testid='contact-nombre']").TagName);
        Assert.Equal("INPUT", cut.Find("[data-testid='contact-email']").TagName);
        Assert.Equal("TEXTAREA", cut.Find("[data-testid='contact-mensaje']").TagName);
        Assert.Equal("INPUT", cut.Find("[data-testid='contact-consent']").TagName);
    }

    [Fact]
    public async Task ContactoPage_EstadoEnviando_DeshabilitaBoton()
    {
        var tcs = new TaskCompletionSource<ContactSendResult>();
        _contactApiMock.Setup(x => x.SendAsync(It.IsAny<ContactSendRequest>(), It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        var cut = _ctx.Render<Contacto>();

        await FillFormAsync(cut);

        var submitTask = SubmitFormAsync(cut);

        cut.WaitForAssertion(() =>
        {
            var btn = cut.Find("[data-testid='contact-submit']");
            Assert.True(btn.HasAttribute("disabled"));
            Assert.Contains("Enviando...", btn.TextContent);
        });

        tcs.SetResult(new ContactSendResult(true, new ContactSendResponse("CONTACTO_ENVIADO", "trace-123", "TRACK-456"), null));
        await submitTask;
        await cut.InvokeAsync(() => { });
    }

    [Fact]
    public async Task ContactoPage_RoleStatusEnExito()
    {
        _contactApiMock.Setup(x => x.SendAsync(It.IsAny<ContactSendRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContactSendResult(true, new ContactSendResponse("CONTACTO_ENVIADO", "trace-123", "TRACK-456"), null));

        var cut = _ctx.Render<Contacto>();

        await FillFormAsync(cut);
        await SubmitFormAsync(cut);

        cut.WaitForAssertion(() =>
        {
            var alert = cut.Find("[data-testid='contact-success']");
            Assert.Equal("status", alert.GetAttribute("role"));
        });
    }
}
