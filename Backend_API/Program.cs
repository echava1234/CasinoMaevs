using CasinoMaevs.Controllers;
using CasinoMaevs.Models;
using CasinoMaevs.Services;
using System.Text.Json;

class Program
{
    // ─── INSTANCIAS DE RESPALDO (TUS MISMOS OBJETOS DEL BACKEND) ──────────────────
    private static PayPalService paypalService = new PayPalService(
        mode:           "sandbox",
        clientId:       "AYSq3RDGsmBLJE-otTkBtM-jBRd1TCQwFf9RGfwddNXWz0uFU9ztymylOuICfZjL",
        clientSecret:   "EGnHDxD_qRPdaLdZz8iCr8N7_MnF7UGpaXplBqk_ANpJo47-FjwWVImRW15-4PCi",
        tasaConversion: 100
    );

    private static CasinoController casino = new CasinoController();
    private static SlotsJuego slots = new SlotsJuego();
    private static BlackjackSimplificado blackjack = new BlackjackSimplificado();
    private static RuletaSuerte ruleta = new RuletaSuerte();

    // Creamos un usuario global en memoria para simular las partidas en la web local
    private static Usuario usuario = new Usuario("MaevsPlayer01", "Guerrero Galáctico");
    
    // Guardamos temporalmente las últimas cajas generadas para poder abrirlas por ID
    private static List<CajaRecompensa> ultimasCajasGeneradas = new List<CajaRecompensa>();

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        // 🚀 INICIALIZACIÓN DEL SERVIDOR WEB MINIMALISTA (.NET 8)
        var builder = WebApplication.CreateBuilder(args);
        
        // Configuración de CORS obligatoria para que el navegador (HTML) pueda comunicarse con la API local
        builder.Services.AddCors(options => {
            options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        });

        var app = builder.Build();
        app.UseCors();

        Console.WriteLine("╔══════════════════════════════════════╗");
        Console.WriteLine("║    🎰 SERVIDOR CASINO MAEVS ACTIVO   ║");
        Console.WriteLine("║    Escuchando en: http://localhost:5000║");
        Console.WriteLine("╚══════════════════════════════════════╝\n");
        Console.WriteLine($"🎮 Jugador activo en sesión: {usuario.Username}\n");

        // ═══════════════════════════════════════════════════════════════════════════════
        // 🔐 ENDPOINTS DE AUTENTICACIÓN (Delegados a AuthService)
        // ═══════════════════════════════════════════════════════════════════════════════

