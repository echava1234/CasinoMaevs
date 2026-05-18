namespace CasinoMaevs.Models;

/// <summary>
/// Rastrea las acciones limitadas diariamente del usuario.
/// Incluye: apertura de cajas, intento de trampa, etc.
/// </summary>
public class AccionesUsuarioDiarias
{
    public DateTime FechaRegistro { get; set; }
    public int AperturasDeHoyGeneradas { get; set; } = 0;
    public int IntentosTrampaHoy { get; set; } = 0;
    public bool YaAbrioCajasHoy { get; set; } = false;
    public DateTime UltimaAperturaIntento { get; set; } = DateTime.MinValue;

    public AccionesUsuarioDiarias()
    {
        FechaRegistro = DateTime.UtcNow.Date; // Solo la fecha, sin hora
    }

    /// <summary>
    /// Verifica si el registro es del día actual. Si no, reinicia los contadores.
    /// </summary>
    public void ReiniciarSiNuevoDia()
    {
        DateTime hoy = DateTime.UtcNow.Date;
        if (FechaRegistro != hoy)
        {
            FechaRegistro = hoy;
            AperturasDeHoyGeneradas = 0;
            IntentosTrampaHoy = 0;
            YaAbrioCajasHoy = false;
        }
    }

    public override string ToString()
    {
        return $"📅 {FechaRegistro.Date:yyyy-MM-dd} | Aperturas: {AperturasDeHoyGeneradas} | Intentos trampa: {IntentosTrampaHoy}";
    }
}
