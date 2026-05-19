namespace CasinoMaevs.Services;

using CasinoMaevs.Models;

/// <summary>
/// Implementa el "Modo Sobrecarga" - Mecánica de riesgo extremo.
/// La barra de energía se llena con cada juego y puede activarse para x3 multiplicador.
/// </summary>
public class OverloadMode
{
    private Dictionary<string, double> _energiaUsuarios = new Dictionary<string, double>();
    private const double ENERGIA_MAXIMA = 100.0;
    private const double ENERGIA_POR_JUEGO = 20.0;

    /// <summary>
    /// Añade energía al usuario con cada juego jugado.
    /// </summary>
    public void AñadirEnergia(string username, double cantidad = ENERGIA_POR_JUEGO)
    {
        if (!_energiaUsuarios.ContainsKey(username))
            _energiaUsuarios[username] = 0;

        _energiaUsuarios[username] = Math.Min(ENERGIA_MAXIMA, _energiaUsuarios[username] + cantidad);
    }

    /// <summary>
    /// Verifica si el usuario puede activar Overload Mode (energía >= 100).
    /// </summary>
    public bool PuedeActivarOverload(string username)
    {
        if (!_energiaUsuarios.TryGetValue(username, out var energia))
            return false;

        return energia >= ENERGIA_MAXIMA;
    }

    /// <summary>
    /// Activa el modo Overload y devuelve el multiplicador especial.
    /// Consume toda la energía y penaliza si pierde.
    /// </summary>
    public (bool activado, double multiplicador) ActivarOverload(string username)
    {
        if (!PuedeActivarOverload(username))
            return (false, 1.0);

        _energiaUsuarios[username] = 0;
        Console.WriteLine($"🔴 ¡OVERLOAD MODE ACTIVADO! x3 multiplicador por 5 tiros.");

        return (true, 3.0);
    }

    /// <summary>
    /// Obtiene el nivel de energía del usuario.
    /// </summary>
    public double ObtenerEnergia(string username)
    {
        _energiaUsuarios.TryGetValue(username, out var energia);
        return energia;
    }

    /// <summary>
    /// Penaliza al usuario que pierde en Overload (pierde un ítem).
    /// </summary>
    public void AplicarPenalizaOverload(Usuario usuario)
    {
        if (usuario.Avatar.ItemsEquipados.Count > 0)
        {
            var itemPerdido = usuario.Avatar.ItemsEquipados.Last();
            usuario.Avatar.ItemsEquipados.RemoveAt(usuario.Avatar.ItemsEquipados.Count - 1);
            Console.WriteLine($"💔 Perdiste: {itemPerdido.Nombre}");
        }
    }
}