        // 🌐 ENDPOINT 1: Registrar nuevo usuario
        app.MapPost("/api/auth/registrar", (RegistroRequest req) => {
            try {
                if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Contraseña))
                    return Results.BadRequest(new { mensaje = "Username y contraseña son requeridos." });

                authService.Registrar(req.Username, req.Contraseña, req.EstiloAvatar ?? "Clásico");
                return Results.Ok(new { mensaje = "Usuario registrado exitosamente.", username = req.Username });
            } catch (Exception ex) {
                return Results.BadRequest(new { mensaje = ex.Message });
            }
        });

        // 🌐 ENDPOINT 2: Login
        app.MapPost("/api/auth/login", (LoginRequest req) => {
            try {
                authService.Login(req.Username, req.Contraseña);
                Usuario usuarioActual = authService.ObtenerUsuarioAutenticado();
                return Results.Ok(new { 
                    mensaje = "Login exitoso", 
                    username = usuarioActual.Username,
                    tokens = usuarioActual.MaevsTokens
                });
            } catch (Exception ex) {
                return Results.Unauthorized();
            }
        });

        // 🌐 ENDPOINT 3: Verificar estado de autenticación
        app.MapGet("/api/auth/estado", () => {
            if (!authService.EstaAutenticado())
                return Results.Unauthorized();

            Usuario usuario = authService.ObtenerUsuarioAutenticado();
            return Results.Ok(new { 
                autenticado = true, 
                username = usuario.Username 
            });
        });

        // 🌐 ENDPOINT 4: Logout
        app.MapPost("/api/auth/logout", () => {
            authService.Logout();
            return Results.Ok(new { mensaje = "Sesión cerrada exitosamente." });
        });

        // 🌐 ENDPOINT 5: Cambiar contraseña
        app.MapPost("/api/auth/cambiar-contraseña", (CambiarContraseñaRequest req) => {
            try {
                if (!authService.EstaAutenticado())
                    return Results.Unauthorized();

                authService.CambiarContraseña(req.ContraseñaAntigua, req.ContraseñaNueva);
                return Results.Ok(new { mensaje = "Contraseña cambiada exitosamente." });
            } catch (Exception ex) {
                return Results.BadRequest(new { mensaje = ex.Message });
            }
        });

        // ═══════════════════════════════════════════════════════════════════════════════
        // 🎮 ENDPOINTS DE JUEGO (Requieren autenticación)
        // ═══════════════════════════════════════════════════════════════════════════════

        // 🌐 ENDPOINT 1: Obtener el estado del jugador en tiempo real
        app.MapGet("/api/usuario", () => Results.Ok(new {
            username = usuario.Username,
            tokens = usuario.MaevsTokens,
            avatarEstilo = usuario.Avatar.EstiloBase,
            multiplicador = usuario.Avatar.MultiplicadorTotal,
            itemsEquipados = usuario.Avatar.ItemsEquipados.Select(i => i.ToString()).ToList()
        }));

        // 🌐 ENDPOINT 2: Procesar recarga simulada de PayPal
        app.MapPost("/api/paypal/recargar", (RecargaRequest req) => {
            bool exito = paypalService.ProcesarRecarga(usuario, req.MontoUsd);
            if (exito)
                return Results.Ok(new { mensaje = "Recarga exitosa", saldoActual = usuario.MaevsTokens });
            return Results.BadRequest(new { mensaje = "Error al procesar el pago" });
        });

        // 🌐 ENDPOINT 3: Jugar a los Slots (Tragamonedas)
        app.MapPost("/api/jugar/slots", (ApuestaRequest req) => {
            try {
                double saldoAntes = usuario.MaevsTokens;
                double ganado = slots.Jugar(usuario, req.Apuesta);
                return Results.Ok(new { exito = true, juego = "Slots", saldoAntes, ganado, saldoActual = usuario.MaevsTokens });
            } catch (Exception ex) {
                return Results.BadRequest(new { exito = false, mensaje = ex.Message });
            }
        });

        // 🌐 ENDPOINT 4: Generar el Pool de 6 Cajas Misteriosas
        app.MapGet("/api/cajas/generar", () => {
            ultimasCajasGeneradas = casino.Generar6Cajas();
            // Retorna las cajas estructuradas para que JavaScript las dibuje en pantalla
            return Results.Ok(ultimasCajasGeneradas.Select(c => new { c.Id, c.FueAbierta, desc = c.Descripcion }));
        });

        // 🌐 ENDPOINT 5: Abrir las 3 cajas seleccionadas por el usuario desde el navegador
        app.MapPost("/api/cajas/abrir", (AbrirCajasRequest req) => {
            try {
                if (ultimasCajasGeneradas.Count == 0)
                    return Results.BadRequest(new { mensaje = "Primero debes generar las cajas." });

                double saldoAntes = usuario.MaevsTokens;
                // Ejecuta tu lógica real filtrada con LINQ
                casino.SeleccionarYAbrirCajas(usuario, ultimasCajasGeneradas, req.IdsSeleccionados);

                return Results.Ok(new { 
                    exito = true, 
                    saldoActual = usuario.MaevsTokens,
                    itemsTotales = usuario.Avatar.ItemsEquipados.Select(i => i.ToString()).ToList(),
                    multiplicadorFinal = usuario.Avatar.MultiplicadorTotal
                });
            } catch (Exception ex) {
                return Results.BadRequest(new { mensaje = ex.Message });
            }
        });

        // Mantiene el hilo de ejecución corriendo de fondo de manera asíncrona
        await app.RunAsync();
    }
}

// 📦 CLASES DTO (Data Transfer Objects) para mapear el JSON que envía el JavaScript del HTML
public class RecargaRequest { public double MontoUsd { get; set; } }
public class ApuestaRequest { public double Apuesta { get; set; } }
public class AbrirCajasRequest { public List<int> IdsSeleccionados { get; set; } }
