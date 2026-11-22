using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public sealed record CreatePromocionInput(
    string Codigo,
    string Nombre,
    string? TextoComercial,
    byte TipoMecanica,
    byte PoliticaPrecio,
    DateOnly FechaDesde,
    DateOnly? FechaHasta,
    short Prioridad,
    bool Exclusiva,
    bool Publicado);

public sealed record UpdatePromocionInput(
    string Nombre,
    string? TextoComercial,
    byte TipoMecanica,
    byte PoliticaPrecio,
    DateOnly FechaDesde,
    DateOnly? FechaHasta,
    short Prioridad,
    bool Exclusiva,
    bool Activo,
    bool Publicado);

public sealed record AddPresentacionInput(
    long IdPresProd,
    long? IdPrecioReferencia,
    byte Rol,
    decimal Cantidad,
    bool EsMuestra);

public sealed record UpdatePresentacionInput(byte Rol, decimal Cantidad);

public sealed record AddReglaInput(byte TipoRegla, byte Operador, string Valor);

public sealed record AddBeneficioInput(byte TipoBeneficio, decimal Valor);

public interface IPromocionServiceAsync
{
    Task<Result<Promocion>> CreateAsync(int idTenant, CreatePromocionInput input, CancellationToken ct = default);
    Task<Result<Promocion?>> GetByIdAsync(int idTenant, long idPromocion, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Promocion>>> GetByTenantAsync(int idTenant, CancellationToken ct = default);
    Task<Result<Promocion>> UpdateAsync(int idTenant, long idPromocion, UpdatePromocionInput input, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(int idTenant, long idPromocion, CancellationToken ct = default);
    Task<Result<PromocionPresentacion>> AddPresentacionAsync(int idTenant, long idPromocion, AddPresentacionInput input, CancellationToken ct = default);
    Task<Result<PromocionPresentacion>> UpdatePresentacionAsync(int idTenant, long idPart, UpdatePresentacionInput input, CancellationToken ct = default);
    Task<Result<bool>> DeletePresentacionAsync(int idTenant, long idPart, CancellationToken ct = default);
    Task<Result<PromocionRegla>> AddReglaAsync(int idTenant, long idPromocion, AddReglaInput input, CancellationToken ct = default);
    Task<Result<PromocionBeneficio>> AddBeneficioAsync(int idTenant, long idPromocion, AddBeneficioInput input, CancellationToken ct = default);
}

// S57.66: promoción ADMIN-ONLY / PERSISTENCE-READY. Aggregate service (único;
// §15: sin cuatro servicios artificiales). Sin Pricing Engine, sin stacking,
// sin POS, sin catálogo público. Referencia congelada: no existe update de
// IdPrecioReferencia (P16 §30).
public sealed class PromocionServiceAsync : IPromocionServiceAsync
{
    private readonly IPromocionRepositoryAsync _repo;
    private readonly IPromocionPresentacionRepositoryAsync _presentaciones;
    private readonly IPromocionReglaRepositoryAsync _reglas;
    private readonly IPromocionBeneficioRepositoryAsync _beneficios;
    private readonly IPrecioRepositoryAsync _precios;
    private readonly IPresProdRepositoryAsync _presProd;

    public PromocionServiceAsync(
        IPromocionRepositoryAsync repo,
        IPromocionPresentacionRepositoryAsync presentaciones,
        IPromocionReglaRepositoryAsync reglas,
        IPromocionBeneficioRepositoryAsync beneficios,
        IPrecioRepositoryAsync precios,
        IPresProdRepositoryAsync presProd)
    {
        _repo = repo;
        _presentaciones = presentaciones;
        _reglas = reglas;
        _beneficios = beneficios;
        _precios = precios;
        _presProd = presProd;
    }

    private static Result<T> Bad<T>(string code, string msg) =>
        Result<T>.Failure(new Error(code, msg).WithType(ErrorType.Validation).WithHttpStatus(400));

    private static Result<T> Unproc<T>(string code, string msg) =>
        Result<T>.Failure(new Error(code, msg).WithType(ErrorType.Validation).WithHttpStatus(422));

    private static Result<T> NotFound<T>(string code, string msg) =>
        Result<T>.Failure(new Error(code, msg).WithType(ErrorType.NotFound).WithHttpStatus(404));

    private static Result<T> Conflict<T>(string code, string msg) =>
        Result<T>.Failure(new Error(code, msg).WithType(ErrorType.Conflict).WithHttpStatus(409));

    public async Task<Result<Promocion>> CreateAsync(int idTenant, CreatePromocionInput input, CancellationToken ct = default)
    {
        if (idTenant <= 0)
            return Bad<Promocion>("VALIDATION", "IdTenant debe ser >0");
        if (input == null)
            return Bad<Promocion>("VALIDATION", "Input requerido");
        var basic = ValidateHeader(input.Codigo, input.Nombre, input.TextoComercial, input.TipoMecanica,
            input.PoliticaPrecio, input.FechaDesde, input.FechaHasta);
        if (basic != null) return basic;

        var dupe = await _repo.GetByTenantAndCodigoAsync(idTenant, input.Codigo.Trim(), ct);
        if (dupe != null)
            return Conflict<Promocion>("PROMOCION_CODIGO_DUPLICADO", "Ya existe una promoción con ese código en el tenant");

        var entity = new Promocion
        {
            IdTenant = idTenant,
            Codigo = input.Codigo.Trim(),
            Nombre = input.Nombre.Trim(),
            TextoComercial = string.IsNullOrWhiteSpace(input.TextoComercial) ? null : input.TextoComercial.Trim(),
            TipoMecanica = input.TipoMecanica,
            PoliticaPrecio = input.PoliticaPrecio,
            FechaDesde = input.FechaDesde,
            FechaHasta = input.FechaHasta,
            Prioridad = input.Prioridad,
            Exclusiva = input.Exclusiva,
            Activo = true,
            Publicado = input.Publicado
        };
        await _repo.AddAsync(entity, ct);
        return Result<Promocion>.Success(entity);
    }

    public async Task<Result<Promocion?>> GetByIdAsync(int idTenant, long idPromocion, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdWithChildrenAsync(idPromocion, ct);
        if (e == null || e.IdTenant != idTenant) return Result<Promocion?>.Success(null, allowNull: true);
        return Result<Promocion?>.Success(e);
    }

    public async Task<Result<IReadOnlyList<Promocion>>> GetByTenantAsync(int idTenant, CancellationToken ct = default)
    {
        if (idTenant <= 0)
            return Bad<IReadOnlyList<Promocion>>("VALIDATION", "IdTenant debe ser >0");
        return Result<IReadOnlyList<Promocion>>.Success(await _repo.GetByTenantAsync(idTenant, ct));
    }

    public async Task<Result<Promocion>> UpdateAsync(int idTenant, long idPromocion, UpdatePromocionInput input, CancellationToken ct = default)
    {
        if (input == null)
            return Bad<Promocion>("VALIDATION", "Input requerido");
        var promo = await _repo.GetByIdAsync(idPromocion, ct);
        if (promo == null || promo.IdTenant != idTenant)
            return NotFound<Promocion>("PROMOCION_NOT_FOUND", "Promoción no encontrada");
        var basic = ValidateHeader("X", input.Nombre, input.TextoComercial, input.TipoMecanica,
            input.PoliticaPrecio, input.FechaDesde, input.FechaHasta);
        if (basic != null) return basic;

        promo.Nombre = input.Nombre.Trim();
        promo.TextoComercial = string.IsNullOrWhiteSpace(input.TextoComercial) ? null : input.TextoComercial.Trim();
        promo.TipoMecanica = input.TipoMecanica;
        promo.PoliticaPrecio = input.PoliticaPrecio;
        promo.FechaDesde = input.FechaDesde;
        promo.FechaHasta = input.FechaHasta;
        promo.Prioridad = input.Prioridad;
        promo.Exclusiva = input.Exclusiva;
        promo.Activo = input.Activo;
        promo.Publicado = input.Publicado;
        promo.FecMod = DateTime.UtcNow;
        await _repo.SaveAsync(ct);
        return Result<Promocion>.Success(promo);
    }

    public async Task<Result<bool>> DeleteAsync(int idTenant, long idPromocion, CancellationToken ct = default)
    {
        var promo = await _repo.GetByIdAsync(idPromocion, ct);
        if (promo == null || promo.IdTenant != idTenant)
            return NotFound<bool>("PROMOCION_NOT_FOUND", "Promoción no encontrada");
        await _repo.DeleteAggregateAsync(promo, ct);
        return Result<bool>.Success(true);
    }

    public async Task<Result<PromocionPresentacion>> AddPresentacionAsync(int idTenant, long idPromocion, AddPresentacionInput input, CancellationToken ct = default)
    {
        if (input == null)
            return Bad<PromocionPresentacion>("VALIDATION", "Input requerido");
        var promo = await _repo.GetByIdAsync(idPromocion, ct);
        if (promo == null || promo.IdTenant != idTenant)
            return NotFound<PromocionPresentacion>("PROMOCION_NOT_FOUND", "Promoción no encontrada");
        if (input.IdPresProd <= 0)
            return Bad<PromocionPresentacion>("VALIDATION", "IdPresProd debe ser >0");
        if (input.Rol < 1 || input.Rol > 3)
            return Bad<PromocionPresentacion>("VALIDATION", "Rol debe ser 1=Gatillo, 2=Beneficio o 3=Regalo");
        if (input.Cantidad <= 0)
            return Bad<PromocionPresentacion>("VALIDATION", "Cantidad debe ser >0");

        // Tenant por cadena: Item→PresProd→Prod→Tenant == Promo.IdTenant (§22/P19).
        var tenantPres = await _presProd.GetTenantIdAsync(input.IdPresProd, ct);
        if (tenantPres == null || tenantPres.Value != promo.IdTenant)
            return NotFound<PromocionPresentacion>("PRES_PROD_NOT_FOUND", "Presentación no encontrada");

        // P18: muestra = excepción explícita; fuera de ella la referencia es obligatoria.
        if (input.EsMuestra)
        {
            if (input.IdPrecioReferencia.HasValue && input.IdPrecioReferencia.Value > 0)
                return Unproc<PromocionPresentacion>("PROMOCION_MUESTRA_INVALIDA", "Una muestra no lleva precio de referencia");
        }
        else
        {
            if (!input.IdPrecioReferencia.HasValue || input.IdPrecioReferencia.Value <= 0)
                return Bad<PromocionPresentacion>("VALIDATION", "IdPrecioReferencia es obligatorio salvo muestra");

            var referencia = await _precios.GetByIdAsync(input.IdPrecioReferencia.Value, ct);
            if (referencia == null)
                return NotFound<PromocionPresentacion>("PROMOCION_REFERENCE_NOT_FOUND", "Precio de referencia no encontrado");
            if (!referencia.Activo)
                return Unproc<PromocionPresentacion>("PROMOCION_REFERENCE_INACTIVO", "El precio de referencia no está activo");
            // S57.69 P13-new: dbo.Precio = precio normal único (sin chequeo de tipo).
            // P14: misma presentación (la FK compuesta lo impone en DB; aquí se devuelve 422).
            if (referencia.IdPresProd != input.IdPresProd)
                return Unproc<PromocionPresentacion>("PROMOCION_REFERENCE_WRONG_PRESPROD", "La referencia pertenece a otra presentación");
            // Temporal C: la referencia existe antes o junto al inicio de la promoción.
            if (referencia.FecVig > promo.FechaDesde)
                return Unproc<PromocionPresentacion>("PROMOCION_REFERENCE_POSTERIOR", "La referencia no puede ser posterior al inicio de la promoción");
        }

        var entity = new PromocionPresentacion
        {
            IdPromocion = promo.IdPromocion,
            IdPresProd = input.IdPresProd,
            IdPrecioReferencia = input.EsMuestra ? null : input.IdPrecioReferencia,
            Rol = input.Rol,
            Cantidad = input.Cantidad,
            EsMuestra = input.EsMuestra
        };
        await _presentaciones.AddAsync(entity, ct);
        return Result<PromocionPresentacion>.Success(entity);
    }

    public async Task<Result<PromocionPresentacion>> UpdatePresentacionAsync(int idTenant, long idPart, UpdatePresentacionInput input, CancellationToken ct = default)
    {
        if (input == null)
            return Bad<PromocionPresentacion>("VALIDATION", "Input requerido");
        var part = await _presentaciones.GetByIdAsync(idPart, ct);
        if (part == null)
            return NotFound<PromocionPresentacion>("PROMOCION_PRESENTACION_NOT_FOUND", "Presentación de promoción no encontrada");
        var promo = await _repo.GetByIdAsync(part.IdPromocion, ct);
        if (promo == null || promo.IdTenant != idTenant)
            return NotFound<PromocionPresentacion>("PROMOCION_PRESENTACION_NOT_FOUND", "Presentación de promoción no encontrada");
        if (input.Rol < 1 || input.Rol > 3)
            return Bad<PromocionPresentacion>("VALIDATION", "Rol debe ser 1=Gatillo, 2=Beneficio o 3=Regalo");
        if (input.Cantidad <= 0)
            return Bad<PromocionPresentacion>("VALIDATION", "Cantidad debe ser >0");

        // P16/P30: referencia congelada — IdPrecioReferencia/IdPresProd/EsMuestra no mutan.
        part.Rol = input.Rol;
        part.Cantidad = input.Cantidad;
        await _presentaciones.SaveAsync(ct);
        return Result<PromocionPresentacion>.Success(part);
    }

    public async Task<Result<bool>> DeletePresentacionAsync(int idTenant, long idPart, CancellationToken ct = default)
    {
        var part = await _presentaciones.GetByIdAsync(idPart, ct);
        if (part == null)
            return NotFound<bool>("PROMOCION_PRESENTACION_NOT_FOUND", "Presentación de promoción no encontrada");
        var promo = await _repo.GetByIdAsync(part.IdPromocion, ct);
        if (promo == null || promo.IdTenant != idTenant)
            return NotFound<bool>("PROMOCION_PRESENTACION_NOT_FOUND", "Presentación de promoción no encontrada");
        await _presentaciones.RemoveAsync(part, ct);
        return Result<bool>.Success(true);
    }

    public async Task<Result<PromocionRegla>> AddReglaAsync(int idTenant, long idPromocion, AddReglaInput input, CancellationToken ct = default)
    {
        if (input == null)
            return Bad<PromocionRegla>("VALIDATION", "Input requerido");
        var promo = await _repo.GetByIdAsync(idPromocion, ct);
        if (promo == null || promo.IdTenant != idTenant)
            return NotFound<PromocionRegla>("PROMOCION_NOT_FOUND", "Promoción no encontrada");
        // Codebook numérico de TipoRegla/Operador: propiedad del futuro motor (DESIGN-NOTE doc 06).
        if (input.TipoRegla == 0)
            return Bad<PromocionRegla>("VALIDATION", "TipoRegla debe ser >0");
        if (input.Operador == 0)
            return Bad<PromocionRegla>("VALIDATION", "Operador debe ser >0");
        if (string.IsNullOrWhiteSpace(input.Valor) || input.Valor.Trim().Length > 100)
            return Bad<PromocionRegla>("VALIDATION", "Valor requerido, máximo 100 caracteres");

        var entity = new PromocionRegla
        {
            IdPromocion = promo.IdPromocion,
            TipoRegla = input.TipoRegla,
            Operador = input.Operador,
            Valor = input.Valor.Trim()
        };
        await _reglas.AddAsync(entity, ct);
        return Result<PromocionRegla>.Success(entity);
    }

    public async Task<Result<PromocionBeneficio>> AddBeneficioAsync(int idTenant, long idPromocion, AddBeneficioInput input, CancellationToken ct = default)
    {
        if (input == null)
            return Bad<PromocionBeneficio>("VALIDATION", "Input requerido");
        var promo = await _repo.GetByIdAsync(idPromocion, ct);
        if (promo == null || promo.IdTenant != idTenant)
            return NotFound<PromocionBeneficio>("PROMOCION_NOT_FOUND", "Promoción no encontrada");
        if (input.TipoBeneficio < 1 || input.TipoBeneficio > 4)
            return Bad<PromocionBeneficio>("VALIDATION", "TipoBeneficio debe ser 1=Pct, 2=Monto, 3=PrecioFijo o 4=UnidadGratis");
        if (input.Valor < 0)
            return Unproc<PromocionBeneficio>("PROMOCION_BENEFICIO_NEGATIVO", "El valor del beneficio no puede ser negativo");

        var entity = new PromocionBeneficio
        {
            IdPromocion = promo.IdPromocion,
            TipoBeneficio = input.TipoBeneficio,
            Valor = input.Valor
        };
        await _beneficios.AddAsync(entity, ct);
        return Result<PromocionBeneficio>.Success(entity);
    }

    // Codigo inmutable (identidad estable, UQ por tenant). Resto: columnas aprobadas DDL/03.
    private static Result<Promocion>? ValidateHeader(string codigo, string nombre, string? texto, byte mecanica, byte politica, DateOnly desde, DateOnly? hasta)
    {
        if (string.IsNullOrWhiteSpace(codigo) || codigo.Trim().Length > 30)
            return Bad<Promocion>("VALIDATION", "Codigo requerido, máximo 30 caracteres");
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length > 120)
            return Bad<Promocion>("VALIDATION", "Nombre requerido, máximo 120 caracteres");
        if (texto != null && texto.Trim().Length > 250)
            return Bad<Promocion>("VALIDATION", "TextoComercial máximo 250 caracteres");
        if (mecanica < 1 || mecanica > 7)
            return Bad<Promocion>("VALIDATION", "TipoMecanica debe ser 1..7 (2x1,3x2,SegundoMitad,Regalo,Combo,MontoMin,CantMin)");
        if (politica < 1 || politica > 3)
            return Bad<Promocion>("VALIDATION", "PoliticaPrecio debe ser 1=A-congelado, 2=B-flotante o 3=C-republicar");
        if (hasta.HasValue && hasta.Value < desde)
            return Bad<Promocion>("PROMOCION_DATE_RANGE_INVALID", "FechaHasta no puede ser anterior a FechaDesde");
        return null;
    }
}
