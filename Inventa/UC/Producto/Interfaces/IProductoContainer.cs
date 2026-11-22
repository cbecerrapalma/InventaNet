using CBP.Data.Abstractions;
using CBP.MultiTenant.Abstractions;
using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Inventa.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventa.UC.Producto.Interfaces;

public enum ZonaContenedor
{
    General,
    Variantes,
    Presentacion,
    Detalle
}

public interface IProductoContainer
{
    Prod? ProductoActual { get; }
    int IdTenant { get; }

    [Obsolete("Use IdTenant from ModuleForm")]
    ITenantContext TenantContext { get; }
    IFormFactory FormFactory { get; }
    IUnitOfWorkSync UnitOfWork { get; }

    void MarcarComoModificado();
    void SolicitarRecarga(TipoDatos tipo);
    void NotificarCambioTamaño(ZonaContenedor zona, int alturaActual, int alturaAnterior);

    event EventHandler<ProductoChangedEventArgs>? ProductoChanged;
}

public interface IRefreshable
{
    /// <summary>
    /// Indica si el control tiene cambios pendientes de guardar.
    /// </summary>
    bool HasChanges { get; }

    /// <summary>
    /// Recarga los datos del control desde la base de datos.
    /// </summary>
    void RefreshData();

    /// <summary>
    /// Limpia todos los datos y controles del componente.
    /// </summary>
    void Clear();
}

public interface IEditable
{
    /// <summary>
    /// Indica si el control está en modo edición.
    /// </summary>
    bool IsEditing { get; }

    /// <summary>
    /// Inicia el modo de edición, habilitando controles.
    /// </summary>
    void BeginEdit();

    /// <summary>
    /// Finaliza el modo de edición. Debe validar y guardar si aplica.
    /// </summary>
    void EndEdit();

    /// <summary>
    /// Cancela la edición actual, descartando cambios.
    /// </summary>
    void CancelEdit();
}

public interface IValidatable
{
    /// <summary>
    /// Indica si los datos actuales son válidos.
    /// </summary>
    bool IsValid { get; }

    /// <summary>
    /// Valida los datos del control.
    /// </summary>
    /// <param name="silent">Si es true, no muestra notificaciones de error.</param>
    /// <returns>true si los datos son válidos; false en caso contrario.</returns>
    bool ValidarDatos(bool silent = false);
}



