namespace CasinoMaevs.Services;

using CasinoMaevs.Models;

/// <summary>
/// Implementa el "Botón de Pánico" - Retiro de Emergencia.
/// El usuario puede rescatar el 50% de su apuesta y enfriar cabeza por 30 seg.
/// </summary>
public class PanicSystem
{
    private Dictionary<string, DateTime> _bloqueosPorUsuario = new Dictionary<string, DateTime>();
    private const int SEGUNDOS_ENFRIAMIENTO = 30;

    /// <summary>
    /// Retira el 50% de una apuesta como medida de seguridad.
    /// Bloquea al usuario por 30 segundos para "enfriar la cabeza".
    /// </summary>
    public (bool exito, string mensaje, double montoBloqueado) EmergencyExtract(Usuario usuario, double apuestaActual)
    {
        // Verifica si está en bloqueo de enfriamiento
        if (_bloqueosPorUsuario.TryGetValue(usuario.Username, out var fechaBloqueo))
        {
            double segundosRestantes = (SEGUNDOS_ENFRIAMIENTO - (DateTime.UtcNow - fechaBloqueo).TotalSeconds);
            if (segundosRestantes > 0)
            {
                return (false, $"⏳ Estás en período de enfriamiento. Espera {segundosRestantes:F0} segundos.", 0);
            }
        }

        // Calcula el 50% de retiro seguro
        double montoBloqueado = apuestaActual * 0.5;
        usuario.MaevsTokens += montoBloqueado;

        // Registra el bloqueo de enfriamiento
        _bloqueosPorUsuario[usuario.Username] = DateTime.UtcNow;

        string mensaje = $"🚨 EXTRACCIÓN DE EMERGENCIA: Rescataste {montoBloqueado} MAEVS del peligro.";
        Console.WriteLine(mensaje);

        return (true, mensaje, montoBloqueado);
    }

    /// <summary>
    /// Verifica si un usuario está en enfriamiento.
    /// </summary>
    public double ObtenerTiempoEnfriamientoRestante(Usuario usuario)
    {
        if (!_bloqueosPorUsuario.TryGetValue(usuario.Username, out var fecha))
            return 0;

        double restante = SEGUNDOS_ENFRIAMIENTO - (DateTime.UtcNow - fecha).TotalSeconds;
        return Math.Max(0, restante);
    }
}
