namespace CasinoMaevs.Services;

using CasinoMaevs.Models;

// ─── CLASE ABSTRACTA BASE ────────────────────────────────────────────────────

public abstract class Juego
{
    public string Nombre { get; protected set; }
    public double ApuestaMinima { get; protected set; }

    protected static readonly Random _rng = new Random();

    protected Juego(string nombre, double apuestaMinima)
    {
        Nombre = nombre;
        ApuestaMinima = apuestaMinima;
    }

    // Método abstracto: cada juego define su propia lógica
    // Lanza FondosInsuficientesException si el saldo es insuficiente
    public abstract double Jugar(Usuario usuario, double apuesta);

    // Método protegido reutilizable para validar apuesta antes de cada juego
    protected void ValidarApuesta(Usuario usuario, double apuesta)
    {
        if (apuesta < ApuestaMinima)
            throw new ArgumentException($"La apuesta mínima para {Nombre} es {ApuestaMinima} MAEVS TOKENS.");

        if (usuario.MaevsTokens < apuesta)
            throw new FondosInsuficientesException(usuario.MaevsTokens, apuesta);
    }
}

// ─── JUEGO 1: SLOTS ──────────────────────────────────────────────────────────

public class SlotsJuego : Juego
{
    private static readonly string[] _simbolos = { "🍒", "🍋", "🔔", "⭐", "💎", "7️⃣" };

    public SlotsJuego() : base("Slots Fortuna", 10) { }

    public override double Jugar(Usuario usuario, double apuesta)
    {
        ValidarApuesta(usuario, apuesta);
        usuario.MaevsTokens -= apuesta;

        string s1 = _simbolos[_rng.Next(_simbolos.Length)];
        string s2 = _simbolos[_rng.Next(_simbolos.Length)];
        string s3 = _simbolos[_rng.Next(_simbolos.Length)];

        Console.WriteLine($"  🎰  Resultado: [ {s1} | {s2} | {s3} ]");

        double ganancia = 0;

        if (s1 == s2 && s2 == s3)
        {
            ganancia = apuesta * 5;
            Console.WriteLine($"  🏆  ¡JACKPOT! Los 3 símbolos coinciden. Ganancia: {ganancia} Tokens");
        }
        else if (s1 == s2 || s2 == s3 || s1 == s3)
        {
            ganancia = apuesta * 1.5;
            Console.WriteLine($"  ✨  ¡2 símbolos coinciden! Ganancia: {ganancia} Tokens");
        }
        else
        {
            Console.WriteLine("  💨  Sin coincidencias. Mejor suerte la próxima.");
        }

        usuario.MaevsTokens += ganancia;
        return ganancia;
    }
}

// ─── JUEGO 2: BLACKJACK SIMPLIFICADO ─────────────────────────────────────────

public class BlackjackSimplificado : Juego
{
    public BlackjackSimplificado() : base("Blackjack Simplificado", 20) { }

    public override double Jugar(Usuario usuario, double apuesta)
    {
        ValidarApuesta(usuario, apuesta);
        usuario.MaevsTokens -= apuesta;

        int puntajeJugador = _rng.Next(12, 22); // 12 a 21 inclusive
        int puntajeCasa    = _rng.Next(12, 22);

        Console.WriteLine($"  🃏  Jugador: {puntajeJugador} | Casa: {puntajeCasa}");

        double ganancia = 0;

        if (puntajeJugador > puntajeCasa)
        {
            ganancia = apuesta * 2;
            Console.WriteLine($"  🏆  ¡Ganaste! Tu puntaje supera a la casa. Ganancia: {ganancia} Tokens");
        }
        else if (puntajeJugador == puntajeCasa)
        {
            ganancia = apuesta; // Empate: devuelve la apuesta
            Console.WriteLine($"  🤝  Empate. Se devuelve la apuesta: {ganancia} Tokens");
        }
        else
        {
            Console.WriteLine("  💔  La casa gana. Sigue intentando.");
        }

        usuario.MaevsTokens += ganancia;
        return ganancia;
    }
}

// ─── JUEGO 3: RULETA DE SUERTE ────────────────────────────────────────────────

public class RuletaSuerte : Juego
{
    public RuletaSuerte() : base("Ruleta de Suerte", 15) { }

    public override double Jugar(Usuario usuario, double apuesta)
    {
        ValidarApuesta(usuario, apuesta);
        usuario.MaevsTokens -= apuesta;

        // 45% de probabilidad de ganar al color
        bool gano = _rng.NextDouble() < 0.45;
        string[] colores = { "🔴 Rojo", "⚫ Negro" };
        string colorJugador = colores[_rng.Next(2)];
        string colorResultado = colores[_rng.Next(2)];

        Console.WriteLine($"  🎡  Apostaste a: {colorJugador} | Resultado: {colorResultado}");

        double ganancia = 0;

        if (gano)
        {
            ganancia = apuesta * 2;
            Console.WriteLine($"  🏆  ¡Color correcto! Ganancia: {ganancia} Tokens");
        }
        else
        {
            Console.WriteLine("  💨  Color incorrecto. ¡La rueda no mintió!");
        }

        usuario.MaevsTokens += ganancia;
        return ganancia;
    }
}namespace CasinoMaevs.Services;

