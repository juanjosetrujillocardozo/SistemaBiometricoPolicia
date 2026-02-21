using System;
using System.Collections.Generic;

namespace SistemaBiometricoPolicia.Utils
{
    public enum EstadoComponente
    {
        Desconocido = 0,
        Ok = 1,
        Advertencia = 2,
        Error = 3
    }

    public class EstadoItem
    {
        public string Nombre { get; set; }
        public EstadoComponente Estado { get; set; }
        public string Detalle { get; set; }
        public DateTime Fecha { get; set; }
    }

    public static class StatusHub
    {
        private static readonly object _lock = new object();
        private static readonly Dictionary<string, EstadoItem> _estado = new Dictionary<string, EstadoItem>(StringComparer.OrdinalIgnoreCase);

        public static event Action<EstadoItem> EstadoActualizado;
        public static event Action<string> Evento;

        // ✅ MÉTODO ORIGINAL (mantener)
        public static void Set(string nombre, EstadoComponente estado, string detalle)
        {
            EstadoItem item;
            lock (_lock)
            {
                item = new EstadoItem
                {
                    Nombre = nombre,
                    Estado = estado,
                    Detalle = detalle ?? "",
                    Fecha = DateTime.Now
                };
                _estado[nombre] = item;
            }
            try { EstadoActualizado?.Invoke(item); }
            catch { /* evitar crash si no hay formularios cargados */ }
        }

        // ✅ MÉTODO NUEVO: Alias para compatibilidad con FormEnrolamiento
        public static void ActualizarEstado(string nombre, string detalle)
        {
            EstadoComponente estado = EstadoComponente.Ok;

            if (detalle.Contains("❌") || detalle.Contains("Error"))
                estado = EstadoComponente.Error;
            else if (detalle.Contains("⚠") || detalle.Contains("Advertencia"))
                estado = EstadoComponente.Advertencia;

            Set(nombre, estado, detalle);
        }

        public static EstadoItem Get(string nombre)
        {
            lock (_lock)
            {
                if (_estado.TryGetValue(nombre, out var item)) return item;
                return new EstadoItem { Nombre = nombre, Estado = EstadoComponente.Desconocido, Detalle = "Sin datos", Fecha = DateTime.MinValue };
            }
        }

        // ✅ MÉTODO ORIGINAL (mantener)
        public static void PushEvento(string mensaje)
        {
            if (string.IsNullOrWhiteSpace(mensaje)) return;
            Evento?.Invoke($"[{DateTime.Now:HH:mm:ss}] {mensaje}");
        }

        // ✅ MÉTODO NUEVO: Alias para compatibilidad
        public static void RegistrarEvento(string mensaje)
        {
            PushEvento(mensaje);
        }
    }
}