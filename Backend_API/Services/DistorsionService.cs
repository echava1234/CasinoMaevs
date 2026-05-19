namespace CasinoMaevs.Services;

using CasinoMaevs.Models;

/// <summary>
/// Gestiona las distorsiones de red aleatorias que afectan el casino.
/// Implementa un patrón de generación temporal usando Timer o Task.
/// </summary>
public class DistorsionService
{
    private static readonly Random _rng = new Random();
    private DistorsionRed? _distorsionActual = null;
    private Timer? _temporizador = null;

    public DistorsionRed? ObtenerDistorsionActiva() => _distorsionActual;

    /// <summary>
    /// Inicia el servicio de distorsiones aleatorias.
    /// Cada intervalo genera una distorsión potencial.
    /// </summary>
    public void IniciarServicio(int intervaloSegundos = 300)
    {
        _temporizador = new Timer(_ =>
        {
            // 30% de probabilidad de generar una distorsión
            if (_rng.NextDouble() < 0.30)
            {
                GenerarDistorsionAleatoria();
            }
        }, null, TimeSpan.FromSeconds(intervaloSegundos), TimeSpan.FromSeconds(intervaloSegundos));

        Console.WriteLine("🌐 Servicio de Distorsión iniciado.");
    }

    /// <summary>
    /// Genera una distorsión aleatoria usando una colección predefinida.
    /// </summary>
    private void GenerarDistorsionAleatoria()
    {
        var distorsiones = new List<DistorsionRed>
        {
            new DistorsionRed("Tormenta Solar", "Aumenta premios +20% pero duplica costos", 0.20, 2.0, 300),
            new DistorsionRed("Falla Temporal", "Incrementa ganancias +15%, apuestas normales", 0.15, 1.0, 240),
            new DistorsionRed("Cortafuegos Activado", "Reduce premios -30%, apuestas x0.5", -0.30, 0.5, 180),
        };

        _distorsionActual = distorsiones[_rng.Next(distorsiones.Count)];
        Console.WriteLine($"\n⚡ DISTORSIÓN DETECTADA: {_distorsionActual}");
    }

    /// <summary>
    /// Aplica el modificador de distorsión a una ganancia de juego.
    /// </summary>
    public double AplicarDistorsion(double gananciaBase)
    {
        if (_distorsionActual == null || !_distorsionActual.EstaActiva)
            return gananciaBase;

        return gananciaBase * (1 + _distorsionActual.BonusGanancia);
    }

    /// <summary>
    /// Aplica el multiplicador de apuesta según distorsión activa.
    /// </summary>
    public double AplicarCostoDistorsion(double apuestaBase)
    {
        if (_distorsionActual == null || !_distorsionActual.EstaActiva)
            return apuestaBase;

        return apuestaBase * _distorsionActual.MultiplicadorApuesta;
    }

    public void Detener()
    {
        _temporizador?.Dispose();
        Console.WriteLine("🌐 Servicio de Distorsión detenido.");
    }
}
