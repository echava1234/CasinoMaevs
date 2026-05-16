namespace CasinoMaevs.Models;

public class CajaRecompensa
{
    public int Id { get; set; }

    // TipoPremio solo acepta "Tokens" o "Cosmético"
    private string _tipoPremio;
    public string TipoPremio
    {
        get => _tipoPremio;
        set
        {
            if (value != "Tokens" && value != "Cosmético")
                throw new ArgumentException("TipoPremio debe ser 'Tokens' o 'Cosmético'.");
            _tipoPremio = value;
        }
    }

    public double CantidadTokens { get; set; }

    // Nullable: solo tiene valor si TipoPremio es "Cosmético"
    public ItemEstetico? Item { get; set; }

    public bool FueAbierta { get; set; }

    // Propiedad calculada: descripción resumida de la caja
    public string Descripcion => FueAbierta
        ? $"Caja #{Id} [ABIERTA] → {(TipoPremio == "Tokens" ? $"{CantidadTokens} Tokens" : Item?.Nombre ?? "Ítem desconocido")}"
        : $"Caja #{Id} [CERRADA] → ???";

    public CajaRecompensa(int id, string tipoPremio, double cantidadTokens = 0, ItemEstetico? item = null)
    {
        Id = id;
        TipoPremio = tipoPremio;
        CantidadTokens = cantidadTokens;
        Item = item;
        FueAbierta = false;
    }
}
