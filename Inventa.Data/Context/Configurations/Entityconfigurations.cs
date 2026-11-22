using CBP.Outbox;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventa.Data.Context.Configurations;

// ─────────────────────────────────────────────────────────────────────────────
// CONVENCIONES APLICADAS EN TODAS LAS CONFIGURACIONES
//
//  • Todas las propiedades DECIMAL(18,4) del SQL → HasPrecision(18, 4)
//  • Columnas DATE del SQL                       → HasColumnType("date")
//  • Columnas DATETIME2 del SQL                  → HasColumnType("datetime2")
//  • DEFAULT SYSDATETIME()                       → HasDefaultValueSql("SYSDATETIME()")
//  • PK NONCLUSTERED + UQ CLUSTERED (MovInv, Venta, VentaDetalle)
//    → PK marcada con IsClustered(false); índice UQ marcado con IsClustered()
//  • Índices cubrientes (INCLUDE) → .IncludeProperties(...)  [EF Core 5+]
//  • Índices descendentes          → .IsDescending(false, true, ...)  [EF Core 8+]
//  • FK sin nombre explícito en SQL → HasConstraintName omitido (EF genera convención)
//  • Nombres de restricción explícitos en SQL → HasConstraintName reproducido
//  • OnDelete en FK: Restrict/Cascade/SetNull según diseño del modelo
// ─────────────────────────────────────────────────────────────────────────────

// ─── Tenant ───────────────────────────────────────────────────────────────────
internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> b)
    {
        b.ToTable("Tenant");

        b.HasKey(x => x.IdTenant)
            .HasName("PK_Tenant");

        b.Property(x => x.IdTenant).UseIdentityColumn();
        b.Property(x => x.Codigo).HasColumnType("varchar(20)").IsRequired();
        b.Property(x => x.Nombre).HasColumnType("varchar(100)").IsRequired();
        b.Property(x => x.Activo).HasColumnType("bit").HasDefaultValue(true);
        b.Property(x => x.FecCrea)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSDATETIME()");

        b.HasIndex(x => x.Codigo)
            .IsUnique()
            .HasDatabaseName("UQ_Tenant_Codigo");

        // S51.2 mirror fields
        b.Property(x => x.IdTenantPassPlat).HasColumnType("int");
        b.Property(x => x.EsSistema).HasColumnType("bit").HasDefaultValue(false);
        b.Property(x => x.FechaSync).HasColumnType("datetime2");
        b.Property(x => x.FecMod).HasColumnType("datetime2");

        b.HasIndex(x => x.IdTenantPassPlat)
            .IsUnique()
            .HasFilter("[IdTenantPassPlat] IS NOT NULL")
            .HasDatabaseName("UX_Tenant_IdTenantPassPlat");
    }
}

// ─── Unidad ───────────────────────────────────────────────────────────────────
internal sealed class UnidadConfiguration : IEntityTypeConfiguration<Unidad>
{
    public void Configure(EntityTypeBuilder<Unidad> b)
    {
        b.ToTable("Unidad");

        b.HasKey(x => x.IdUnidad)
            .HasName("PK_Unidad");

        b.Property(x => x.IdUnidad).UseIdentityColumn();
        b.Property(x => x.Codigo).HasColumnType("varchar(10)").IsRequired();
        b.Property(x => x.Nombre).HasColumnType("varchar(50)").IsRequired();

        b.HasIndex(x => x.Codigo)
            .IsUnique()
            .HasDatabaseName("UQ_Unidad_Codigo");
    }
}

// ─── Marca ────────────────────────────────────────────────────────────────────
internal sealed class MarcaConfiguration : IEntityTypeConfiguration<Marca>
{
    public void Configure(EntityTypeBuilder<Marca> b)
    {
        b.ToTable("Marca");

        b.HasKey(x => x.IdMarca)
            .HasName("PK_Marca");

        b.Property(x => x.IdMarca).UseIdentityColumn();
        b.Property(x => x.Codigo).HasColumnType("varchar(50)").IsRequired();
        b.Property(x => x.Nombre).HasColumnType("varchar(100)").IsRequired();
        b.Property(x => x.Activo).HasColumnType("bit").HasDefaultValue(true);
        b.Property(x => x.FecCrea)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSDATETIME()");

        // CONSTRAINT UQ_Marca UNIQUE (IdTenant, Codigo)
        b.HasIndex(x => new { x.IdTenant, x.Codigo })
            .IsUnique()
            .HasDatabaseName("UQ_Marca");

        // CREATE INDEX IX_Marca_Tenant ON Marca(IdTenant, Activo)
        b.HasIndex(x => new { x.IdTenant, x.Activo })
            .HasDatabaseName("IX_Marca_Tenant");

        b.HasOne(x => x.Tenant)
            .WithMany(t => t.Marcas)
            .HasForeignKey(x => x.IdTenant)
            .HasConstraintName("FK_Marca_Tenant")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ─── Categoria ────────────────────────────────────────────────────────────────
internal sealed class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> b)
    {
        // S69: CHECK anti-autopadre espejo del SQL (CK_Categoria_NoSelfParent).
        b.ToTable("Categoria", t => t.HasCheckConstraint("CK_Categoria_NoSelfParent", "[IdPadre] IS NULL OR [IdPadre] <> [IdCategoria]"));

        b.HasKey(x => x.IdCategoria)
            .HasName("PK_Categoria");

        b.Property(x => x.IdCategoria).UseIdentityColumn();
        b.Property(x => x.Codigo).HasColumnType("varchar(50)").IsRequired();
        b.Property(x => x.Nombre).HasColumnType("varchar(100)").IsRequired();
        b.Property(x => x.Activo).HasColumnType("bit").HasDefaultValue(true);

        // S69: jerarquía (IdPadre + Orden + ImagenUrl).
        b.Property(x => x.Orden).HasColumnType("smallint").HasDefaultValue((short)0);
        b.Property(x => x.ImagenUrl).HasColumnType("varchar(500)").IsRequired(false);

        // CONSTRAINT UQ_Cat UNIQUE (IdTenant, Codigo)
        b.HasIndex(x => new { x.IdTenant, x.Codigo })
            .IsUnique()
            .HasDatabaseName("UQ_Cat");

        // Análogo a IX_Marca_Tenant, IX_Prov_Tenant, IX_Cliente_Tenant
        b.HasIndex(x => new { x.IdTenant, x.Activo })
            .HasDatabaseName("IX_Cat_Tenant");

        // S69: índice consulta árbol/jerarquía (padre por tenant). Sin INCLUDE
        // en EF (no soportado): el índice SQL IX_Cat_Tenant_Padre lleva
        // INCLUDE (Orden, Nombre); el mapeo solo garantiza el key.
        b.HasIndex(x => new { x.IdTenant, x.IdPadre, x.Activo })
            .HasDatabaseName("IX_Cat_Tenant_Padre");

        b.HasOne(x => x.Tenant)
            .WithMany(t => t.Categorias)
            .HasForeignKey(x => x.IdTenant)
            .HasConstraintName("FK_Cat_Tenant")
            .OnDelete(DeleteBehavior.Restrict);

        // S69: autorreferencia opcional sin cascada (Restrict). El CHECK
        // anti-autopadre vive en SQL (CK_Categoria_NoSelfParent).
        b.HasOne(x => x.Padre)
            .WithMany(x => x.Hijas)
            .HasForeignKey(x => x.IdPadre)
            .HasConstraintName("FK_Cat_Padre")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ─── Prod ─────────────────────────────────────────────────────────────────────
internal sealed class ProdConfiguration : IEntityTypeConfiguration<Prod>
{
    public void Configure(EntityTypeBuilder<Prod> b)
    {
        b.ToTable("Prod");

        b.HasKey(x => x.IdProd)
            .HasName("PK_Prod");

        b.Property(x => x.IdProd).UseIdentityColumn();
        b.Property(x => x.Codigo).HasColumnType("varchar(50)").IsRequired();
        b.Property(x => x.Nombre).HasColumnType("varchar(150)").IsRequired();
        b.Property(x => x.DiasVen).HasColumnType("int");                        // INT NULL
        b.Property(x => x.ManejaLot).HasColumnType("bit").HasDefaultValue(false);
        b.Property(x => x.EsPesable).HasColumnType("bit").HasDefaultValue(false);
        b.Property(x => x.Activo).HasColumnType("bit").HasDefaultValue(true);
        // S57.2 publishability: explicit commercial opt-in, default false (fail-closed for new products).
        b.Property(x => x.VisibleStore).HasColumnType("bit").HasDefaultValue(false);

        // CONSTRAINT UQ_Prod UNIQUE (IdTenant, Codigo)
        b.HasIndex(x => new { x.IdTenant, x.Codigo })
            .IsUnique()
            .HasDatabaseName("UQ_Prod");

        // CREATE INDEX IX_Prod_Tenant_Cat ON Prod(IdTenant, IdCategoria)
        b.HasIndex(x => new { x.IdTenant, x.IdCategoria })
            .HasDatabaseName("IX_Prod_Tenant_Cat");

        // CREATE INDEX IX_Prod_Marca ON Prod(IdMarca)
        b.HasIndex(x => x.IdMarca)
            .HasDatabaseName("IX_Prod_Marca");

        b.HasOne(x => x.Tenant)
            .WithMany(t => t.Productos)
            .HasForeignKey(x => x.IdTenant)
            .HasConstraintName("FK_Prod_Tenant")
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Categoria)
            .WithMany(c => c.Productos)
            .HasForeignKey(x => x.IdCategoria)
            .HasConstraintName("FK_Prod_Cat")
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.UnidadBase)
            .WithMany(u => u.Productos)
            .HasForeignKey(x => x.IdUniBase)
            .HasConstraintName("FK_Prod_Uni")
            .OnDelete(DeleteBehavior.Restrict);

        // IdMarca nullable → SetNull al borrar marca
        b.HasOne(x => x.Marca)
            .WithMany(m => m.Productos)
            .HasForeignKey(x => x.IdMarca)
            .HasConstraintName("FK_Prod_Marca")
            .OnDelete(DeleteBehavior.SetNull);
    }
}

