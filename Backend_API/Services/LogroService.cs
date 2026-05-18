namespace CasinoMaevs.Services;

using CasinoMaevs.Models;

/// <summary>
/// Gestiona los logros y hitos desbloqueables del usuario.
/// Usa LINQ para filtrar y contar progreso.
/// </summary>
public class LogroService
{
    public class Logro
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Icono { get; set; }
        public bool Desbloqueado { get; set; } = false;
        public DateTime FechaDesbloq { get; set; }

        public Logro(string id, string nombre, string descripcion, string icono)
        {
            Id = id;
            Nombre = nombre;
            Descripcion = descripcion;
            Icono = icono;
        }
    }

    private List<Logro> _logros = new List<Logro>
    {
        new Logro("principiante", "Suerte de Principiante", "Gana tu primer juego", "🍀"),
        new Logro("minero", "Minero de Datos", "Juega 10 veces seguidas", "⛏️"),
        new Logro("cazador_bugs", "Cazador de Bugs", "Intenta apostar 0 tokens", "🐛"),
        new Logro("hacker_elite", "Hacker de Élite", "Alcanza 5.0x de multiplicador", "🎩"),
        new Logro("multimillonario", "Multimillonario", "Acumula 1,000,000 MAEVS", "💰"),
    };

    /// <summary>
    /// Desbloquea un logro específico.
    /// </summary>
    public bool DesbloquearLogro(string idLogro)
    {
        var logro = _logros.FirstOrDefault(l => l.Id == idLogro);
        if (logro == null || logro.Desbloqueado)
            return false;

        logro.Desbloqueado = true;
        logro.FechaDesbloq = DateTime.UtcNow;

        Console.WriteLine($"🏆 ¡LOGRO DESBLOQUEADO! {logro.Icono} {logro.Nombre}");
        return true;
    }

    /// <summary>
    /// Obtiene todos los logros desbloqueados (usando LINQ).
    /// </summary>
    public List<Logro> ObtenerLogrosDesbloqueados()
    {
        return _logros.Where(l => l.Desbloqueado).ToList();
    }

    /// <summary>
    /// Calcula el progreso total de logros (porcentaje).
    /// </summary>
    public double ObtenerPorcentajeProgreso()
    {
        int desbloqueados = _logros.Count(l => l.Desbloqueado);
        return (desbloqueados / (double)_logros.Count) * 100;
    }
}
