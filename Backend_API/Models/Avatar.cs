namespace CasinoMaevs.Models;

public class Avatar
{
    public string EstiloBase { get; set; }
    public List<ItemEstetico> ItemsEquipados { get; private set; }
    
    // 🔧 NUEVO: Hardware para mejorar multiplicador
    public List<ComponenteHardware> ComponentesHardware { get; private set; }
    
    // 🧪 NUEVO: Items consumibles activos
    public List<ItemConsumible> Inventario { get; private set; }

    // Propiedad calculada: suma todos los multiplicadores (ítems + hardware)
    public double MultiplicadorTotal => 
        ItemsEquipados.Sum(item => item.Multiplicador) + 
        ComponentesHardware.Sum(comp => comp.BonusMultiplicador);

    public Avatar(string estiloBase)
    {
        EstiloBase = estiloBase;
        ItemsEquipados = new List<ItemEstetico>();
        ComponentesHardware = new List<ComponenteHardware>();
        Inventario = new List<ItemConsumible>();
        
        // Inicializa 3 componentes de hardware predeterminados
        InicializarComponentes();
    }

    /// <summary>
    /// Inicializa los componentes de hardware disponibles.
    /// </summary>
    private void InicializarComponentes()
    {
        ComponentesHardware.Add(new ComponenteHardware("GPU Cuántica", "Tarjeta de video avanzada", 150));
        ComponentesHardware.Add(new ComponenteHardware("CPU Líquida", "Procesador enfriado con Neón", 200));
        ComponentesHardware.Add(new ComponenteHardware("Memoria Flotante", "RAM sincronizada con la red", 100));
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

    /// <summary>
    /// Mejora un componente de hardware específico por índice.
    /// </summary>
    public bool MejorarComponente(int indice, Usuario usuario)
    {
        if (indice < 0 || indice >= ComponentesHardware.Count)
            return false;

        var componente = ComponentesHardware[indice];
        double costoProxima = componente.CalcularCostoProxima();

        if (usuario.MaevsTokens < costoProxima)
            return false;

        usuario.MaevsTokens -= costoProxima;
        componente.MejorarNivel();
        return true;
    }

    /// <summary>
    /// Añade un ítem consumible al inventario o suma cantidad si ya existe.
    /// </summary>
    public void AñadirConsumible(ItemConsumible item)
    {
        var existente = Inventario.FirstOrDefault(i => i.Nombre == item.Nombre);
        
        if (existente != null)
            existente.Añadir(item.Cantidad);
        else
            Inventario.Add(item);
    }
}
