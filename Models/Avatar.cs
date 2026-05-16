namespace CasinoMaevs.Models;

public class Avatar
{
    public string Estilo { get; set; }

    public Avatar(string estilo = "Clásico")
    {
        Estilo = estilo;
    }

    public override string ToString()
        => $"Avatar ({Estilo})";
}
