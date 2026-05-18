namespace CasinoMaevs.Models;

/// <summary>
/// Representa un ítem consumible que el usuario puede activar antes de jugar.
/// Los ítems se usan una sola vez y afectan el resultado del próximo juego.
/// </summary>
public class ItemConsumible
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Efecto { get; set; } // Descripción del efecto
    public int Cantidad { get; private set; }
    public DateTime FechaAdquisicion { get; set; }

    public ItemConsumible(string nombre, string efecto, int cantidad = 1)
    {
        Nombre = nombre;
        Efecto = efecto;
        Cantidad = cantidad;
        FechaAdquisicion = DateTime.UtcNow;
    }

    /// <summary>
    /// Consume un item. Usa validación para evitar consumir más de lo que hay.
    /// </summary>
    public bool Consumir()
    {
        if (Cantidad <= 0)
            return false;
        Cantidad--;
        return true;
    }

    public void Añadir(int cantidad = 1)
    {
        if (cantidad > 0)
            Cantidad += cantidad;
    }

    public override string ToString()
    {
        return $"🧪 {Nombre} x{Cantidad} - {Efecto}";
    }
}
