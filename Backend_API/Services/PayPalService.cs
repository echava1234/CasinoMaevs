namespace CasinoMaevs.Services;

using CasinoMaevs.Models;
using Microsoft.Extensions.Configuration;

public class PayPalService
{
    private readonly string _mode;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly double _tasaConversion;

    public PayPalService(IConfiguration config)
    {
        _mode          = config["PayPal:Mode"] ?? "sandbox";
        _clientId      = config["PayPal:ClientId"] ?? string.Empty;
        _clientSecret  = config["PayPal:ClientSecret"] ?? string.Empty;
        _tasaConversion = double.Parse(config["Economia:TasaConversion"] ?? "100");
    }

    // Constructor alternativo para uso sin IConfiguration (consola simple)
    public PayPalService(string mode, string clientId, string clientSecret, double tasaConversion = 100)
    {
        _mode          = mode;
        _clientId      = clientId;
        _clientSecret  = clientSecret;
        _tasaConversion = tasaConversion;
    }

    // Simula la conexión al Sandbox de PayPal y acredita MAEVS TOKENS al usuario
    public bool ProcesarRecarga(Usuario usuario, double montoUsd)
    {
        Console.WriteLine($"\n  💳  Conectando con PayPal [{_mode.ToUpper()}]...");
        Console.WriteLine($"      ClientId: {_clientId[..10]}... (simulado)");

        try
        {
            if (montoUsd <= 0)
                throw new ArgumentException("El monto en USD debe ser mayor a 0.");

            // Simulación de latencia de pasarela
            Console.WriteLine("  🔄  Procesando transacción...");

            double tokensAcreditados = montoUsd * _tasaConversion;

            // Acreditación segura usando la propiedad validada de Usuario
            usuario.MaevsTokens += tokensAcreditados;

            Console.WriteLine($"  ✅  Pago aprobado. ${montoUsd} USD → {tokensAcreditados} MAEVS TOKENS acreditados.");
            Console.WriteLine($"      Nuevo saldo: {usuario.MaevsTokens} MAEVS TOKENS\n");

            return true;
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"  ❌  Error en la recarga: {ex.Message}");
            return false;
        }
        finally
        {
            Console.WriteLine("  🔒  Conexión con PayPal cerrada.");
        }
    }
}
