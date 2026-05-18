namespace CasinoMaevs.Models;

/// <summary>
/// Representa una distorsión temporal en la red del casino.
/// Afecta probabilidades de ganancias y costos de apuestas.
/// </summary>
public class DistorsionRed
{
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public double BonusGanancia { get; set; } // % adicional de premio (ej: 0.20 = 20% bonus)
    public double MultiplicadorApuesta { get; set; } // Costo de apuestas (ej: 2.0 = cuesta el doble)
    public DateTime FechaInicio { get; set; }
    public int DuracionSegundos { get; set; }
    public bool EstaActiva => (DateTime.UtcNow - FechaInicio).TotalSeconds < DuracionSegundos;

    public DistorsionRed(string nombre, string descripcion, double bonusGanancia, 
                        double multiplicadorApuesta, int duracionSegundos = 300)
    {
        Nombre = nombre;
        Descripcion = descripcion;
        BonusGanancia = bonusGanancia;
        MultiplicadorApuesta = multiplicadorApuesta;
        DuracionSegundos = duracionSegundos;
        FechaInicio = DateTime.UtcNow;
    }

    public double TiempoRestante()
    {
        double restante = DuracionSegundos - (DateTime.UtcNow - FechaInicio).TotalSeconds;
        return Math.Max(0, restante);
    }

    public override string ToString()
    {
        return $"⚡ {Nombre} - {Descripcion} | +{(BonusGanancia * 100):F0}% bonus | Costo x{MultiplicadorApuesta}";
    }
}
