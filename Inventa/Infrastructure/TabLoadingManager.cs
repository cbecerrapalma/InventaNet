using System;
using System.Collections.Generic;

namespace Inventa.Infrastructure
{
    /// <summary>
    /// Estados posibles para la carga lazy de pestañas.
    /// </summary>
    public enum LazyLoadState
    {
        /// <summary>
        /// Tab nunca ha sido seleccionado/cargado.
        /// </summary>
        NotLoaded = 0,

        /// <summary>
        /// Tab está en proceso de carga.
        /// </summary>
        Loading = 1,

        /// <summary>
        /// Tab ha sido cargado exitosamente.
        /// </summary>
        Loaded = 2,

        /// <summary>
        /// Erro durante la carga.
        /// </summary>
        Error = 3,

        /// <summary>
        /// Tab fue descargado (para liberar recursos).
        /// </summary>
        Unloaded = 4
    }

    /// <summary>
    /// Gestor de estado de carga para pestañas en lazy-loading.
    /// Rastrea qué pestañas han sido cargadas y en qué estado se encuentran.
    /// 
    /// Uso: Se instancia una vez por formulario.
    /// </summary>
    public class TabLoadingManager
    {
        private readonly Dictionary<string, TabLoadingState> _tabStates =
            new Dictionary<string, TabLoadingState>(StringComparer.OrdinalIgnoreCase);

        private object _lockObject = new object();

        // ─────────────────────────────────────────────────────────────────────────
        // Métodos públicos
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Obtiene el estado actual de una pestaña.
        /// Devuelve NotLoaded si la pestaña no existe en el registro.
        /// </summary>
        public LazyLoadState GetState(string tabKey)
        {
            lock (_lockObject)
            {
                return _tabStates.TryGetValue(tabKey, out var state)
                    ? state.State
                    : LazyLoadState.NotLoaded;
            }
        }

        /// <summary>
        /// Establece el estado de una pestaña.
        /// </summary>
        public void SetState(string tabKey, LazyLoadState newState, string? errorMessage = null)
        {
            lock (_lockObject)
            {
                if (!_tabStates.ContainsKey(tabKey))
                {
                    _tabStates[tabKey] = new TabLoadingState { TabKey = tabKey };
                }

                var state = _tabStates[tabKey];
                state.State = newState;
                state.LastStateChange = DateTime.UtcNow;
                state.ErrorMessage = errorMessage;

                if (newState == LazyLoadState.Loaded)
                    state.LoadedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Verifica si una pestaña está completamente cargada.
        /// </summary>
        public bool IsLoaded(string tabKey)
        {
            return GetState(tabKey) == LazyLoadState.Loaded;
        }

        /// <summary>
        /// Verifica si una pestaña está en proceso de carga.
        /// </summary>
        public bool IsLoading(string tabKey)
        {
            return GetState(tabKey) == LazyLoadState.Loading;
        }

        /// <summary>
        /// Obtiene información de estado detallada de una pestaña.
        /// </summary>
        public TabLoadingState? GetStateInfo(string tabKey)
        {
            lock (_lockObject)
            {
                return _tabStates.TryGetValue(tabKey, out var state)
                    ? new TabLoadingState
                    {
                        TabKey = state.TabKey,
                        State = state.State,
                        LoadedAt = state.LoadedAt,
                        LastStateChange = state.LastStateChange,
                        ErrorMessage = state.ErrorMessage
                    }
                    : null;
            }
        }

        /// <summary>
        /// Obtiene todas las pestañas que están cargadas.
        /// </summary>
        public List<string> GetLoadedTabs()
        {
            lock (_lockObject)
            {
                var loaded = new List<string>();
                foreach (var kvp in _tabStates)
                {
                    if (kvp.Value.State == LazyLoadState.Loaded)
                        loaded.Add(kvp.Key);
                }
                return loaded;
            }
        }

        /// <summary>
        /// Marca una pestaña para recarga (resetea a NotLoaded).
        /// </summary>
        public void MarkForReload(string tabKey)
        {
            SetState(tabKey, LazyLoadState.NotLoaded);
        }

        /// <summary>
        /// Marca todas las pestañas para recarga.
        /// </summary>
        public void MarkAllForReload()
        {
            lock (_lockObject)
            {
                foreach (var kvp in _tabStates)
                {
                    kvp.Value.State = LazyLoadState.NotLoaded;
                }
            }
        }

        /// <summary>
        /// Limpia el registro de estados (útil cuando se cierra el formulario).
        /// </summary>
        public void Clear()
        {
            lock (_lockObject)
            {
                _tabStates.Clear();
            }
        }

        /// <summary>
        /// Obtiene estadísticas de carga.
        /// </summary>
        public TabLoadingStatistics GetStatistics()
        {
            lock (_lockObject)
            {
                var stats = new TabLoadingStatistics();

                foreach (var state in _tabStates.Values)
                {
                    switch (state.State)
                    {
                        case LazyLoadState.NotLoaded:
                            stats.NotLoadedCount++;
                            break;
                        case LazyLoadState.Loading:
                            stats.LoadingCount++;
                            break;
                        case LazyLoadState.Loaded:
                            stats.LoadedCount++;
                            if (state.LoadedAt.HasValue)
                                stats.TotalLoadTimeMs += (long)(DateTime.UtcNow - state.LoadedAt.Value).TotalMilliseconds;
                            break;
                        case LazyLoadState.Error:
                            stats.ErrorCount++;
                            break;
                        case LazyLoadState.Unloaded:
                            stats.UnloadedCount++;
                            break;
                    }
                }

                stats.TotalTabs = _tabStates.Count;
                if (stats.LoadedCount > 0)
                    stats.AverageLoadTimeMs = stats.TotalLoadTimeMs / stats.LoadedCount;

                return stats;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Clases de soporte
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Estado detallado de una pestaña individual.
    /// </summary>
    public class TabLoadingState
    {
        public string TabKey { get; set; } = "";
        public LazyLoadState State { get; set; } = LazyLoadState.NotLoaded;
        public DateTime? LoadedAt { get; set; }
        public DateTime LastStateChange { get; set; } = DateTime.UtcNow;
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Estadísticas agregadas de carga de pestañas.
    /// </summary>
    public class TabLoadingStatistics
    {
        public int TotalTabs { get; set; }
        public int NotLoadedCount { get; set; }
        public int LoadingCount { get; set; }
        public int LoadedCount { get; set; }
        public int ErrorCount { get; set; }
        public int UnloadedCount { get; set; }
        public long TotalLoadTimeMs { get; set; }
        public long AverageLoadTimeMs { get; set; }

        public double LoadedPercentage =>
            TotalTabs > 0 ? (double)LoadedCount / TotalTabs * 100 : 0;
    }
}
