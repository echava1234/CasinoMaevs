namespace CasinoMaevs.Models;

/// <summary>
/// Representa un componente de hardware que el usuario puede mejorar
/// para aumentar su multiplicador. Cada componente tiene un nivel, costo y bonus.
/// </summary>
public class ComponenteHardware
{
    // Propiedades automáticas con encapsulamiento
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public int Nivel { get; private set; }
    public double BonusMultiplicador { get; private set; } // Bonus por nivel
    public double CostoPorNivel { get; set; } // Costo en MAEVS para subir de nivel
    public int NivelMaximo { get; set; } = 10;

    public ComponenteHardware(string nombre, string descripcion, double costoBase)
    {
        Nombre = nombre;
        Descripcion = descripcion;
        Nivel = 1;
        BonusMultiplicador = 0.05; // 5% de bonus inicial
        CostoPorNivel = costoBase;
    }

    /// <summary>
    /// Mejora el componente al siguiente nivel.
    /// Usa una validación por propiedad para evitar superar el nivel máximo.
    /// </summary>
    public bool MejorarNivel()
    {
        if (Nivel >= NivelMaximo)
            return false;

        Nivel++;
        BonusMultiplicador += 0.05; // Suma 5% adicional por cada nivel
        return true;
    }

    /// <summary>
    /// Calcula el costo total para mejorar al siguiente nivel.
    /// Usa una función con escalado exponencial.
    /// </summary>
    public double CalcularCostoProxima()
    {
        if (Nivel >= NivelMaximo)
            return double.MaxValue;
        
        // Costo escalado: costo_base * (1.2 ^ nivel)
        return CostoPorNivel * Math.Pow(1.2, Nivel - 1);
    }

    public override string ToString()
    {
        return $"🔧 {Nombre} (Nivel {Nivel}/{NivelMaximo}) - Bonus: +{(BonusMultiplicador * 100):F1}%";
    }
}