using CasinoMaevs.Models;

// ─── CLASE ABSTRACTA BASE ────────────────────────────────────────────────────

public abstract class Juego
{
    public string Nombre { get; protected set; }
    public double ApuestaMinima { get; protected set; }

    protected static readonly Random _rng = new Random();

    protected Juego(string nombre, double apuestaMinima)
    {
        Nombre = nombre;
        ApuestaMinima = apuestaMinima;
    }

    // Método abstracto: cada juego define su propia lógica
    // Lanza FondosInsuficientesException si el saldo es insuficiente
    public abstract double Jugar(Usuario usuario, double apuesta);

    // Método protegido reutilizable para validar apuesta antes de cada juego
    protected void ValidarApuesta(Usuario usuario, double apuesta)
    {
        if (apuesta < ApuestaMinima)
            throw new ArgumentException($"La apuesta mínima para {Nombre} es {ApuestaMinima} MAEVS TOKENS.");

        if (usuario.MaevsTokens < apuesta)
            throw new FondosInsuficientesException(usuario.MaevsTokens, apuesta);
    }
}

// ─── JUEGO 1: SLOTS ──────────────────────────────────────────────────────────

public class SlotsJuego : Juego
{
    private static readonly string[] _simbolos = { "🍒", "🍋", "🔔", "⭐", "💎", "7️⃣" };

    public SlotsJuego() : base("Slots Fortuna", 10) { }

    public override double Jugar(Usuario usuario, double apuesta)
    {
        ValidarApuesta(usuario, apuesta);
        usuario.MaevsTokens -= apuesta;

        string s1 = _simbolos[_rng.Next(_simbolos.Length)];
        string s2 = _simbolos[_rng.Next(_simbolos.Length)];
        string s3 = _simbolos[_rng.Next(_simbolos.Length)];

        Console.WriteLine($"  🎰  Resultado: [ {s1} | {s2} | {s3} ]");

        double ganancia = 0;

        if (s1 == s2 && s2 == s3)
        {
            ganancia = apuesta * 5;
            Console.WriteLine($"  🏆  ¡JACKPOT! Los 3 símbolos coinciden. Ganancia: {ganancia} Tokens");
        }
        else if (s1 == s2 || s2 == s3 || s1 == s3)
        {
            ganancia = apuesta * 1.5;
            Console.WriteLine($"  ✨  ¡2 símbolos coinciden! Ganancia: {ganancia} Tokens");
        }
        else
        {
            Console.WriteLine("  💨  Sin coincidencias. Mejor suerte la próxima.");
        }

        usuario.MaevsTokens += ganancia;
        return ganancia;
    }
}

// ─── JUEGO 2: BLACKJACK SIMPLIFICADO ─────────────────────────────────────────

public class BlackjackSimplificado : Juego
{
    public BlackjackSimplificado() : base("Blackjack Simplificado", 20) { }

    public override double Jugar(Usuario usuario, double apuesta)
    {
        ValidarApuesta(usuario, apuesta);
        usuario.MaevsTokens -= apuesta;

        int puntajeJugador = _rng.Next(12, 22); // 12 a 21 inclusive
        int puntajeCasa    = _rng.Next(12, 22);

        Console.WriteLine($"  🃏  Jugador: {puntajeJugador} | Casa: {puntajeCasa}");

        double ganancia = 0;

        if (puntajeJugador > puntajeCasa)
        {
            ganancia = apuesta * 2;
            Console.WriteLine($"  🏆  ¡Ganaste! Tu puntaje supera a la casa. Ganancia: {ganancia} Tokens");
        }
        else if (puntajeJugador == puntajeCasa)
        {
            ganancia = apuesta; // Empate: devuelve la apuesta
            Console.WriteLine($"  🤝  Empate. Se devuelve la apuesta: {ganancia} Tokens");
        }
        else
        {
            Console.WriteLine("  💔  La casa gana. Sigue intentando.");
        }

        usuario.MaevsTokens += ganancia;
        return ganancia;
    }
}

// ─── JUEGO 3: RULETA DE SUERTE ────────────────────────────────────────────────

public class RuletaSuerte : Juego
{
    public RuletaSuerte() : base("Ruleta de Suerte", 15) { }

    public override double Jugar(Usuario usuario, double apuesta)
    {
        ValidarApuesta(usuario, apuesta);
        usuario.MaevsTokens -= apuesta;

        // 45% de probabilidad de ganar al color
        bool gano = _rng.NextDouble() < 0.45;
        string[] colores = { "🔴 Rojo", "⚫ Negro" };
        string colorJugador = colores[_rng.Next(2)];
        string colorResultado = colores[_rng.Next(2)];

        Console.WriteLine($"  🎡  Apostaste a: {colorJugador} | Resultado: {colorResultado}");

        double ganancia = 0;

        if (gano)
        {
            ganancia = apuesta * 2;
            Console.WriteLine($"  🏆  ¡Color correcto! Ganancia: {ganancia} Tokens");
        }
        else
        {
            Console.WriteLine("  💨  Color incorrecto. ¡La rueda no mintió!");
        }

        usuario.MaevsTokens += ganancia;
        return ganancia;
    }
}