// ─── VarProd ──────────────────────────────────────────────────────────────────
internal sealed class VarProdConfiguration : IEntityTypeConfiguration<VarProd>
{
    public void Configure(EntityTypeBuilder<VarProd> b)
    {
        b.ToTable("VarProd");

        b.HasKey(x => x.IdVarProd)
            .HasName("PK_VarProd");

        b.Property(x => x.IdVarProd).UseIdentityColumn();
        b.Property(x => x.Codigo).HasColumnType("varchar(50)").IsRequired();
        b.Property(x => x.Nombre).HasColumnType("varchar(100)").IsRequired();

        // DECIMAL(18,4) — costo promedio ponderado actualizado por SPs
        b.Property(x => x.CostoProm)
            .HasPrecision(18, 4)
            .HasDefaultValue(0m);

        b.Property(x => x.Activo).HasColumnType("bit").HasDefaultValue(true);

        // CONSTRAINT UQ_Var UNIQUE (IdProd, Codigo)
        b.HasIndex(x => new { x.IdProd, x.Codigo })
            .IsUnique()
            .HasDatabaseName("UQ_Var");

        // CREATE INDEX IX_VarProd_Prod ON VarProd(IdProd)
        b.HasIndex(x => x.IdProd)
            .HasDatabaseName("IX_VarProd_Prod");

        b.HasOne(x => x.Prod)
            .WithMany(p => p.Variantes)
            .HasForeignKey(x => x.IdProd)
            .HasConstraintName("FK_Var_Prod")
            .OnDelete(DeleteBehavior.Cascade);  // Al borrar Prod, sus variantes desaparecen
    }
}

