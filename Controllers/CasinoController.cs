namespace CasinoMaevs.Controllers;

using CasinoMaevs.Models;
using CasinoMaevs.Services;

public class CasinoController
{
    private static readonly Random _rng = new Random();

    // Pool estático de ítems estéticos con distintas rarezas
    private static readonly List<ItemEstetico> _lootPool = new List<ItemEstetico>
    {
        new ItemEstetico("Corona de Luz",        Rareza.Legendario, 2.50),
        new ItemEstetico("Capa del Vacío",        Rareza.Epico,      1.80),
        new ItemEstetico("Escudo Estelar",        Rareza.Epico,      1.60),
        new ItemEstetico("Guantelete de Cristal", Rareza.Raro,       1.30),
        new ItemEstetico("Botas de Mercurio",     Rareza.Raro,       1.20),
        new ItemEstetico("Amuleto Común",         Rareza.Comun,      1.05),
        new ItemEstetico("Antorcha de Bronce",    Rareza.Comun,      1.02),
        new ItemEstetico("Máscara de Neón",       Rareza.Legendario, 2.80),
    };

    // Genera exactamente 6 cajas con premios aleatorios del pool
    public List<CajaRecompensa> Generar6Cajas()
    {
        List<CajaRecompensa> cajas = new List<CajaRecompensa>();

        for (int i = 1; i <= 6; i++)
        {
            bool esToken = _rng.NextDouble() < 0.5; // 50% tokens, 50% cosmético

            if (esToken)
            {
                double cantidadTokens = _rng.Next(50, 501); // Entre 50 y 500 tokens
                cajas.Add(new CajaRecompensa(i, "Tokens", cantidadTokens));
            }
            else
            {
                ItemEstetico itemAleatorio = _lootPool[_rng.Next(_lootPool.Count)];
                cajas.Add(new CajaRecompensa(i, "Cosmético", 0, itemAleatorio));
            }
        }

        Console.WriteLine("\n  📦  Se han generado 6 cajas de recompensa misterio:");
        cajas.ForEach(c => Console.WriteLine($"      {c.Descripcion}"));

        return cajas;
    }

    // Filtra las 3 cajas seleccionadas por ID y las procesa para el usuario
    public void SeleccionarYAbrirCajas(Usuario usuario, List<CajaRecompensa> cajas, List<int> idsSeleccionados)
    {
        Console.WriteLine($"\n  🎁  {usuario.Username} selecciona las cajas: #{string.Join(", #", idsSeleccionados)}");

        try
        {
            // LINQ .Where: filtra estrictamente los 3 IDs elegidos por el usuario
            List<CajaRecompensa> cajasSeleccionadas = cajas
                .Where(c => idsSeleccionados.Contains(c.Id))
                .Take(3)
                .ToList();

            if (cajasSeleccionadas.Count == 0)
                throw new InvalidOperationException("No se encontraron cajas con los IDs proporcionados.");

            // Lambda para procesar cada caja seleccionada
            cajasSeleccionadas.ForEach(caja =>
            {
                caja.FueAbierta = true;
                Console.WriteLine($"\n  🔓  Abriendo {caja.Descripcion}");

                if (caja.TipoPremio == "Tokens")
                {
                    usuario.MaevsTokens += caja.CantidadTokens;
                    Console.WriteLine($"      💰  +{caja.CantidadTokens} MAEVS TOKENS añadidos al saldo.");
                }
                else if (caja.TipoPremio == "Cosmético" && caja.Item != null)
                {
                    usuario.Avatar.EquiparItem(caja.Item);
                }
            });

            // LINQ .Sum: suma total de tokens ganados en las cajas abiertas
            double totalTokensGanados = cajasSeleccionadas
                .Where(c => c.TipoPremio == "Tokens")
                .Sum(c => c.CantidadTokens);

            Console.WriteLine($"\n  📊  Tokens ganados en cajas: {totalTokensGanados} MAEVS TOKENS");
        }
        catch (FondosInsuficientesException ex)
        {
            Console.WriteLine($"  ❌  Error de fondos: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"  ❌  Error al abrir cajas: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌  Error inesperado: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("  🔒  Proceso de apertura de cajas finalizado.");
        }
    }
}
