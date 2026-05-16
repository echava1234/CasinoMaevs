namespace CasinoMaevs.Models;

public class Avatar
{
    public string EstiloBase { get; set; }
    public List<ItemEstetico> ItemsEquipados { get; private set; }

    // Propiedad calculada con expresión flecha (=>) usando LINQ .Sum
    // Suma los multiplicadores de todos los ítems equipados para bonus pasivo
    public double MultiplicadorTotal => ItemsEquipados.Sum(item => item.Multiplicador);

    public Avatar(string estiloBase)
    {
        EstiloBase = estiloBase;
        ItemsEquipados = new List<ItemEstetico>();
    }

    // Usa LINQ .Any para evitar equipar el mismo ítem dos veces
    public bool EquiparItem(ItemEstetico item)
    {
        bool yaEquipado = ItemsEquipados.Any(i => i.Nombre == item.Nombre);

        if (yaEquipado)
        {
            Console.WriteLine($"  ⚠️  El ítem '{item.Nombre}' ya está equipado en el avatar.");
            return false;
        }

        ItemsEquipados.Add(item);
        Console.WriteLine($"  ✅  Ítem '{item.Nombre}' equipado exitosamente.");
        return true;
    }
}
