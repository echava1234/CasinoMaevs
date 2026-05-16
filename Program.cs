using CasinoMaevs.Controllers;
using CasinoMaevs.Models;
using CasinoMaevs.Services;

// ════════════════════════════════════════════════════════════════════
//   CASINO MAEVS TOKENS — Orquestador Principal
// ════════════════════════════════════════════════════════════════════

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("╔══════════════════════════════════════╗");
Console.WriteLine("║       CASINO  MAEVS  TOKENS          ║");
Console.WriteLine("╚══════════════════════════════════════╝\n");

// ─── PASO 1: Instanciar servicios y controlador ──────────────────────────────

PayPalService paypalService = new PayPalService(
    mode:           "sandbox",
    clientId:       "AYSq3RDGsmBLJE-otTkBtM-jBRd1TCQwFf9RGfwddNXWz0uFU9ztymylOuICfZjL",
    clientSecret:   "EGnHDxD_qRPdaLdZz8iCr8N7_MnF7UGpaXplBqk_ANpJo47-FjwWVImRW15-4PCi",
    tasaConversion: 100
);

List<Juego> juegos = new List<Juego>
{
    new SlotsJuego(),
    new BlackjackSimplificado(),
    new RuletaSuerte()
};

CasinoController casino = new CasinoController();

// ─── PASO 2: Crear usuario con saldo 0 ───────────────────────────────────────

Usuario usuario = new Usuario("MaevsPlayer01", "Guerrero Galáctico");
Console.WriteLine($"👤  Usuario creado: {usuario.Username}");
Console.WriteLine($"    Saldo inicial: {usuario.MaevsTokens} MAEVS TOKENS\n");

// ─── PASO 3: Recarga exitosa de $50 USD vía PayPal ───────────────────────────

Console.WriteLine("═══ RECARGA VÍA PAYPAL ══════════════════════════════");
paypalService.ProcesarRecarga(usuario, 50);

// ─── PASO 4: Rondas de juego con manejo de FondosInsuficientesException ──────

Console.WriteLine("═══ RONDAS DE JUEGO ═════════════════════════════════");

foreach (Juego juego in juegos)
{
    Console.WriteLine($"\n▶  {juego.Nombre} (mín. {juego.ApuestaMinima} Tokens)");
    Console.WriteLine($"   Saldo antes: {usuario.MaevsTokens} Tokens");

    try
    {
        double apuesta = juego.ApuestaMinima * 2;
        double resultado = juego.Jugar(usuario, apuesta);
        Console.WriteLine($"   Saldo después: {usuario.MaevsTokens} Tokens");
    }
    catch (FondosInsuficientesException ex)
    {
        Console.WriteLine($"  🚫  {ex.Message}");
        Console.WriteLine($"      Saldo actual sin cambios: {usuario.MaevsTokens} Tokens");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ❌  Error inesperado en juego: {ex.Message}");
    }
    finally
    {
        Console.WriteLine($"   --- Ronda de {juego.Nombre} finalizada ---");
    }
}

// ─── PASO 5: Simular victoria que activa cajas de recompensa ─────────────────

Console.WriteLine("\n═══ CAJAS DE RECOMPENSA ═════════════════════════════");
Console.WriteLine("🎉  ¡Victoria especial detectada! Se generan 6 cajas misterio...");

List<CajaRecompensa> cajas = casino.Generar6Cajas();

// ─── PASO 6: LINQ para que el usuario elija exactamente 3 cajas por ID ───────

// Simula que el usuario elige las cajas 1, 3 y 5
List<int> idsCajasElegidas = cajas
    .Select(c => c.Id)
    .Where(id => id % 2 != 0) // Elige los IDs impares: 1, 3, 5
    .Take(3)
    .ToList();

casino.SeleccionarYAbrirCajas(usuario, cajas, idsCajasElegidas);

// ─── PASO 7: Estado final detallado ──────────────────────────────────────────

Console.WriteLine("\n╔══════════════════════════════════════╗");
Console.WriteLine("  ║         ESTADO FINAL DEL JUGADOR     ║");
Console.WriteLine("  ╚══════════════════════════════════════╝");

Console.WriteLine($"\n👤  Jugador    : {usuario.Username}");
Console.WriteLine($"🎨  Avatar     : {usuario.Avatar.EstiloBase}");
Console.WriteLine($"💰  Saldo final: {usuario.MaevsTokens} MAEVS TOKENS");

// LINQ .OrderBy: muestra el inventario ordenado por rareza (enum ascendente = Común → Legendario)
List<ItemEstetico> inventarioOrdenado = usuario.Avatar.ItemsEquipados
    .OrderByDescending(item => item.Rareza) // Legendario primero
    .ToList();

Console.WriteLine($"\n🎒  Inventario del Avatar ({inventarioOrdenado.Count} ítem(s) equipado(s)):");

if (inventarioOrdenado.Count == 0)
{
    Console.WriteLine("    (Sin ítems equipados)");
}
else
{
    inventarioOrdenado.ForEach(item =>
        Console.WriteLine($"    • {item}")
    );
}

// Propiedad calculada MultiplicadorTotal recalculada automáticamente
Console.WriteLine($"\n⚡  MultiplicadorTotal del Avatar: x{usuario.Avatar.MultiplicadorTotal:F2}");
Console.WriteLine("    (Bonus pasivo calculado con LINQ .Sum sobre ítems equipados)\n");

Console.WriteLine("════════════════════════════════════════");
Console.WriteLine("  Gracias por jugar en Casino MAEVS TOKENS");
Console.WriteLine("════════════════════════════════════════");
