namespace CasinoMaevs.Models;

public class Usuario
{
    // Campos privados para encapsulamiento
    private string _username;
    private double _maevsTokens;

    // Propiedad con validación: Username no puede ser nulo ni vacío
    public string Username
    {
        get => _username;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El nombre de usuario no puede ser nulo ni estar vacío.");
            _username = value;
        }
    }

    // Propiedad con validación: MaevsTokens nunca puede ser negativo
    public double MaevsTokens
    {
        get => _maevsTokens;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(MaevsTokens),
                    "El saldo de MAEVS TOKENS no puede ser negativo. Operación rechazada.");
            _maevsTokens = value;
        }
    }

    public Avatar Avatar { get; set; }

    public Usuario(string username, string estiloAvatar = "Clásico")
    {
        Username = username;
        MaevsTokens = 0;
        Avatar = new Avatar(estiloAvatar);
    }
}