// ─── AtriDef ──────────────────────────────────────────────────────────────────
internal sealed class AtriDefConfiguration : IEntityTypeConfiguration<AtriDef>
{
    public void Configure(EntityTypeBuilder<AtriDef> b)
    {
        b.ToTable("AtriDef");

        b.HasKey(x => x.IdAtriDef)
            .HasName("PK_AtriDef");

        b.Property(x => x.IdAtriDef).UseIdentityColumn();
        b.Property(x => x.Codigo).HasColumnType("varchar(50)").IsRequired();
        b.Property(x => x.Nombre).HasColumnType("varchar(100)").IsRequired();
        b.Property(x => x.EsOblig).HasColumnType("bit").HasDefaultValue(false);
        b.Property(x => x.Activo).HasColumnType("bit").HasDefaultValue(true);

        // CONSTRAINT UQ_ADef UNIQUE (IdCategoria, Codigo)
        b.HasIndex(x => new { x.IdCategoria, x.Codigo })
            .IsUnique()
            .HasDatabaseName("UQ_ADef");

        b.HasOne(x => x.Categoria)
            .WithMany(c => c.AtributosDef)
            .HasForeignKey(x => x.IdCategoria)
            .HasConstraintName("FK_ADef_Cat")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ─── AtriVal ──────────────────────────────────────────────────────────────────
internal sealed class AtriValConfiguration : IEntityTypeConfiguration<AtriVal>
{
    public void Configure(EntityTypeBuilder<AtriVal> b)
    {
        b.ToTable("AtriVal");

        b.HasKey(x => x.IdAtriVal)
            .HasName("PK_AtriVal");

        b.Property(x => x.IdAtriVal).UseIdentityColumn();
        b.Property(x => x.Codigo).HasColumnType("varchar(50)").IsRequired();
        b.Property(x => x.Nombre).HasColumnType("varchar(100)").IsRequired();
        b.Property(x => x.Orden).HasColumnType("int").HasDefaultValue(0);
        b.Property(x => x.Activo).HasColumnType("bit").HasDefaultValue(true);

        // CONSTRAINT UQ_AVal UNIQUE (IdAtriDef, Codigo)
        b.HasIndex(x => new { x.IdAtriDef, x.Codigo })
            .IsUnique()
            .HasDatabaseName("UQ_AVal");

        b.HasOne(x => x.AtriDef)
            .WithMany(d => d.Valores)
            .HasForeignKey(x => x.IdAtriDef)
            .HasConstraintName("FK_AVal_Def")
            .OnDelete(DeleteBehavior.Cascade);  // Al borrar definición, sus valores desaparecen
    }
}

// ─── VarAtri (pivot variante–atributo) ───────────────────────────────────────
internal sealed class VarAtriConfiguration : IEntityTypeConfiguration<VarAtri>
{
    public void Configure(EntityTypeBuilder<VarAtri> b)
    {
        b.ToTable("VarAtri");

        b.HasKey(x => x.IdVarAtri)
            .HasName("PK_VarAtri");

        b.Property(x => x.IdVarAtri).UseIdentityColumn();

        // CONSTRAINT UQ_VAtri UNIQUE (IdVarProd, IdAtriVal)
        b.HasIndex(x => new { x.IdVarProd, x.IdAtriVal })
            .IsUnique()
            .HasDatabaseName("UQ_VAtri");

        b.HasOne(x => x.VarProd)
            .WithMany(v => v.Atributos)
            .HasForeignKey(x => x.IdVarProd)
            .HasConstraintName("FK_VAtri_Var")
            .OnDelete(DeleteBehavior.Cascade);  // Al borrar variante, sus atributos desaparecen

        b.HasOne(x => x.AtriVal)
            .WithMany(v => v.VarAtributos)
            .HasForeignKey(x => x.IdAtriVal)
            .HasConstraintName("FK_VAtri_Val")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ─── ProdConv ─────────────────────────────────────────────────────────────────
internal sealed class ProdConvConfiguration : IEntityTypeConfiguration<ProdConv>
{
    public void Configure(EntityTypeBuilder<ProdConv> b)
    {
        b.ToTable("ProdConv");

        b.HasKey(x => x.IdProdConv)
            .HasName("PK_ProdConv");

        b.Property(x => x.IdProdConv).UseIdentityColumn();

        // DECIMAL(18,8) — alta precisión para factores de conversión fraccionarios
        b.Property(x => x.Factor).HasPrecision(18, 8);

        // CONSTRAINT UQ_PConv_Var UNIQUE (IdVarProd, IdUnidad)
        b.HasIndex(x => new { x.IdVarProd, x.IdUnidad })
            .IsUnique()
            .HasDatabaseName("UQ_PConv_Var");

        b.HasOne(x => x.VarProd)
            .WithMany(v => v.Conversiones)
            .HasForeignKey(x => x.IdVarProd)
            .HasConstraintName("FK_PConv_Var")
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Unidad)
            .WithMany(u => u.Conversiones)
            .HasForeignKey(x => x.IdUnidad)
            .HasConstraintName("FK_PConv_Uni")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ─── PresProd ─────────────────────────────────────────────────────────────────
internal sealed class PresProdConfiguration : IEntityTypeConfiguration<PresProd>
{
    public void Configure(EntityTypeBuilder<PresProd> b)
    {
        b.ToTable("PresProd");

        b.HasKey(x => x.IdPresProd)
            .HasName("PK_PresProd");

        b.Property(x => x.IdPresProd).UseIdentityColumn();
        b.Property(x => x.Descrip).HasColumnType("varchar(100)").IsRequired();
        b.Property(x => x.EsVenta).HasColumnType("bit").HasDefaultValue(true);
        b.Property(x => x.EsCompra).HasColumnType("bit").HasDefaultValue(true);
        b.Property(x => x.Activo).HasColumnType("bit").HasDefaultValue(true);

        // CREATE INDEX IX_PresProd_Var ON PresProd(IdVarProd)
        b.HasIndex(x => x.IdVarProd)
            .HasDatabaseName("IX_PresProd_Var");

        b.HasOne(x => x.VarProd)
            .WithMany(v => v.Presentaciones)
            .HasForeignKey(x => x.IdVarProd)
            .HasConstraintName("FK_Pres_Var")
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Unidad)
            .WithMany(u => u.Presentaciones)
            .HasForeignKey(x => x.IdUnidad)
            .HasConstraintName("FK_Pres_Uni")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ─── CodBar ───────────────────────────────────────────────────────────────────
internal sealed class CodBarConfiguration : IEntityTypeConfiguration<CodBar>
{
    public void Configure(EntityTypeBuilder<CodBar> b)
    {
        b.ToTable("CodBar");

        b.HasKey(x => x.IdCodBar)
            .HasName("PK_CodBar");

        b.Property(x => x.IdCodBar).UseIdentityColumn();
        b.Property(x => x.Codigo).HasColumnType("varchar(50)").IsRequired();

        // TINYINT — almacenado como byte en C#
        b.Property(x => x.TipoCod)
            .HasColumnType("tinyint")
            .HasDefaultValue((byte)1);

        b.Property(x => x.Principal).HasColumnType("bit").HasDefaultValue(false);

        // CONSTRAINT UQ_CBar UNIQUE (Codigo) — único global (códigos de barra son únicos mundiales)
        b.HasIndex(x => x.Codigo)
            .IsUnique()
            .HasDatabaseName("UQ_CBar");

        // CREATE INDEX IX_CodBar_Fast ON CodBar(Codigo, IdPresProd)
        // Cubriente para lookup POS: busca por código y accede IdPresProd sin heap
        b.HasIndex(x => new { x.Codigo, x.IdPresProd })
            .HasDatabaseName("IX_CodBar_Fast");

        b.HasOne(x => x.PresProd)
            .WithMany(p => p.CodigosBarra)
            .HasForeignKey(x => x.IdPresProd)
            .HasConstraintName("FK_CBar_Pres")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// ─── Bodega ───────────────────────────────────────────────────────────────────
internal sealed class BodegaConfiguration : IEntityTypeConfiguration<Bodega>
{
    public void Configure(EntityTypeBuilder<Bodega> b)
    {
        b.ToTable("Bodega");

        b.HasKey(x => x.IdBodega)
            .HasName("PK_Bodega");

        b.Property(x => x.IdBodega).UseIdentityColumn();
        b.Property(x => x.Codigo).HasColumnType("varchar(20)").IsRequired();
        b.Property(x => x.Nombre).HasColumnType("varchar(100)").IsRequired();
        b.Property(x => x.Activo).HasColumnType("bit").HasDefaultValue(true);

        // CONSTRAINT UQ_Bod UNIQUE (IdTenant, Codigo)
        b.HasIndex(x => new { x.IdTenant, x.Codigo })
            .IsUnique()
            .HasDatabaseName("UQ_Bod");

        b.HasOne(x => x.Tenant)
            .WithMany(t => t.Bodegas)
            .HasForeignKey(x => x.IdTenant)
            .HasConstraintName("FK_Bod_Tenant")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ─── Lote ─────────────────────────────────────────────────────────────────────
internal sealed class LoteConfiguration : IEntityTypeConfiguration<Lote>
{
    public void Configure(EntityTypeBuilder<Lote> b)
    {
        b.ToTable("Lote");

        b.HasKey(x => x.IdLote)
            .HasName("PK_Lote");

        b.Property(x => x.IdLote).UseIdentityColumn();
        b.Property(x => x.Codigo).HasColumnType("varchar(50)").IsRequired();

        // DATE NULL → DateOnly? en C#
        b.Property(x => x.FecVen).HasColumnType("date");

        b.Property(x => x.Activo).HasColumnType("bit").HasDefaultValue(true);

        // CONSTRAINT UQ_Lote UNIQUE (IdTenant, IdVarProd, Codigo)
        b.HasIndex(x => new { x.IdTenant, x.IdVarProd, x.Codigo })
            .IsUnique()
            .HasDatabaseName("UQ_Lote");

        // CREATE INDEX IX_Lote_Ven ON Lote(IdVarProd, FecVen)
        // Soporte para alertas de vencimiento y FIFO/FEFO
        b.HasIndex(x => new { x.IdVarProd, x.FecVen })
            .HasDatabaseName("IX_Lote_Ven");

        // Índice de operaciones: filtrar lotes activos por tenant
        b.HasIndex(x => new { x.IdTenant, x.Activo })
            .HasDatabaseName("IX_Lote_Tenant");

        b.HasOne(x => x.Tenant)
            .WithMany(t => t.Lotes)
            .HasForeignKey(x => x.IdTenant)
            .HasConstraintName("FK_Lote_Tenant")
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.VarProd)
            .WithMany(v => v.Lotes)
            .HasForeignKey(x => x.IdVarProd)
            .HasConstraintName("FK_Lote_Var")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ─── MovInv ───────────────────────────────────────────────────────────────────
// DISEÑO CRÍTICO: el SQL define PK NONCLUSTERED + UNIQUE CLUSTERED separados.
// En EF Core, la PK es clustered por defecto. Para replicar la estructura:
//   1. PK se configura como NONCLUSTERED con IsClustered(false)
//   2. El índice UQ_MovInv_Cluster se configura con IsClustered()
// Esto requiere SQL Server y EF Core con proveedor SqlServer.
internal sealed class MovInvConfiguration : IEntityTypeConfiguration<MovInv>
{
    public void Configure(EntityTypeBuilder<MovInv> b)
    {
        b.ToTable("MovInv");

        // CONSTRAINT PK_MovInv PRIMARY KEY NONCLUSTERED (IdMovInv)
        b.HasKey(x => x.IdMovInv)
            .HasName("PK_MovInv")
            .IsClustered(false);                // ← PK NONCLUSTERED

        b.Property(x => x.IdMovInv).UseIdentityColumn();
        b.Property(x => x.FecMov)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSDATETIME()");

        // TINYINT NOT NULL
        b.Property(x => x.TipoMov).HasColumnType("tinyint");

        // DECIMAL(18,4) NOT NULL
        b.Property(x => x.CantCom).HasPrecision(18, 4);
        b.Property(x => x.CantBase).HasPrecision(18, 4);
        b.Property(x => x.CostoUni).HasPrecision(18, 4);

        // CONSTRAINT UQ_MovInv_Cluster UNIQUE CLUSTERED (IdTenant, FecMov, IdMovInv)
        // Cluster por tenant+fecha: acceso secuencial por fecha dentro de cada tenant
        b.HasIndex(x => new { x.IdTenant, x.FecMov, x.IdMovInv })
            .IsUnique()
            .IsClustered()                      // ← UNIQUE CLUSTERED
            .HasDatabaseName("UQ_MovInv_Cluster");

        // CREATE INDEX IX_MovInv_Prod ON MovInv(IdTenant, IdPresProd, FecMov)
        // Soporte para kardex de producto y búsquedas por presentación
        b.HasIndex(x => new { x.IdTenant, x.IdPresProd, x.FecMov })
            .HasDatabaseName("IX_MovInv_Prod");

        // FK a Bodega y PresProd — sin cascade (MovInv es registro inmutable)
        b.HasOne(x => x.Bodega)
            .WithMany(bd => bd.Movimientos)
            .HasForeignKey(x => x.IdBodega)
            .HasConstraintName("FK_MovInv_Bod")
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.PresProd)
            .WithMany(p => p.Movimientos)
            .HasForeignKey(x => x.IdPresProd)
            .HasConstraintName("FK_MovInv_Pres")
            .OnDelete(DeleteBehavior.Restrict);

        // IdLote nullable → SetNull al borrar lote (el movimiento conserva su registro)
        b.HasOne(x => x.Lote)
            .WithMany(l => l.Movimientos)
            .HasForeignKey(x => x.IdLote)
            .HasConstraintName("FK_MovInv_Lote")
            .OnDelete(DeleteBehavior.SetNull);

        // S51.2 auditoría — dbo.MovInv NO tiene UsuarioCrea en BBDD congelada:
        // se ignora en el modelo (sin DDL). Si el owner aprueba migración, revertir.
        b.Ignore(x => x.UsuarioCrea);
        b.Ignore(x => x.UsuarioCreaNav);
    }
}

// ─── Stock ────────────────────────────────────────────────────────────────────
// DISEÑO CRÍTICO: el UQ incluye IdLote que es NULLABLE.
// SQL Server trata cada NULL como distinto en índices únicos por defecto
// → dos filas con IdLote = NULL serían DISTINTAS, pero en este modelo
//   IdLote = NULL significa "sin lote" y debe existir solo UNA fila por
//   combinación (IdTenant, IdBodega, IdVarProd) con IdLote = NULL.
// EF Core 9 expone AreNullsDistinct(false) para índices únicos con columnas nullable.
// Con versiones anteriores se recomienda un índice filtrado adicional o
// dejar la restricción sólo en los SPs (que ya lo hacen con MERGE).
internal sealed class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> b)
    {
        b.ToTable("Stock");

        b.HasKey(x => x.IdStock)
            .HasName("PK_Stock");

        b.Property(x => x.IdStock).UseIdentityColumn();

        // DECIMAL(18,4) NOT NULL
        b.Property(x => x.CantAct).HasPrecision(18, 4);
        b.Property(x => x.CostoProm).HasPrecision(18, 4);

        b.Property(x => x.FecAct).HasColumnType("datetime2");

        // CONSTRAINT UQ_Stock UNIQUE (IdTenant, IdBodega, IdVarProd, IdLote)
        // Nota: IdLote es nullable. AreNullsDistinct(false) requiere EF Core 9+.
        // Con EF Core 8 y anteriores este UQ se aplica sin esa opción;
        // la integridad de la fila NULL queda garantizada por los SPs (MERGE).
        b.HasIndex(x => new { x.IdTenant, x.IdBodega, x.IdVarProd, x.IdLote })
            .IsUnique()
            .HasDatabaseName("UQ_Stock");

        // CREATE INDEX IX_Stock_Busq ON Stock(IdTenant, IdVarProd, IdBodega)
        b.HasIndex(x => new { x.IdTenant, x.IdVarProd, x.IdBodega })
            .HasDatabaseName("IX_Stock_Busq");

        b.HasOne(x => x.Bodega)
            .WithMany(bd => bd.Stocks)
            .HasForeignKey(x => x.IdBodega)
            .HasConstraintName("FK_Stock_Bod")
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.VarProd)
            .WithMany(v => v.Stocks)
            .HasForeignKey(x => x.IdVarProd)
            .HasConstraintName("FK_Stock_Var")
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Lote)
            .WithMany(l => l.Stocks)
            .HasForeignKey(x => x.IdLote)
            .HasConstraintName("FK_Stock_Lote")
            .OnDelete(DeleteBehavior.SetNull);
    }
}

// ─── StockDisp ────────────────────────────────────────────────────────────────
// Vista materializada de stock por presentación/bodega para consultas POS.
// El UQ_StockDisp ya actúa como índice — no se duplica con otro HasIndex igual.
internal sealed class StockDispConfiguration : IEntityTypeConfiguration<StockDisp>
{
    public void Configure(EntityTypeBuilder<StockDisp> b)
    {
        b.ToTable("StockDisp");

        b.HasKey(x => x.IdStockDisp)
            .HasName("PK_StockDisp");

        b.Property(x => x.IdStockDisp).UseIdentityColumn();

        // DECIMAL(18,4) NOT NULL
        b.Property(x => x.CantDisp).HasPrecision(18, 4);

        // CONSTRAINT UQ_StockDisp UNIQUE (IdTenant, IdPresProd, IdBodega)
        // + CREATE INDEX IX_StockDisp_Busq ON StockDisp(IdTenant, IdPresProd, IdBodega)
        // → Son los mismos campos: el UQ ya crea el índice, no se duplica.
        b.HasIndex(x => new { x.IdTenant, x.IdPresProd, x.IdBodega })
            .IsUnique()
            .HasDatabaseName("UQ_StockDisp");

        b.HasOne(x => x.PresProd)
            .WithMany(p => p.StocksDisp)
            .HasForeignKey(x => x.IdPresProd)
            .HasConstraintName("FK_StockDisp_Pres")
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Bodega)
            .WithMany(bd => bd.StocksDisp)
            .HasForeignKey(x => x.IdBodega)
            .HasConstraintName("FK_StockDisp_Bod")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ─── Precio ───────────────────────────────────────────────────────────────────
internal sealed class PrecioConfiguration : IEntityTypeConfiguration<Precio>
{
    public void Configure(EntityTypeBuilder<Precio> b)
    {
        b.ToTable("Precio");

        b.HasKey(x => x.IdPrecio)
            .HasName("PK_Precio");

        b.Property(x => x.IdPrecio).UseIdentityColumn();

        // S57.69: precio normal único (columna TipoPrec eliminada en fase 4).

        // DECIMAL(18,4) NOT NULL
        b.Property(x => x.Valor).HasPrecision(18, 4);

        // DATE NOT NULL → DateOnly en C#
        b.Property(x => x.FecVig).HasColumnType("date");

        b.Property(x => x.Activo).HasColumnType("bit").HasDefaultValue(true);

        // CREATE INDEX IX_Precio_Fast ON Precio(IdPresProd, FecVig DESC)
        // S57.69: índice cubriente para "precio normal vigente más reciente" —
        // consulta crítica POS. Mismo nombre, nueva forma (sin TipoPrec).
        // FecVig DESC: EF Core 8+ permite especificar orden de columnas individualmente.
        b.HasIndex(x => new { x.IdPresProd, x.FecVig })
            .IsDescending(false, true)   // solo FecVig es DESC
            .HasDatabaseName("IX_Precio_Fast");

        b.HasOne(x => x.PresProd)
            .WithMany(p => p.Precios)
            .HasForeignKey(x => x.IdPresProd)
            .HasConstraintName("FK_Precio_Pres")
            .OnDelete(DeleteBehavior.Cascade);

        // S57.52: base de la FK compuesta Oferta(IdPrecioReferencia,IdPresProd).
        // S57.66: alternate key real — también base de
        // FK_PromocionPres_MismaPres. Índice real creado por DDL (UX_Precio_IdPres).
        b.HasAlternateKey(x => new { x.IdPrecio, x.IdPresProd })
            .HasName("UX_Precio_IdPres");
    }
}

// ─── PrecioMayorista (S57.69) ───────────────────────────────────────────
// Relación dedicada mayorista. DDL real: D:\CODIGOS\BBDD\Inventa\historico\S57\s5769-wholesale-price-ddl.sql (copia verificada por el arquitecto; ver S77-RESULT).
internal sealed class PrecioMayoristaConfiguration : IEntityTypeConfiguration<PrecioMayorista>
{
    public void Configure(EntityTypeBuilder<PrecioMayorista> b)
    {
        b.ToTable("PrecioMayorista");

        b.HasKey(x => x.IdPrecioMayorista)
            .HasName("PK_PrecioMayorista");

        b.Property(x => x.IdPrecioMayorista).UseIdentityColumn();

        // DECIMAL(18,4) NOT NULL
        b.Property(x => x.Valor).HasPrecision(18, 4);

        // DATE NOT NULL → DateOnly en C#
        b.Property(x => x.FecVig).HasColumnType("date");

        b.Property(x => x.Activo).HasColumnType("bit").HasDefaultValue(true);

        // CREATE INDEX IX_PrecioMayorista_Fast ON PrecioMayorista(IdPresProd, FecVig DESC)
        b.HasIndex(x => new { x.IdPresProd, x.FecVig })
            .IsDescending(false, true)
            .HasDatabaseName("IX_PrecioMayorista_Fast");

        b.HasOne(x => x.PresProd)
            .WithMany(p => p.PreciosMayorista)
            .HasForeignKey(x => x.IdPresProd)
            .HasConstraintName("FK_PrecioMayorista_Pres")
            .OnDelete(DeleteBehavior.Restrict);

        // Simetría con UX_Precio_IdPres para futuras refs compuestas.
        b.HasAlternateKey(x => new { x.IdPrecioMayorista, x.IdPresProd })
            .HasName("UX_PrecioMayorista_IdPres");
    }
}

// ─── Oferta (S57.52) ────────────────────────────────────────────────────────
internal sealed class OfertaConfiguration : IEntityTypeConfiguration<Oferta>
{
    public void Configure(EntityTypeBuilder<Oferta> b)
    {
        b.ToTable("Oferta");

        b.HasKey(x => x.IdOferta)
            .HasName("PK_Oferta");

        b.Property(x => x.IdOferta).UseIdentityColumn();

        // DECIMAL(18,4) NOT NULL — fuente de verdad
        b.Property(x => x.PrecioOferta).HasPrecision(18, 4);

        // DECIMAL(5,2) NULL — informativo
        b.Property(x => x.PctReferencial).HasPrecision(5, 2);

        // DATE NOT NULL / DATE NULL → DateOnly / DateOnly?
        b.Property(x => x.FechaDesde).HasColumnType("date");
        b.Property(x => x.FechaHasta).HasColumnType("date");

        b.Property(x => x.Prioridad).HasColumnType("smallint").HasDefaultValue((short)100);
        b.Property(x => x.Activo).HasColumnType("bit").HasDefaultValue(true);
        b.Property(x => x.Publicado).HasColumnType("bit").HasDefaultValue(false);
        b.Property(x => x.OrigenMigracion).HasColumnType("tinyint").HasDefaultValue((byte)0);

        b.Property(x => x.FecCrea)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSDATETIME()");
        b.Property(x => x.FecMod).HasColumnType("datetime2");

        // CREATE INDEX IX_Oferta_Lookup ON Oferta(IdPresProd, FechaDesde DESC)
        //   INCLUDE (PrecioOferta, FechaHasta, Activo, Publicado, IdPrecioReferencia)
        b.HasIndex(x => new { x.IdPresProd, x.FechaDesde })
            .IsDescending(false, true)
            .IncludeProperties(x => new { x.PrecioOferta, x.FechaHasta, x.Activo, x.Publicado, x.IdPrecioReferencia })
            .HasDatabaseName("IX_Oferta_Lookup");

        // CREATE INDEX IX_Oferta_Reporte ON Oferta(FechaHasta, Activo)
        //   INCLUDE (IdPresProd, PrecioOferta, IdPrecioReferencia)
        b.HasIndex(x => new { x.FechaHasta, x.Activo })
            .IncludeProperties(x => new { x.IdPresProd, x.PrecioOferta, x.IdPrecioReferencia })
            .HasDatabaseName("IX_Oferta_Reporte");

        b.HasOne(x => x.PresProd)
            .WithMany(p => p.Ofertas)
            .HasForeignKey(x => x.IdPresProd)
            .HasConstraintName("FK_Oferta_PresProd")
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.PrecioReferencia)
            .WithMany(p => p.OfertasReferenciadas)
            .HasForeignKey(x => x.IdPrecioReferencia)
            .HasConstraintName("FK_Oferta_PrecioRef")
            .OnDelete(DeleteBehavior.Restrict);

        // FK_Oferta_MismaPres (IdPrecioReferencia,IdPresProd)->Precio(IdPrecio,IdPresProd):
        // integridad DB real (DDL §4); el servicio valida misma-presentación antes
        // del INSERT para devolver 422 en vez de SqlException.
    }
}

// ─── Proveedor ────────────────────────────────────────────────────────────────
internal sealed class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> b)
    {
        b.ToTable("Proveedor");

        b.HasKey(x => x.IdProveedor)
            .HasName("PK_Proveedor");

        b.Property(x => x.IdProveedor).UseIdentityColumn();
        b.Property(x => x.Codigo).HasColumnType("varchar(50)").IsRequired();
        b.Property(x => x.RazonSocial).HasColumnType("varchar(200)").IsRequired();
        b.Property(x => x.NombreComercial).HasColumnType("varchar(200)");
        b.Property(x => x.RUT).HasColumnType("varchar(20)");
        b.Property(x => x.Direccion).HasColumnType("varchar(200)");
        b.Property(x => x.Telefono).HasColumnType("varchar(50)");
        b.Property(x => x.Email).HasColumnType("varchar(100)");
        b.Property(x => x.Contacto).HasColumnType("varchar(100)");
        b.Property(x => x.Activo).HasColumnType("bit").HasDefaultValue(true);
        b.Property(x => x.FecCrea)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSDATETIME()");

        // CONSTRAINT UQ_Prov UNIQUE (IdTenant, Codigo)
        b.HasIndex(x => new { x.IdTenant, x.Codigo })
            .IsUnique()
            .HasDatabaseName("UQ_Prov");

        // CREATE INDEX IX_Prov_Tenant ON Proveedor(IdTenant, Activo)
        b.HasIndex(x => new { x.IdTenant, x.Activo })
            .HasDatabaseName("IX_Prov_Tenant");

        b.HasOne(x => x.Tenant)
            .WithMany(t => t.Proveedores)
            .HasForeignKey(x => x.IdTenant)
            .HasConstraintName("FK_Prov_Tenant")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ─── OrdenCompra ──────────────────────────────────────────────────────────────
internal sealed class OrdenCompraConfiguration : IEntityTypeConfiguration<OrdenCompra>
{
    public void Configure(EntityTypeBuilder<OrdenCompra> b)
    {
        b.ToTable("OrdenCompra");

        b.HasKey(x => x.IdOrdenCompra)
            .HasName("PK_OrdenCompra");

        b.Property(x => x.IdOrdenCompra).UseIdentityColumn();
        b.Property(x => x.NumOrden).HasColumnType("varchar(50)").IsRequired();

        // DATE NOT NULL / DATE NULL → DateOnly / DateOnly?
        b.Property(x => x.FecOrden).HasColumnType("date");
        b.Property(x => x.FecEntrega).HasColumnType("date");

        // TINYINT NOT NULL — 1=Pendiente, 2=Aprobada, 3=Recibida, 4=Cancelada
        b.Property(x => x.Estado)
            .HasColumnType("tinyint")
            .HasDefaultValue((byte)1);

        // DECIMAL(18,4) NOT NULL DEFAULT 0
        b.Property(x => x.Total)
            .HasPrecision(18, 4)
            .HasDefaultValue(0m);

        b.Property(x => x.Observaciones).HasColumnType("varchar(500)");
        b.Property(x => x.FecCrea)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSDATETIME()");

        // CONSTRAINT UQ_OC UNIQUE (IdTenant, NumOrden)
        b.HasIndex(x => new { x.IdTenant, x.NumOrden })
            .IsUnique()
            .HasDatabaseName("UQ_OC");

        // CREATE INDEX IX_OC_Tenant_Fecha ON OrdenCompra(IdTenant, FecOrden DESC)
        b.HasIndex(x => new { x.IdTenant, x.FecOrden })
            .IsDescending(false, true)          // FecOrden DESC
            .HasDatabaseName("IX_OC_Tenant_Fecha");

        // CREATE INDEX IX_OC_Prov ON OrdenCompra(IdProveedor, Estado)
        b.HasIndex(x => new { x.IdProveedor, x.Estado })
            .HasDatabaseName("IX_OC_Prov");

        b.HasOne(x => x.Tenant)
            .WithMany(t => t.OrdenesCompra)
            .HasForeignKey(x => x.IdTenant)
            .HasConstraintName("FK_OC_Tenant")
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Proveedor)
            .WithMany(p => p.OrdenesCompra)
            .HasForeignKey(x => x.IdProveedor)
            .HasConstraintName("FK_OC_Prov")
            .OnDelete(DeleteBehavior.Restrict);

        // S51.2 auditoría — dbo.OrdenCompra NO tiene UsuarioCrea/UsuarioMod en
        // BBDD congelada: se ignoran en el modelo (sin DDL). Si el owner aprueba
        // migración, revertir.
        b.Ignore(x => x.UsuarioCrea);
        b.Ignore(x => x.UsuarioMod);
        b.Ignore(x => x.UsuarioCreaNav);
        b.Ignore(x => x.UsuarioModNav);
    }
}

// ─── OrdenCompraDetalle ───────────────────────────────────────────────────────
internal sealed class OrdenCompraDetalleConfiguration : IEntityTypeConfiguration<OrdenCompraDetalle>
{
    public void Configure(EntityTypeBuilder<OrdenCompraDetalle> b)
    {
        b.ToTable("OrdenCompraDetalle");

        b.HasKey(x => x.IdOCDetalle)
            .HasName("PK_OrdenCompraDetalle");

        b.Property(x => x.IdOCDetalle).UseIdentityColumn();

        // DECIMAL(18,4) NOT NULL para todas las columnas numéricas
        b.Property(x => x.Cantidad).HasPrecision(18, 4);
        b.Property(x => x.PrecioUnitario).HasPrecision(18, 4);
        b.Property(x => x.Subtotal).HasPrecision(18, 4);

        // CREATE INDEX IX_OCDet_OC ON OrdenCompraDetalle(IdOrdenCompra)
        b.HasIndex(x => x.IdOrdenCompra)
            .HasDatabaseName("IX_OCDet_OC");

        b.HasOne(x => x.OrdenCompra)
            .WithMany(o => o.Detalles)
            .HasForeignKey(x => x.IdOrdenCompra)
            .HasConstraintName("FK_OCDet_OC")
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.PresProd)
            .WithMany(p => p.DetallesOC)
            .HasForeignKey(x => x.IdPresProd)
            .HasConstraintName("FK_OCDet_Pres")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ─── Cliente ──────────────────────────────────────────────────────────────────
internal sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> b)
    {
        b.ToTable("Cliente");

        b.HasKey(x => x.IdCliente)
            .HasName("PK_Cliente");

        b.Property(x => x.IdCliente).UseIdentityColumn();
        b.Property(x => x.Codigo).HasColumnType("varchar(50)").IsRequired();
        b.Property(x => x.Nombre).HasColumnType("varchar(200)").IsRequired();
        b.Property(x => x.RUT).HasColumnType("varchar(20)");
        b.Property(x => x.Direccion).HasColumnType("varchar(200)");
        b.Property(x => x.Telefono).HasColumnType("varchar(50)");
        b.Property(x => x.Email).HasColumnType("varchar(100)");

        // TINYINT NOT NULL DEFAULT 1 — 1=Público General, 2=Frecuente, 3=Mayorista, 4=Corporativo
        b.Property(x => x.TipoCliente)
            .HasColumnType("tinyint")
            .HasDefaultValue((byte)1);

        b.Property(x => x.Activo).HasColumnType("bit").HasDefaultValue(true);
        b.Property(x => x.FecCrea)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSDATETIME()");

        // CONSTRAINT UQ_Cliente UNIQUE (IdTenant, Codigo)
        b.HasIndex(x => new { x.IdTenant, x.Codigo })
            .IsUnique()
            .HasDatabaseName("UQ_Cliente");

        // CREATE INDEX IX_Cliente_Tenant ON Cliente(IdTenant, Activo)
        b.HasIndex(x => new { x.IdTenant, x.Activo })
            .HasDatabaseName("IX_Cliente_Tenant");

        b.HasOne(x => x.Tenant)
            .WithMany(t => t.Clientes)
            .HasForeignKey(x => x.IdTenant)
            .HasConstraintName("FK_Cliente_Tenant")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ─── Venta ────────────────────────────────────────────────────────────────────
// DISEÑO CRÍTICO: igual que MovInv, PK NONCLUSTERED + UQ CLUSTERED.
internal sealed class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> b)
    {
        b.ToTable("Venta");

        // CONSTRAINT PK_Venta PRIMARY KEY NONCLUSTERED (IdVenta)
        b.HasKey(x => x.IdVenta)
            .HasName("PK_Venta")
            .IsClustered(false);                // ← PK NONCLUSTERED

        b.Property(x => x.IdVenta).UseIdentityColumn();
        b.Property(x => x.NumVenta).HasColumnType("varchar(50)").IsRequired();
        b.Property(x => x.FecVenta)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSDATETIME()");

        // TINYINT NOT NULL DEFAULT 1 — 1=Boleta, 2=Factura, 3=Nota de Venta
        b.Property(x => x.TipoDoc)
            .HasColumnType("tinyint")
            .HasDefaultValue((byte)1);

        b.Property(x => x.IdUsuario).HasColumnType("int");

        // DECIMAL(18,4) NOT NULL
        b.Property(x => x.SubTotal).HasPrecision(18, 4);
        b.Property(x => x.Descuento).HasPrecision(18, 4).HasDefaultValue(0m);
        b.Property(x => x.Impuesto).HasPrecision(18, 4).HasDefaultValue(0m);
        b.Property(x => x.Total).HasPrecision(18, 4);

        // TINYINT NOT NULL DEFAULT 1 — 1=Completada, 2=Anulada
        b.Property(x => x.Estado)
            .HasColumnType("tinyint")
            .HasDefaultValue((byte)1);

        b.Property(x => x.FecAnulacion).HasColumnType("datetime2");
        b.Property(x => x.MotivoAnulacion).HasColumnType("varchar(200)");

        // CONSTRAINT UQ_Venta_Cluster UNIQUE CLUSTERED (IdTenant, FecVenta, IdVenta)
        // Cluster por tenant+fecha: acceso secuencial para reportes de caja
        b.HasIndex(x => new { x.IdTenant, x.FecVenta, x.IdVenta })
            .IsUnique()
            .IsClustered()                      // ← UNIQUE CLUSTERED
            .HasDatabaseName("UQ_Venta_Cluster");

        // CONSTRAINT UQ_Venta_Num UNIQUE (IdTenant, NumVenta)
        b.HasIndex(x => new { x.IdTenant, x.NumVenta })
            .IsUnique()
            .HasDatabaseName("UQ_Venta_Num");

        // CREATE INDEX IX_Venta_Fecha ON Venta(IdTenant, FecVenta DESC) INCLUDE (Total, Estado)
        // Índice cubriente para cierre de caja y reportes: lee Total y Estado sin heap
        b.HasIndex(x => new { x.IdTenant, x.FecVenta })
            .IsDescending(false, true)          // FecVenta DESC
            .IncludeProperties(x => new { x.Total, x.Estado })
            .HasDatabaseName("IX_Venta_Fecha");

        // CREATE INDEX IX_Venta_Cliente ON Venta(IdCliente, FecVenta DESC)
        //   WHERE IdCliente IS NOT NULL
        b.HasIndex(x => new { x.IdCliente, x.FecVenta })
            .IsDescending(false, true)          // FecVenta DESC
            .HasFilter("[IdCliente] IS NOT NULL")
            .HasDatabaseName("IX_Venta_Cliente");

        // CREATE INDEX IX_Venta_Bodega ON Venta(IdBodega, FecVenta DESC)
        b.HasIndex(x => new { x.IdBodega, x.FecVenta })
            .IsDescending(false, true)          // FecVenta DESC
            .HasDatabaseName("IX_Venta_Bodega");

        b.HasOne(x => x.Tenant)
            .WithMany(t => t.Ventas)
            .HasForeignKey(x => x.IdTenant)
            .HasConstraintName("FK_Venta_Tenant")
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Bodega)
            .WithMany(bd => bd.Ventas)
            .HasForeignKey(x => x.IdBodega)
            .HasConstraintName("FK_Venta_Bod")
            .OnDelete(DeleteBehavior.Restrict);

        // IdCliente nullable → SetNull (venta anónima válida)
        b.HasOne(x => x.Cliente)
            .WithMany(c => c.Ventas)
            .HasForeignKey(x => x.IdCliente)
            .HasConstraintName("FK_Venta_Cliente")
            .OnDelete(DeleteBehavior.SetNull);

        // S51.2 auditoría — IdUsuario/Usuario existen en dbo.Venta; UsuarioAnula
        // NO existe en BBDD congelada: se ignora (sin DDL). Si el owner aprueba
        // migración, revertir.
        b.Ignore(x => x.UsuarioAnula);
        b.Ignore(x => x.UsuarioAnulaNav);
        b.HasIndex(x => new { x.IdTenant, x.IdUsuario })
            .HasFilter("[IdUsuario] IS NOT NULL")
            .HasDatabaseName("IX_Venta_Usuario");
        b.HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.IdUsuario)
            .HasConstraintName("FK_Venta_Usuario")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ─── VentaDetalle ─────────────────────────────────────────────────────────────
// DISEÑO CRÍTICO: PK NONCLUSTERED + UQ CLUSTERED (IdVenta, NumLinea).
// El cluster por (IdVenta, NumLinea) garantiza localidad física de las líneas
// de una misma venta → acceso secuencial en impresión de ticket / factura.
internal sealed class VentaDetalleConfiguration : IEntityTypeConfiguration<VentaDetalle>
{
    public void Configure(EntityTypeBuilder<VentaDetalle> b)
    {
        b.ToTable("VentaDetalle");

        // CONSTRAINT PK_VentaDetalle PRIMARY KEY NONCLUSTERED (IdVentaDetalle)
        b.HasKey(x => x.IdVentaDetalle)
            .HasName("PK_VentaDetalle")
            .IsClustered(false);                // ← PK NONCLUSTERED

        b.Property(x => x.IdVentaDetalle).UseIdentityColumn();
        b.Property(x => x.NumLinea).HasColumnType("int");

        // DECIMAL(18,4) NOT NULL para todas las columnas monetarias/de cantidad
        b.Property(x => x.Cantidad).HasPrecision(18, 4);
        b.Property(x => x.PrecioUnitario).HasPrecision(18, 4);
        b.Property(x => x.CostoUnitario).HasPrecision(18, 4);
        b.Property(x => x.Descuento).HasPrecision(18, 4).HasDefaultValue(0m);
        b.Property(x => x.Subtotal).HasPrecision(18, 4);

        // CONSTRAINT UQ_VentaDetalle_Cluster UNIQUE CLUSTERED (IdVenta, NumLinea)
        b.HasIndex(x => new { x.IdVenta, x.NumLinea })
            .IsUnique()
            .IsClustered()                      // ← UNIQUE CLUSTERED
            .HasDatabaseName("UQ_VentaDetalle_Cluster");

        // CREATE INDEX IX_VentaDetalle_Reporte
        //   ON VentaDetalle(IdPresProd, IdVenta)
        //   INCLUDE (Cantidad, PrecioUnitario, CostoUnitario, Descuento)
        // Índice cubriente para reportes de venta por producto:
        // evita lookup al heap para leer todas las columnas de análisis.
        b.HasIndex(x => new { x.IdPresProd, x.IdVenta })
            .IncludeProperties(x => new { x.Cantidad, x.PrecioUnitario, x.CostoUnitario, x.Descuento })
            .HasDatabaseName("IX_VentaDetalle_Reporte");

        b.HasOne(x => x.Venta)
            .WithMany(v => v.Detalles)
            .HasForeignKey(x => x.IdVenta)
            .HasConstraintName("FK_VentaDet_Venta")
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.PresProd)
            .WithMany(p => p.DetallesVenta)
            .HasForeignKey(x => x.IdPresProd)
            .HasConstraintName("FK_VentaDet_Pres")
            .OnDelete(DeleteBehavior.Restrict);

        // Relación 1:1 — cada línea de venta tiene exactamente un MovInv
        b.HasOne(x => x.MovInv)
            .WithOne(m => m.VentaDetalle)
            .HasForeignKey<VentaDetalle>(x => x.IdMovInv)
            .HasConstraintName("FK_VentaDet_Mov")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ─── MetodoPago ───────────────────────────────────────────────────────────────
internal sealed class MetodoPagoConfiguration : IEntityTypeConfiguration<MetodoPago>
{
    public void Configure(EntityTypeBuilder<MetodoPago> b)
    {
        b.ToTable("MetodoPago");

        // TINYINT IDENTITY PRIMARY KEY — C# mapea como byte
        b.HasKey(x => x.IdMetodoPago)
            .HasName("PK_MetodoPago");

        b.Property(x => x.IdMetodoPago)
            .HasColumnType("tinyint")
            .UseIdentityColumn();

        b.Property(x => x.Codigo).HasColumnType("varchar(20)").IsRequired();
        b.Property(x => x.Nombre).HasColumnType("varchar(50)").IsRequired();
        b.Property(x => x.Activo).HasColumnType("bit").HasDefaultValue(true);

        b.HasIndex(x => x.Codigo)
            .IsUnique()
            .HasDatabaseName("UQ_MetodoPago_Codigo");
    }
}

// ─── VentaPago ────────────────────────────────────────────────────────────────
internal sealed class VentaPagoConfiguration : IEntityTypeConfiguration<VentaPago>
{
    public void Configure(EntityTypeBuilder<VentaPago> b)
    {
        b.ToTable("VentaPago");

        b.HasKey(x => x.IdVentaPago)
            .HasName("PK_VentaPago");

        b.Property(x => x.IdVentaPago).UseIdentityColumn();
        b.Property(x => x.IdMetodoPago).HasColumnType("tinyint");

        // DECIMAL(18,4) NOT NULL
        b.Property(x => x.Monto).HasPrecision(18, 4);

        b.Property(x => x.Referencia).HasColumnType("varchar(100)");

        // CREATE INDEX IX_VentaPago_Venta ON VentaPago(IdVenta)
        b.HasIndex(x => x.IdVenta)
            .HasDatabaseName("IX_VentaPago_Venta");

        b.HasOne(x => x.Venta)
            .WithMany(v => v.Pagos)
            .HasForeignKey(x => x.IdVenta)
            .HasConstraintName("FK_VentaPago_Venta")
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.MetodoPago)
            .WithMany(m => m.VentaPagos)
            .HasForeignKey(x => x.IdMetodoPago)
            .HasConstraintName("FK_VentaPago_Metodo")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ─── ProductoImagen ────────────────────────────────────────────────────────
internal sealed class ProductoImagenConfiguration : IEntityTypeConfiguration<ProductoImagen>
{
    public void Configure(EntityTypeBuilder<ProductoImagen> b)
    {
        b.ToTable("ProductoImagen");

        b.HasKey(x => x.IdProductoImagen)
            .HasName("PK_ProductoImagen");

        b.Property(x => x.IdProductoImagen).UseIdentityColumn();
        b.Property(x => x.Url).HasColumnType("varchar(500)").IsRequired();
        b.Property(x => x.UrlMiniatura).HasColumnType("varchar(500)");
        b.Property(x => x.UrlFicha).HasColumnType("varchar(500)");
        b.Property(x => x.NombreArchivo).HasColumnType("varchar(255)").IsRequired();
        b.Property(x => x.ContentType).HasColumnType("varchar(100)").IsRequired();
        b.Property(x => x.TamanoBytes).HasColumnType("bigint");
        b.Property(x => x.Orden).HasColumnType("int").HasDefaultValue(0);
        b.Property(x => x.EsPrincipal).HasColumnType("bit").HasDefaultValue(false);
        b.Property(x => x.Activo).HasColumnType("bit").HasDefaultValue(true);
        b.Property(x => x.FecCrea).HasColumnType("datetime2").HasDefaultValueSql("SYSDATETIME()");

        b.HasIndex(x => x.IdProd).HasDatabaseName("IX_ProductoImagen_Prod");
        b.HasIndex(x => new { x.IdProd, x.Orden }).HasDatabaseName("IX_ProductoImagen_Orden");
        b.HasIndex(x => new { x.IdProd, x.EsPrincipal }).HasDatabaseName("IX_ProductoImagen_Principal");
        b.Property(x => x.IdArchivo).HasColumnType("bigint");
        b.HasIndex(x => x.IdArchivo).HasDatabaseName("IX_ProductoImagen_Archivo");

        b.HasOne(x => x.Prod)
            .WithMany()
            .HasForeignKey(x => x.IdProd)
            .HasConstraintName("FK_ProductoImagen_Prod")
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Archivo)
            .WithMany()
            .HasForeignKey(x => x.IdArchivo)
            .HasConstraintName("FK_ProductoImagen_Archivo")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ─── Archivo (S104: storage) ───────────────────────────────────────────────
internal sealed class ArchivoConfiguration : IEntityTypeConfiguration<Archivo>
{
    public void Configure(EntityTypeBuilder<Archivo> b)
    {
        b.ToTable("Archivo");

        b.HasKey(x => x.IdArchivo)
            .HasName("PK_Archivo");

        b.Property(x => x.IdArchivo).UseIdentityColumn();
        b.Property(x => x.Proveedor).HasColumnType("varchar(30)").IsRequired();
        b.Property(x => x.Contenedor).HasColumnType("varchar(30)").IsRequired();
        b.Property(x => x.Clave).HasColumnType("varchar(400)").IsRequired();
        b.Property(x => x.Entidad).HasColumnType("varchar(60)").IsRequired();
        b.Property(x => x.IdEntidad).HasColumnType("bigint");
        b.Property(x => x.NombreArchivo).HasColumnType("varchar(255)").IsRequired();
        b.Property(x => x.ContentType).HasColumnType("varchar(100)").IsRequired();
        b.Property(x => x.TamanoBytes).HasColumnType("bigint");
        b.Property(x => x.ETag).HasColumnType("varchar(100)");
        b.Property(x => x.Estado).HasColumnType("tinyint");
        b.Property(x => x.FecCrea).HasColumnType("datetime2").HasDefaultValueSql("SYSDATETIME()");
        b.Property(x => x.FecEstado).HasColumnType("datetime2");

        b.HasIndex(x => x.Clave).IsUnique().HasDatabaseName("UX_Archivo_Clave");
        b.HasIndex(x => new { x.IdTenant, x.Estado }).HasDatabaseName("IX_Archivo_Tenant_Estado");

        b.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.IdTenant)
            .HasConstraintName("FK_Archivo_Tenant")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ─── Usuario ─────────────────────────────────────────────────────────────────
internal sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> b)
    {
        b.ToTable("Usuario");
        b.HasKey(x => x.IdUsuario).HasName("PK_Usuario");
        b.Property(x => x.IdUsuario).ValueGeneratedNever();
        b.Property(x => x.NomUsuario).HasColumnType("nvarchar(100)").IsRequired();
        b.Property(x => x.Email).HasColumnType("nvarchar(255)");
        b.Property(x => x.Nombre).HasColumnType("nvarchar(100)");
        b.Property(x => x.Apellido).HasColumnType("nvarchar(100)");
        b.Property(x => x.Activo).HasColumnType("bit").HasDefaultValue(true);
        b.Property(x => x.EsSistema).HasColumnType("bit").HasDefaultValue(false);
        b.Property(x => x.EmailVerificado).HasColumnType("bit").HasDefaultValue(false);
        b.Property(x => x.FechaSync).HasColumnType("datetime2").HasDefaultValueSql("SYSDATETIME()");
        b.Property(x => x.FecCrea).HasColumnType("datetime2").HasDefaultValueSql("SYSDATETIME()");
        b.Property(x => x.FecMod).HasColumnType("datetime2");
        b.HasIndex(x => x.NomUsuario).IsUnique().HasDatabaseName("UQ_Usuario_NomUsuario");
        b.HasIndex(x => x.Email).IsUnique().HasFilter("[Email] IS NOT NULL").HasDatabaseName("UQ_Usuario_Email");
    }
}

// ─── UsuarioTenant ────────────────────────────────────────────────────────────
internal sealed class UsuarioTenantConfiguration : IEntityTypeConfiguration<UsuarioTenant>
{
    public void Configure(EntityTypeBuilder<UsuarioTenant> b)
    {
        b.ToTable("UsuarioTenant");
        b.HasKey(x => new { x.IdUsuario, x.IdTenant }).HasName("PK_UsuarioTenant");
        b.Property(x => x.Activo).HasColumnType("bit").HasDefaultValue(true);
        b.Property(x => x.EsPrincipal).HasColumnType("bit").HasDefaultValue(false);
        b.Property(x => x.FechaSync).HasColumnType("datetime2").HasDefaultValueSql("SYSDATETIME()");
        b.HasIndex(x => new { x.IdTenant, x.Activo }).HasDatabaseName("IX_UsuarioTenant_Tenant");
        b.HasOne(x => x.Usuario).WithMany(u => u.UsuarioTenants).HasForeignKey(x => x.IdUsuario).HasConstraintName("FK_UsuarioTenant_Usuario").OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Tenant).WithMany(t => t.UsuarioTenants).HasForeignKey(x => x.IdTenant).HasConstraintName("FK_UsuarioTenant_Tenant").OnDelete(DeleteBehavior.Restrict);
    }
}

// ─── Configuracion ────────────────────────────────────────────────────────────
internal sealed class ConfiguracionConfiguration : IEntityTypeConfiguration<Configuracion>
{
    public void Configure(EntityTypeBuilder<Configuracion> b)
    {
        b.ToTable("Configuracion");
        b.HasKey(x => x.Id).HasName("PK_Configuracion");
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.PassPlatApiBaseUrl).HasColumnType("nvarchar(500)").IsRequired();
        b.Property(x => x.PassPlatTimeoutMs).HasColumnType("int").HasDefaultValue(10000);
        b.Property(x => x.PassPlatAmbiente).HasColumnType("varchar(20)").HasDefaultValue("Production");
        b.Property(x => x.Activo).HasColumnType("bit").HasDefaultValue(true);
        b.Property(x => x.FechaSync).HasColumnType("datetime2");
        b.Property(x => x.FecMod).HasColumnType("datetime2");
        // S57.24 — dbo.Configuracion materializada según contrato congelado Inventa.sql:713-731:
        // UsuarioMod reactivo (FK_Configuracion_UsuarioMod → dbo.Usuario), mismo patrón que TenantConfigInventa.
        b.Property(x => x.UsuarioMod).HasColumnType("int");
        b.HasOne(x => x.UsuarioModNav).WithMany().HasForeignKey(x => x.UsuarioMod).HasConstraintName("FK_Configuracion_UsuarioMod").OnDelete(DeleteBehavior.Restrict);
    }
}

// ─── S57.1 TenantStorefront ───────────────────────────────────────────────────
internal sealed class TenantStorefrontConfiguration : IEntityTypeConfiguration<TenantStorefront>
{
    public void Configure(EntityTypeBuilder<TenantStorefront> b)
    {
        b.ToTable("TenantStorefront");
        b.HasKey(x => x.IdTenant).HasName("PK_TenantStorefront");
        b.Property(x => x.IdTenant).ValueGeneratedNever();
        b.Property(x => x.NombreFantasia).HasColumnType("nvarchar(200)");
        b.Property(x => x.DescripcionCorta).HasColumnType("nvarchar(500)");
        b.Property(x => x.Descripcion).HasColumnType("nvarchar(4000)");
        b.Property(x => x.LogoUrl).HasColumnType("varchar(500)");
        b.Property(x => x.Telefono).HasColumnType("varchar(50)");
        b.Property(x => x.WhatsApp).HasColumnType("varchar(50)");
        b.Property(x => x.Email).HasColumnType("nvarchar(255)");
        b.Property(x => x.Direccion).HasColumnType("nvarchar(300)");
        b.Property(x => x.Comuna).HasColumnType("nvarchar(100)");
        b.Property(x => x.Ciudad).HasColumnType("nvarchar(100)");
        b.Property(x => x.Region).HasColumnType("nvarchar(100)");
        b.Property(x => x.SitioWeb).HasColumnType("varchar(300)");
        // S60.1 Branding público (coherente con Inventa_S60_001_StorefrontBranding.sql).
        b.Property(x => x.ColorAcento).HasColumnType("varchar(7)");
        b.Property(x => x.FaviconUrl).HasColumnType("varchar(500)");
        b.Property(x => x.AnuncioTexto).HasColumnType("nvarchar(200)");
        b.Property(x => x.PerfilStore).HasColumnType("varchar(20)").IsRequired().HasDefaultValue("General");
        b.Property(x => x.Publicado).HasColumnType("bit").HasDefaultValue(false);
        b.Property(x => x.FecCrea).HasColumnType("datetime2").HasDefaultValueSql("SYSDATETIME()");
        b.Property(x => x.FecMod).HasColumnType("datetime2");
        b.HasOne(x => x.Tenant).WithOne().HasForeignKey<TenantStorefront>(x => x.IdTenant).HasConstraintName("FK_TenantStorefront_Tenant").OnDelete(DeleteBehavior.Restrict);
        b.Ignore(x => x.SocialLinks);
        b.Ignore(x => x.BusinessHours);
    }
}

internal sealed class TenantSocialLinkConfiguration : IEntityTypeConfiguration<TenantSocialLink>
{
    private static readonly string[] Allowed = ["facebook", "instagram", "tiktok", "x", "linkedin", "youtube", "whatsapp", "web"];
    public void Configure(EntityTypeBuilder<TenantSocialLink> b)
    {
        b.ToTable("TenantSocialLink");
        b.HasKey(x => x.IdTenantSocialLink).HasName("PK_TenantSocialLink");
        b.Property(x => x.IdTenantSocialLink).UseIdentityColumn();
        b.Property(x => x.Tipo).HasColumnType("varchar(30)").IsRequired();
        b.Property(x => x.Url).HasColumnType("varchar(500)").IsRequired();
        b.Property(x => x.Orden).HasColumnType("int").HasDefaultValue(0);
        b.Property(x => x.Activo).HasColumnType("bit").HasDefaultValue(true);
        b.HasIndex(x => new { x.IdTenant, x.Orden }).HasDatabaseName("IX_TenantSocialLink_Tenant");
        b.HasOne(x => x.Tenant).WithMany(t => t.SocialLinks).HasForeignKey(x => x.IdTenant).HasConstraintName("FK_TenantSocialLink_Tenant").OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class TenantBusinessHourConfiguration : IEntityTypeConfiguration<TenantBusinessHour>
{
    public void Configure(EntityTypeBuilder<TenantBusinessHour> b)
    {
        b.ToTable("TenantBusinessHour");
        b.HasKey(x => x.IdTenantBusinessHour).HasName("PK_TenantBusinessHour");
        b.Property(x => x.IdTenantBusinessHour).UseIdentityColumn();
        b.Property(x => x.DiaSemana).HasColumnType("tinyint");
        b.Property(x => x.Apertura1).HasColumnType("time");
        b.Property(x => x.Cierre1).HasColumnType("time");
        b.Property(x => x.Apertura2).HasColumnType("time");
        b.Property(x => x.Cierre2).HasColumnType("time");
        b.Property(x => x.Cerrado).HasColumnType("bit").HasDefaultValue(false);
        b.Property(x => x.Activo).HasColumnType("bit").HasDefaultValue(true);
        b.HasIndex(x => new { x.IdTenant, x.DiaSemana }).IsUnique().HasDatabaseName("UQ_TenantBusinessHour_TenantDia");
        b.HasOne(x => x.Tenant).WithMany(t => t.BusinessHours).HasForeignKey(x => x.IdTenant).HasConstraintName("FK_TenantBusinessHour_Tenant").OnDelete(DeleteBehavior.Cascade);
    }
}

// ─── TenantConfigInventa ──────────────────────────────────────────────────────
internal sealed class TenantConfigInventaConfiguration : IEntityTypeConfiguration<TenantConfigInventa>
{
    public void Configure(EntityTypeBuilder<TenantConfigInventa> b)
    {
        b.ToTable("TenantConfigInventa");
        b.HasKey(x => x.IdTenant).HasName("PK_TenantConfigInventa");
        b.Property(x => x.IdTenant).ValueGeneratedNever();
        b.Property(x => x.MaxBodegas).HasColumnType("smallint").HasDefaultValue((short)10);
        b.Property(x => x.PermiteStockNegativo).HasColumnType("bit").HasDefaultValue(false);
        b.Property(x => x.Moneda).HasColumnType("varchar(3)").HasDefaultValue("CLP");
        b.Property(x => x.TasaImpuesto).HasPrecision(5, 2).HasDefaultValue(0m);
        b.Property(x => x.DiasRetencionMov).HasColumnType("int").HasDefaultValue(365);
        b.Property(x => x.PermiteVentaSinCliente).HasColumnType("bit").HasDefaultValue(true);
        b.Property(x => x.FecMod).HasColumnType("datetime2");
        b.Property(x => x.UsuarioMod).HasColumnType("int");
        b.HasOne(x => x.Tenant).WithOne(t => t.TenantConfig).HasForeignKey<TenantConfigInventa>(x => x.IdTenant).HasConstraintName("FK_TenantConfig_Tenant").OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.UsuarioModNav).WithMany().HasForeignKey(x => x.UsuarioMod).HasConstraintName("FK_TenantConfig_UsuarioMod").OnDelete(DeleteBehavior.Restrict);
    }
}

// ─── Promocion (S57.66 — transcribe DDL/03 + 04) ─────────────────────────
internal sealed class PromocionConfiguration : IEntityTypeConfiguration<Promocion>
{
    public void Configure(EntityTypeBuilder<Promocion> b)
    {
        b.ToTable("Promocion");

        b.HasKey(x => x.IdPromocion)
            .HasName("PK_Promocion");

        b.Property(x => x.IdPromocion).UseIdentityColumn();

        b.Property(x => x.Codigo).HasColumnType("varchar(30)").IsRequired().HasMaxLength(30);
        b.Property(x => x.Nombre).HasColumnType("nvarchar(120)").IsRequired().HasMaxLength(120);
        b.Property(x => x.TextoComercial).HasColumnType("nvarchar(250)").HasMaxLength(250);
        b.Property(x => x.TipoMecanica).HasColumnType("tinyint");
        b.Property(x => x.PoliticaPrecio).HasColumnType("tinyint").HasDefaultValue((byte)1);

        b.Property(x => x.FechaDesde).HasColumnType("date");
        b.Property(x => x.FechaHasta).HasColumnType("date");

        b.Property(x => x.Prioridad).HasColumnType("smallint").HasDefaultValue((short)100);
        b.Property(x => x.Exclusiva).HasColumnType("bit").HasDefaultValue(false);
        b.Property(x => x.Activo).HasColumnType("bit").HasDefaultValue(true);
        b.Property(x => x.Publicado).HasColumnType("bit").HasDefaultValue(false);

        b.Property(x => x.FecCrea)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSDATETIME()");
        b.Property(x => x.FecMod).HasColumnType("datetime2");

        // CONSTRAINT UQ_Promocion_CodigoTenant UNIQUE (IdTenant, Codigo)
        b.HasIndex(x => new { x.IdTenant, x.Codigo })
            .IsUnique()
            .HasDatabaseName("UQ_Promocion_CodigoTenant");

        // CREATE INDEX IX_Promocion_Lookup ON Promocion(IdTenant, FechaDesde, FechaHasta, Activo, Publicado)
        b.HasIndex(x => new { x.IdTenant, x.FechaDesde, x.FechaHasta, x.Activo, x.Publicado })
            .HasDatabaseName("IX_Promocion_Lookup");

        b.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.IdTenant)
            .HasConstraintName("FK_Promocion_Tenant")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PromocionPresentacionConfiguration : IEntityTypeConfiguration<PromocionPresentacion>
{
    public void Configure(EntityTypeBuilder<PromocionPresentacion> b)
    {
        b.ToTable("PromocionPresentacion");

        b.HasKey(x => x.IdPart)
            .HasName("PK_PromocionPresentacion");

        b.Property(x => x.IdPart).UseIdentityColumn();
        b.Property(x => x.Cantidad).HasPrecision(18, 4).HasDefaultValue(1m);
        b.Property(x => x.EsMuestra).HasColumnType("bit").HasDefaultValue(false);

        // CREATE INDEX IX_PromocionPres_Promo ON PromocionPresentacion(IdPromocion, Rol)
        b.HasIndex(x => new { x.IdPromocion, x.Rol })
            .HasDatabaseName("IX_PromocionPres_Promo");

        // CREATE INDEX IX_PromocionPres_Inverso ON PromocionPresentacion(IdPresProd)
        b.HasIndex(x => x.IdPresProd)
            .HasDatabaseName("IX_PromocionPres_Inverso");

        b.HasOne(x => x.Promocion)
            .WithMany(p => p.Presentaciones)
            .HasForeignKey(x => x.IdPromocion)
            .HasConstraintName("FK_PromocionPres_Promocion")
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.PresProd)
            .WithMany(p => p.PromocionPresentaciones)
            .HasForeignKey(x => x.IdPresProd)
            .HasConstraintName("FK_PromocionPres_PresProd")
            .OnDelete(DeleteBehavior.Restrict);

        // FK_PromocionPres_MismaPres (IdPrecioReferencia,IdPresProd)->Precio(IdPrecio,IdPresProd):
        // integridad DB real (DDL S57.66); el servicio valida misma-presentación antes
        // del INSERT para devolver 422 en vez de SqlException. NULL (muestra) => sin check.
        b.HasOne(x => x.PrecioReferencia)
            .WithMany(p => p.PromocionPresentacionesReferenciadas)
            .HasForeignKey(x => new { x.IdPrecioReferencia, x.IdPresProd })
            .HasPrincipalKey(p => new { p.IdPrecio, p.IdPresProd })
            .HasConstraintName("FK_PromocionPres_MismaPres")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PromocionReglaConfiguration : IEntityTypeConfiguration<PromocionRegla>
{
    public void Configure(EntityTypeBuilder<PromocionRegla> b)
    {
        b.ToTable("PromocionRegla");

        b.HasKey(x => x.IdRegla)
            .HasName("PK_PromocionRegla");

        b.Property(x => x.IdRegla).UseIdentityColumn();
        b.Property(x => x.TipoRegla).HasColumnType("tinyint");
        b.Property(x => x.Operador).HasColumnType("tinyint").HasDefaultValue((byte)1);
        b.Property(x => x.Valor).HasColumnType("varchar(100)").IsRequired().HasMaxLength(100);

        b.HasOne(x => x.Promocion)
            .WithMany(p => p.Reglas)
            .HasForeignKey(x => x.IdPromocion)
            .HasConstraintName("FK_PromocionRegla_Promocion")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PromocionBeneficioConfiguration : IEntityTypeConfiguration<PromocionBeneficio>
{
    public void Configure(EntityTypeBuilder<PromocionBeneficio> b)
    {
        b.ToTable("PromocionBeneficio");

        b.HasKey(x => x.IdBeneficio)
            .HasName("PK_PromocionBeneficio");

        b.Property(x => x.IdBeneficio).UseIdentityColumn();
        b.Property(x => x.TipoBeneficio).HasColumnType("tinyint");
        b.Property(x => x.Valor).HasPrecision(18, 4);

        b.HasOne(x => x.Promocion)
            .WithMany(p => p.Beneficios)
            .HasForeignKey(x => x.IdPromocion)
            .HasConstraintName("FK_PromocionBeneficio_Promocion")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ─── S87 ImportacionLote ──────────────────────────────────────────────────────
internal sealed class ImportacionLoteConfiguration : IEntityTypeConfiguration<ImportacionLote>
{
    public void Configure(EntityTypeBuilder<ImportacionLote> b)
    {
        b.ToTable("ImportacionLote");

        b.HasKey(x => x.IdImportacionLote)
            .HasName("PK_ImportacionLote");

        b.Property(x => x.IdImportacionLote).UseIdentityColumn();
        b.Property(x => x.Tipo).HasColumnType("varchar(30)").IsRequired().HasMaxLength(30);
        b.Property(x => x.NombreArchivo).HasColumnType("nvarchar(255)").IsRequired().HasMaxLength(255);
        b.Property(x => x.Sha256).HasColumnType("char(64)").IsRequired().IsFixedLength().HasMaxLength(64);
        b.Property(x => x.RutaArchivo).HasColumnType("varchar(500)").IsRequired().HasMaxLength(500);
        b.Property(x => x.Estado).HasColumnType("varchar(20)").IsRequired().HasMaxLength(20);
        b.Property(x => x.FecCrea)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSDATETIME()");
        b.Property(x => x.ResumenJson).HasColumnType("nvarchar(max)");

        b.HasIndex(x => new { x.IdTenant, x.FecCrea })
            .IsDescending(false, true)
            .HasDatabaseName("IX_ImportacionLote_TenantFec");

        b.HasIndex(x => new { x.IdTenant, x.Sha256 })
            .HasDatabaseName("IX_ImportacionLote_TenantSha");

        b.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.IdTenant)
            .HasConstraintName("FK_ImportacionLote_Tenant")
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.IdUsuario)
            .HasConstraintName("FK_ImportacionLote_Usuario")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// PassPlat S136: fila genérica de CBP.Outbox sobre dbo.Outbox (Inventa_PP136_001_Outbox.sql). Sin FK.
internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> b)
    {
        b.ToTable("Outbox");

        b.HasKey(x => x.Id)
            .HasName("PK_Outbox");

        b.Property(x => x.Id).UseIdentityColumn();
        b.Property(x => x.EventId).HasDefaultValueSql("NEWID()");
        b.Property(x => x.EventType).HasColumnType("nvarchar(100)").IsRequired();
        b.Property(x => x.Payload).HasColumnType("nvarchar(max)").IsRequired();
        b.Property(x => x.CorrelationId).HasColumnType("nvarchar(64)").IsRequired();
        b.Property(x => x.Status).HasColumnType("nvarchar(20)").IsRequired().HasDefaultValue(OutboxEstados.Pending);
        b.Property(x => x.Attempts).HasDefaultValue(0);
        b.Property(x => x.CreatedAt).HasColumnType("datetime2(3)").HasDefaultValueSql("SYSDATETIME()");
        b.Property(x => x.ProcessingStartedAt).HasColumnType("datetime2(3)");
        b.Property(x => x.ProcessedAt).HasColumnType("datetime2(3)");
        b.Property(x => x.LastError).HasColumnType("nvarchar(max)");
        b.Property(x => x.NextAttemptAt).HasColumnType("datetime2(3)");

        b.HasIndex(x => x.EventId).IsUnique().HasDatabaseName("UX_Outbox_EventId");
        b.HasIndex(x => new { x.Status, x.CreatedAt }).HasDatabaseName("IX_Outbox_Pending_Status_CreatedOn");
        b.HasIndex(x => x.ProcessingStartedAt).HasDatabaseName("IX_Outbox_ProcessingStartedAt");
        b.HasIndex(x => new { x.Status, x.NextAttemptAt }).HasDatabaseName("IX_Outbox_Status_NextAttemptAt");
    }
}