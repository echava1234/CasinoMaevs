namespace CasinoMaevs.Services;

// Excepción personalizada que hereda de Exception
// Se lanza cuando el usuario intenta apostar más de lo que tiene
public class FondosInsuficientesException : Exception
{
    public double SaldoActual { get; }
    public double ApuestaIntentada { get; }

    public FondosInsuficientesException(double saldoActual, double apuestaIntentada)
        : base($"Fondos insuficientes. Saldo actual: {saldoActual} MAEVS TOKENS. Apuesta intentada: {apuestaIntentada} MAEVS TOKENS.")
    {
        SaldoActual = saldoActual;
        ApuestaIntentada = apuestaIntentada;
    }

    public FondosInsuficientesException(string mensaje) : base(mensaje) { }
}
