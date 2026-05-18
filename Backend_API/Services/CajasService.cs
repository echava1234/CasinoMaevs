namespace CasinoMaevs.Services;

using CasinoMaevs.Models;

/// <summary>
/// Gestiona la lógica de cajas con restricciones diarias y prevención de trampa.
/// </summary>
public class CajasService
{
    private static readonly Random _rng = new Random();

    /// <summary>
    /// Verifica si el usuario ya abrió cajas hoy.
    /// ESTO DEBE ESTAR EN BACKEND PARA EVITAR QUE MANIPULEN.
    /// </summary>
    public (bool puedeAbrir, string mensaje) VerificarPermisoCajasHoy(Usuario usuario)
    {
        // Primero, reinicia los contadores si es un nuevo día
        usuario.AccionesDiarias.ReiniciarSiNuevoDia();

        if (usuario.AccionesDiarias.YaAbrioCajasHoy)
        {
            return (false, "❌ Ya abriste cajas hoy. Vuelve mañana para una nueva apertura. ⏰");
        }

        return (true, "✅ Puedes abrir cajas hoy.");
    }

    /// <summary>
    /// Marca que el usuario ya abrió cajas hoy.
    /// </summary>
    public void MarcarCajasAbridasHoy(Usuario usuario)
    {
        usuario.AccionesDiarias.YaAbrioCajasHoy = true;
        usuario.AccionesDiarias.AperturasDeHoyGeneradas++;
    }

    /// <summary>
    /// Selecciona un personaje ALEATORIO para el usuario.
    /// No puede elegir: el sistema decide.
    /// Tipos: "Normal" (95%), "Sombra" (5%)
    /// </summary>
    public string GenerarPersonajeAleatorio()
    {
        double random = _rng.NextDouble();
        
        // 5% de probabilidad de ser Sombra
        if (random < 0.05)
        {
            Console.WriteLine("🌑 ¡Encontraste a SOMBRA! Puede hacer trampa...");
            return "Sombra";
        }

        Console.WriteLine("✨ Encontraste a un PERSONAJE NORMAL.");
        return "Normal";
    }

    /// <summary>
    /// Simula un intento de trampa. Solo funciona si es Sombra.
    /// Tiene un porcentaje muy bajo de éxito (15%).
    /// </summary>
    public (bool trampaExitosa, string mensaje, double bonusTokens) IntentarTrampa(Usuario usuario, string personajeActual)
    {
        // Primero, valida que sea Sombra
        if (personajeActual != "Sombra")
        {
            return (false, "❌ Solo Sombra puede hacer trampa.", 0);
        }

        // Incrementa contador de intentos
        usuario.AccionesDiarias.IntentosTrampaHoy++;

        // Si ya intentó trampa más de 3 veces hoy, se bloquea
        if (usuario.AccionesDiarias.IntentosTrampaHoy > 3)
        {
            return (false, "🔒 Ya intentaste trampa demasiadas veces. El sistema te bloqueó.", 0);
        }

        // 15% de probabilidad de éxito
        double random = _rng.NextDouble();
        if (random < 0.15)
        {
            double bonusTokens = _rng.Next(100, 500); // Bonus aleatorio 100-500
            Console.WriteLine($"💀 ¡Trampa exitosa! Sombra te regala {bonusTokens} tokens bonus.");
            return (true, $"💀 ¡La trampa funcionó! Recibiste {bonusTokens} tokens extra.", bonusTokens);
        }

        // 85% de fracaso
        Console.WriteLine("⚠️ La trampa falló. El sistema lo detectó.");
        return (false, "⚠️ ¡La trampa falló! El sistema anti-fraude de Sombra te detectó. Sin penalización.", 0);
    }

    /// <summary>
    /// Valida si está intentando hacer trampa múltiples veces muy rápido.
    /// Esto es un mecanismo anti-bot.
    /// </summary>
    public bool ValidarTiempoEntreIntentos(Usuario usuario)
    {
        TimeSpan tiempoTranscurrido = DateTime.UtcNow - usuario.AccionesDiarias.UltimaAperturaIntento;
        
        // Debe pasar al menos 2 segundos entre intentos
        if (tiempoTranscurrido.TotalSeconds < 2)
        {
            return false; // Bloqueado por ser muy rápido
        }

        usuario.AccionesDiarias.UltimaAperturaIntento = DateTime.UtcNow;
        return true;
    }
}
