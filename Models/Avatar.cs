namespace CasinoMaevs.Models;

public enum Rareza
{
    Comun,
    Raro,
    Epico,
    Legendario
}

public class ItemEstetico
{
    public string Nombre { get; set; }
    public Rareza Rareza { get; set; }
    public double Multiplicador { get; set; }

    public ItemEstetico(string nombre, Rareza rareza, double multiplicador)
    {
        Nombre = nombre;
        Rareza = rareza;
        Multiplicador = multiplicador;
    }

    public override string ToString()
        => $"[{Rareza}] {Nombre} (x{Multiplicador} multiplicador)";
}
