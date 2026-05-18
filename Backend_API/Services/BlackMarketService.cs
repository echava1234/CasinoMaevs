namespace CasinoMaevs.Services;

using CasinoMaevs.Models;

/// <summary>
/// Simula un mercado dinámico donde el precio del token fluctúa cada 30 segundos.
/// Emula un sistema de criptomonedas con aleatoriedad.
/// </summary>
public class BlackMarketService
{
    private double _precioActualUSD = 1.0; // 1 USD = 1 Token base
    private Timer? _temporizador = null;
    private static readonly Random _rng = new Random();

    public double PrecioActual => _precioActualUSD;

    /// <summary>
    /// Inicia el temporizador que cambia el precio cada intervalo.
    /// </summary>
    public void IniciarMercado(int intervaloSegundos = 30)
    {
        _temporizador = new Timer(_ =>
        {
            FluctuarPrecio();
        }, null, TimeSpan.FromSeconds(intervaloSegundos), TimeSpan.FromSeconds(intervaloSegundos));

        Console.WriteLine("📈 Mercado Negro iniciado.");
    }

    /// <summary>
    /// Fluctúa el precio usando una volatilidad controlada.
    /// </summary>
    private void FluctuarPrecio()
    {
        double cambio = ((_rng.NextDouble() - 0.5) * 0.1); // ±5% de volatilidad
        _precioActualUSD += cambio;
        _precioActualUSD = Math.Max(0.5, Math.Min(2.0, _precioActualUSD)); // Limita entre 0.5 y 2.0

        Console.WriteLine($"📊 Precio actualizado: 1 MAEVS = ${_precioActualUSD:F4} USD");
    }

    /// <summary>
    /// Calcula cuántos tokens obtiene el usuario por cierta cantidad en USD.
    /// </summary>
    public double ComprarTokens(double usd)
    {
        return usd / _precioActualUSD;
    }

    /// <summary>
    /// Calcula cuánto USD obtiene el usuario por cierta cantidad de tokens.
    /// </summary>
    public double VenderTokens(double tokens)
    {
        return tokens * _precioActualUSD;
    }

    public void Detener()
    {
        _temporizador?.Dispose();
        Console.WriteLine("📈 Mercado Negro detenido.");
    }
}
