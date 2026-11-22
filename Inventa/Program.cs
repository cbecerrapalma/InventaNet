using CBP.Data.Synchronous;               // AddUnitOfWorkSync, AddRepositorySync, RegisterRepositoryMappingSync
using CBP.MultiTenant.Extensions;
using CBP.MultiTenant.Registration;
using CBP.WinFrms.Notification;
using CBP.WinFrms.Theming;
using Inventa.Application;                  // AddApplication
using Inventa.Data.Context;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;
using Inventa.Forms;
using Inventa.Forms.Compras;
using Inventa.Forms.Mantenedores;
using Inventa.Forms.Productos;
using Inventa.Forms.Utilidades;
using Inventa.Infrastructure;
using Inventa.Infrastructure.Tenancy;
using Inventa.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OfficeOpenXml;
using System.Reflection;

namespace Inventa;

internal static class Program
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    [STAThread]
    static async Task Main()
    {
        ExcelPackage.License.SetNonCommercialPersonal("CBecerra");

        ApplicationConfiguration.Initialize();

        var host = CreateHostBuilder().Build();
        ServiceProvider = host.Services;

        await host.InitializeTenantAsync("ABARROTES");

        ThemeManager.Instance.Initialize();

        using var scope =
            ServiceProvider.CreateScope();

        System.Windows.Forms.Application.Run(
            scope.ServiceProvider
                .GetRequiredService<frm_Producto>());

        //System.Windows.Forms.Application.Run(ServiceProvider.GetRequiredService<frm_Producto>());

        ThemeManager.Instance.Shutdown();
    }

    static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // ===== CONFIGURACIÓN DE BASE DE DATOS =====
                // S57.13 P0-1: password sa solo desde environment, nunca en archivos.
                var connectionString = context.Configuration.GetConnectionString("DefaultConnection")!;
                const string placeholder = "__INVENTA_SQL_PASSWORD__";
                if (connectionString.Contains(placeholder, StringComparison.Ordinal))
                {
                    var dbPassword = Environment.GetEnvironmentVariable("INVENTA_SQL_PASSWORD");
                    if (string.IsNullOrWhiteSpace(dbPassword))
                        throw new InvalidOperationException("INVENTA_SQL_PASSWORD no configurado en environment. WinForms no puede conectar.");
                    connectionString = connectionString.Replace(placeholder, dbPassword, StringComparison.Ordinal);
                }
                services.AddDbContext<InventaDbContext>(options =>
                {
                    options.UseSqlServer(
                        connectionString,
                        sql =>
                        {
                            sql.EnableRetryOnFailure(
                                maxRetryCount: 5,
                                maxRetryDelay: TimeSpan.FromSeconds(30),
                                errorNumbersToAdd: null);
                            sql.CommandTimeout(60);
                        });
                });

                // ===== UNIT OF WORK SINCRÓNICO (WinForms) =====
                services.AddUnitOfWorkSync<InventaDbContext>();

                // ===== REGISTRO DE REPOSITORIOS PERSONALIZADOS =====
                // Escaneo automático de repositorios, se realiza en el método AddRepositories() que busca clases que implementen interfaces terminadas en "Repository"
                AddRepositories(services);

                // ===== CAPA DE APLICACIÓN (Servicios, AutoMapper, FluentValidation) =====
                services.AddApplication();

                // ===== MULTI-TENANT =====
                services.AddMultiTenant<Tenant, InventaTenantMapper, InventaTenantResolver>();

                // ===== SISTEMA DE NOTIFICACIONES =====
                NotificationManager.Configure(options =>
                {
                    options.DefaultDurationMs = 4000;
                    options.DefaultPosition = NotificationPosition.BottomRight;
                    options.MaxConcurrent = 5;
                    options.CornerRadius = 8;
                    options.Opacity = 0.95;
                    options.TitleFontSize = 14f;
                    options.MessageFontSize = 12f;
                    options.MessageFontName = "Segoe UI";
                    options.TitleFontName = "Segoe UI";
                    options.SuccessColor = AppColors.Success;
                    options.WarningColor = AppColors.Warning;
                    options.ErrorColor = AppColors.Error;
                    options.InfoColor = AppColors.Primary;
                    options.TextColor = Color.White;
                });

                // ===== FÁBRICA DE FORMULARIOS =====
                services.AddSingleton<IFormFactory, FormFactory>();

                // ===== FORMULARIOS =====
                services.AddTransient<frm_Principal>();

                // Compras
                services.AddTransient<frm_OrdenCompra>();

                // Mantenedores
                services.AddTransient<frm_Bodega>();
                services.AddTransient<frm_Categoria>();
                services.AddTransient<frm_Cliente>();
                services.AddTransient<frm_Marca>();
                services.AddTransient<frm_MetodoPago>();
                services.AddTransient<frm_Proveedor>();
                services.AddTransient<frm_Tenant>();
                services.AddTransient<frm_Unidad>();

                // Productos
                //services.AddTransient<frm_AtributoDef>();
                //services.AddTransient<frm_AtributoValor>();
                //services.AddTransient<frm_CodigoBarra>();
                //services.AddTransient<frm_ConfigAtributos>();
                //services.AddTransient<frm_Precio>();
                //services.AddTransient<frm_Presentacion>();
                services.AddTransient<frm_Producto>();
                //services.AddTransient<frm_Stock>();
                //services.AddTransient<frm_StockAjuste>();
                //services.AddTransient<frm_StockTransferir>();
                //services.AddTransient<frm_Variante>();
                //services.AddTransient<Producto>();

                // Utilidades
                services.AddTransient<frm_ImportacionExportacion>();
                services.AddTransient<FrmImportacionProductos>();
            });
    }

    /// <summary>
    /// Escanea el ensamblado que contiene los repositorios (Inventa.Data) y registra todas las interfaces
    /// que terminan con "Repository" junto con su implementación concreta como Scoped.
    /// </summary>
    private static void AddRepositories(IServiceCollection services)
    {
        var assembly = Assembly.Load("Inventa.Data");

        var repositoryTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces(), (type, iface) => new { type, iface })
            .Where(t => t.iface.Name.EndsWith("Repository"))
            .ToList();

        foreach (var repo in repositoryTypes)
        {
            // Registrar la interfaz con su implementación concreta como Scoped
            services.AddScoped(repo.iface, repo.type);
        }
    }
}

