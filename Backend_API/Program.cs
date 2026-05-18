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

    private static DistorsionService distorsionService = new DistorsionService();
    private static PanicSystem panicSystem = new PanicSystem();
    private static LogroService logroService = new LogroService();
    private static BlackMarketService blackMarket = new BlackMarketService();
    private static OverloadMode overloadMode = new OverloadMode();


    private static CasinoController casino = new CasinoController();
    private static SlotsJuego slots = new SlotsJuego();
    private static BlackjackSimplificado blackjack = new BlackjackSimplificado();
    private static RuletaSuerte ruleta = new RuletaSuerte();

    
    
    private static AuthService authService = new AuthService();

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
            } catch (Exception) { // ✨ Corrección de la advertencia (warning CS0168) al remover la variable 'ex' que no se usaba
                return Results.Unauthorized();
            }
        });

        // 🌐 ENDPOINT 3: Verificar estado de autenticación
        app.MapGet("/api/auth/estado", () => {
            if (!authService.EstaAutenticado())
                return Results.Unauthorized();

            Usuario usuarioAutenticado = authService.ObtenerUsuarioAutenticado();
            return Results.Ok(new { 
                autenticado = true, 
                username = usuarioAutenticado.Username 
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
        app.MapGet("/api/usuario", () => {
            // Modificación inteligente: Si el usuario ya inició sesión mediante authService, usamos ese. Si no, usamos el global por defecto.
            Usuario usuarioActual = authService.EstaAutenticado() ? authService.ObtenerUsuarioAutenticado() : usuario;

            return Results.Ok(new {
                username = usuarioActual.Username,
                tokens = usuarioActual.MaevsTokens,
                avatarEstilo = usuarioActual.Avatar.EstiloBase,
                multiplicador = usuarioActual.Avatar.MultiplicadorTotal,
                itemsEquipados = usuarioActual.Avatar.ItemsEquipados.Select(i => i.ToString()).ToList()
            });
        });

        // 🌐 ENDPOINT 2: Procesar recarga simulada de PayPal
        app.MapPost("/api/paypal/recargar", (RecargaRequest req) => {
            Usuario usuarioActual = authService.EstaAutenticado() ? authService.ObtenerUsuarioAutenticado() : usuario;
            bool exito = paypalService.ProcesarRecarga(usuarioActual, req.MontoUsd);
            if (exito)
                return Results.Ok(new { mensaje = "Recarga exitosa", saldoActual = usuarioActual.MaevsTokens });
            return Results.BadRequest(new { mensaje = "Error al procesar el pago" });
        });

        // 🌐 ENDPOINT 3: Jugar a los Slots (Tragamonedas)
        app.MapPost("/api/jugar/slots", (ApuestaRequest req) => {
            try {
                Usuario usuarioActual = authService.EstaAutenticado() ? authService.ObtenerUsuarioAutenticado() : usuario;
                double saldoAntes = usuarioActual.MaevsTokens;
                double ganado = slots.Jugar(usuarioActual, req.Apuesta);
                return Results.Ok(new { exito = true, juego = "Slots", saldoAntes, ganado, saldoActual = usuarioActual.MaevsTokens });
            } catch (Exception ex) {
                return Results.BadRequest(new { exito = false, mensaje = ex.Message });
            }
        });

        // 🌐 ENDPOINT: Jugar Blackjack
        app.MapPost("/api/jugar/blackjack", (ApuestaRequest req) => {
            try {
                Usuario usuarioActual = authService.EstaAutenticado() ? authService.ObtenerUsuarioAutenticado() : usuario;
                double saldoAntes = usuarioActual.MaevsTokens;
                double ganado = blackjack.Jugar(usuarioActual, req.Apuesta);
                return Results.Ok(new { exito = true, juego = "Blackjack", saldoAntes, ganado, saldoActual = usuarioActual.MaevsTokens });
            } catch (Exception ex) {
                return Results.BadRequest(new { exito = false, mensaje = ex.Message });
            }
        });
        
        // 🌐 ENDPOINT: Jugar Ruleta
        app.MapPost("/api/jugar/ruleta", (ApuestaRequest req) => {
            try {
                Usuario usuarioActual = authService.EstaAutenticado() ? authService.ObtenerUsuarioAutenticado() : usuario;
                double saldoAntes = usuarioActual.MaevsTokens;
                double ganado = ruleta.Jugar(usuarioActual, req.Apuesta);
                return Results.Ok(new { exito = true, juego = "Ruleta", saldoAntes, ganado, saldoActual = usuarioActual.MaevsTokens });
            } catch (Exception ex) {
                return Results.BadRequest(new { exito = false, mensaje = ex.Message });
            }
        });

        // 🌐 ENDPOINT 4: Generar el Pool de 6 Cajas Misteriosas
        app.MapGet("/api/cajas/generar", () => {
            ultimasCajasGeneradas = casino.Generar6Cajas();
            return Results.Ok(ultimasCajasGeneradas.Select(c => new { c.Id, c.FueAbierta, desc = c.Descripcion }));
        });

                    // 🌐 Obtener componentes de hardware disponibles
            app.MapGet("/api/hardware/componentes", () => {
                Usuario usuarioActual = authService.EstaAutenticado() ? authService.ObtenerUsuarioAutenticado() : usuario;
                return Results.Ok(usuarioActual.Avatar.ComponentesHardware.Select(c => new {
                    id = usuarioActual.Avatar.ComponentesHardware.IndexOf(c),
                    nombre = c.Nombre,
                    nivel = c.Nivel,
                    nivelMaximo = c.NivelMaximo,
                    bonus = c.BonusMultiplicador,
                    costoProxima = c.CalcularCostoProxima()
                }));
            });
            
            // 🌐 Mejorar un componente específico
            app.MapPost("/api/hardware/mejorar/{id}", (int id) => {
                try {
                    Usuario usuarioActual = authService.EstaAutenticado() ? authService.ObtenerUsuarioAutenticado() : usuario;
                    
                    if (usuarioActual.Avatar.MejorarComponente(id, usuarioActual))
                    {
                        return Results.Ok(new {
                            exito = true,
                            mensaje = "Componente mejorado",
                            multiplicadorTotal = usuarioActual.Avatar.MultiplicadorTotal,
                            saldoActual = usuarioActual.MaevsTokens
                        });
                    }
                    return Results.BadRequest(new { mensaje = "No hay fondos o componente inválido" });
                } catch (Exception ex) {
                    return Results.BadRequest(new { mensaje = ex.Message });
                }
            });
            
            // ──────────────────────────────────────────────────────────────
            // ⚡ ENDPOINTS DE DISTORSIÓN (Eventos Climáticos)
            // ──────────────────────────────────────────────────────────────
            
            // 🌐 Obtener distorsión activa actual
            app.MapGet("/api/distorsion/actual", () => {
                var distorsion = distorsionService.ObtenerDistorsionActiva();
                if (distorsion == null)
                    return Results.Ok(new { activa = false });
            
                return Results.Ok(new {
                    activa = distorsion.EstaActiva,
                    nombre = distorsion.Nombre,
                    descripcion = distorsion.Descripcion,
                    bonusGanancia = distorsion.BonusGanancia,
                    multiplicadorApuesta = distorsion.MultiplicadorApuesta,
                    tiempoRestante = distorsion.TiempoRestante()
                });
            });
            
            // ──────────────────────────────────────────────────────────────
            // 🚨 ENDPOINTS DE PÁNICO (Emergency Extract)
            // ──────────────────────────────────────────────────────────────
            
            // 🌐 Activar retiro de emergencia
            app.MapPost("/api/panic/extract", (ExtractRequest req) => {
                Usuario usuarioActual = authService.EstaAutenticado() ? authService.ObtenerUsuarioAutenticado() : usuario;
                
                var (exito, mensaje, montoBloqueado) = panicSystem.EmergencyExtract(usuarioActual, req.ApuestaActual);
                
                return Results.Ok(new {
                    exito,
                    mensaje,
                    montoBloqueado,
                    saldoActual = usuarioActual.MaevsTokens,
                    tiempoEnfriamiento = panicSystem.ObtenerTiempoEnfriamientoRestante(usuarioActual)
                });
            });

                // 🌐 ENDPOINT: Verificar si puede abrir cajas hoy
        app.MapGet("/api/cajas/verificar-permiso", () => {
            Usuario usuarioActual = authService.EstaAutenticado() ? authService.ObtenerUsuarioAutenticado() : usuario;
            CajasService cajasService = casino.ObtenerServicioCajas();
            
            var (puedeAbrir, mensaje) = cajasService.VerificarPermisoCajasHoy(usuarioActual);
            
            return Results.Ok(new {
                puedeAbrir,
                mensaje,
                aperturasDiaActual = usuarioActual.AccionesDiarias.AperturasDeHoyGeneradas,
                fecha = DateTime.UtcNow.Date
            });
        });
        
        // 🌐 ENDPOINT: Generar cajas (CON VALIDACIÓN DE PERMISO)
        app.MapGet("/api/cajas/generar", () => {
            Usuario usuarioActual = authService.EstaAutenticado() ? authService.ObtenerUsuarioAutenticado() : usuario;
            CajasService cajasService = casino.ObtenerServicioCajas();
            
            // VALIDACIÓN CRÍTICA EN BACKEND
            var (puedeAbrir, mensaje) = cajasService.VerificarPermisoCajasHoy(usuarioActual);
            if (!puedeAbrir)
                return Results.BadRequest(new { exito = false, mensaje });
            
            // GENERAR PERSONAJE ALEATORIO
            string personajeAleatorio = cajasService.GenerarPersonajeAleatorio();
            
            // Generar cajas
            ultimasCajasGeneradas = casino.Generar6Cajas();
            
            return Results.Ok(new {
                exito = true,
                cajas = ultimasCajasGeneradas.Select(c => new { c.Id, c.FueAbierta, desc = c.Descripcion }),
                personajeAsignado = personajeAleatorio, // El usuario NO elige
                mensaje = $"Te tocó {personajeAleatorio}. ¡Abre las cajas!"
            });
        });
        
        // 🌐 ENDPOINT: Intentar trampa (SOLO SOMBRA)
        app.MapPost("/api/cajas/intentar-trampa", (IntentarTrampaRequest req) => {
            Usuario usuarioActual = authService.EstaAutenticado() ? authService.ObtenerUsuarioAutenticado() : usuario;
            CajasService cajasService = casino.ObtenerServicioCajas();
            
            // Validar tiempo entre intentos (anti-bot)
            if (!cajasService.ValidarTiempoEntreIntentos(usuarioActual))
            {
                return Results.BadRequest(new { 
                    exito = false, 
                    mensaje = "⚠️ Intentas muy rápido. Espera 2 segundos entre intentos."
                });
            }
            
            var (trampaExitosa, mensaje, bonusTokens) = cajasService.IntentarTrampa(usuarioActual, req.PersonajeActual);
            
            if (trampaExitosa)
            {
                usuarioActual.MaevsTokens += bonusTokens;
            }
            
            return Results.Ok(new {
                exito = trampaExitosa,
                mensaje,
                bonusTokens,
                saldoActual = usuarioActual.MaevsTokens,
                intentosRestantes = Math.Max(0, 3 - usuarioActual.AccionesDiarias.IntentosTrampaHoy)
            });
        });
        
        // 🌐 ENDPOINT: Abrir cajas (CON VALIDACIÓN)
        app.MapPost("/api/cajas/abrir", (AbrirCajasRequest req) => {
            try {
                Usuario usuarioActual = authService.EstaAutenticado() ? authService.ObtenerUsuarioAutenticado() : usuario;
                CajasService cajasService = casino.ObtenerServicioCajas();
                
                if (ultimasCajasGeneradas.Count == 0)
                    return Results.BadRequest(new { mensaje = "Primero debes generar las cajas." });
        
                casino.SeleccionarYAbrirCajas(usuarioActual, ultimasCajasGeneradas, req.IdsSeleccionados);
                
                // 🔑 MARCA QUE ABRIÓ CAJAS HOY (¡IMPORTANTE!)
                cajasService.MarcarCajasAbridasHoy(usuarioActual);
        
                return Results.Ok(new { 
                    exito = true, 
                    saldoActual = usuarioActual.MaevsTokens,
                    itemsTotales = usuarioActual.Avatar.ItemsEquipados.Select(i => i.ToString()).ToList(),
                    multiplicadorFinal = usuarioActual.Avatar.MultiplicadorTotal,
                    mensaje = "¡Cajas abiertas! Vuelve mañana para más."
                });
            } catch (Exception ex) {
                return Results.BadRequest(new { mensaje = ex.Message });
            }
        });
            
            // ──────────────────────────────────────────────────────────────
            // 🏆 ENDPOINTS DE LOGROS
            // ──────────────────────────────────────────────────────────────
            
            // 🌐 Obtener todos los logros del usuario
            app.MapGet("/api/logros", () => {
                return Results.Ok(new {
                    logros = logroService.ObtenerLogrosDesbloqueados(),
                    progreso = logroService.ObtenerPorcentajeProgreso()
                });
            });
            
            // 🌐 Desbloquear un logro (endpoint interno para validación)
            app.MapPost("/api/logros/desbloquear/{id}", (string id) => {
                bool desbloqueado = logroService.DesbloquearLogro(id);
                return Results.Ok(new { desbloqueado });
            });
            
            // ──────────────────────────────────────────────────────────────
            // 📈 ENDPOINTS DE MERCADO NEGRO
            // ──────────────────────────────────────────────────────────────
            
            // 🌐 Obtener precio actual del token
            app.MapGet("/api/mercado/precio", () => {
                return Results.Ok(new {
                    precioUSD = blackMarket.PrecioActual,
                    ultimaActualizacion = DateTime.UtcNow
                });
            });
            
            // 🌐 Comprar tokens en el mercado
            app.MapPost("/api/mercado/comprar", (CompraTokensRequest req) => {
                Usuario usuarioActual = authService.EstaAutenticado() ? authService.ObtenerUsuarioAutenticado() : usuario;
                
                double tokensObtenidos = blackMarket.ComprarTokens(req.USD);
                usuarioActual.MaevsTokens += tokensObtenidos;
                
                return Results.Ok(new {
                    usdUsados = req.USD,
                    tokensObtenidos = tokensObtenidos,
                    precioUsado = blackMarket.PrecioActual,
                    saldoActual = usuarioActual.MaevsTokens
                });
            });
            
            // 🌐 Vender tokens en el mercado
            app.MapPost("/api/mercado/vender", (VenderTokensRequest req) => {
                Usuario usuarioActual = authService.EstaAutenticado() ? authService.ObtenerUsuarioAutenticado() : usuario;
                
                if (usuarioActual.MaevsTokens < req.Tokens)
                    return Results.BadRequest(new { mensaje = "No tienes suficientes tokens" });
                
                double usdObtenidos = blackMarket.VenderTokens(req.Tokens);
                usuarioActual.MaevsTokens -= req.Tokens;
                
                return Results.Ok(new {
                    tokensVendidos = req.Tokens,
                    usdObtenidos = usdObtenidos,
                    precioUsado = blackMarket.PrecioActual,
                    saldoTokens = usuarioActual.MaevsTokens
                });
            });
            
            // ──────────────────────────────────────────────────────────────
            // 🔴 ENDPOINTS DE OVERLOAD MODE
            // ──────────────────────────────────────────────────────────────
            
            // 🌐 Obtener nivel de energía
            app.MapGet("/api/overload/energia/{username}", (string username) => {
                double energia = overloadMode.ObtenerEnergia(username);
                bool puedeActivar = overloadMode.PuedeActivarOverload(username);
                
                return Results.Ok(new {
                    energia,
                    energiaMaxima = 100.0,
                    puedeActivar,
                    porcentaje = (energia / 100.0) * 100
                });
            });
            
            // 🌐 Activar modo Overload
            app.MapPost("/api/overload/activar", () => {
                Usuario usuarioActual = authService.EstaAutenticado() ? authService.ObtenerUsuarioAutenticado() : usuario;
                
                var (activado, multiplicador) = overloadMode.ActivarOverload(usuarioActual.Username);
                
                return Results.Ok(new {
                    activado,
                    multiplicador,
                    tirosRestantes = activado ? 5 : 0,
                    mensaje = activado ? "¡OVERLOAD MODE ACTIVADO! x3 por 5 tiros" : "Necesitas 100 de energía"
                });
            });
            
            // Iniciar servicios en segundo plano
            distorsionService.IniciarServicio(intervaloSegundos: 300); // Cada 5 minutos
            blackMarket.IniciarMercado(intervaloSegundos: 30);        // Cada 30 segundos

        // 🌐 ENDPOINT 5: Abrir las 3 cajas seleccionadas por el usuario desde el navegador
        app.MapPost("/api/cajas/abrir", (AbrirCajasRequest req) => {
            try {
                if (ultimasCajasGeneradas.Count == 0)
                    return Results.BadRequest(new { mensaje = "Primero debes generar las cajas." });

                Usuario usuarioActual = authService.EstaAutenticado() ? authService.ObtenerUsuarioAutenticado() : usuario;
                double saldoAntes = usuarioActual.MaevsTokens;
                
                casino.SeleccionarYAbrirCajas(usuarioActual, ultimasCajasGeneradas, req.IdsSeleccionados);

                return Results.Ok(new { 
                    exito = true, 
                    saldoActual = usuarioActual.MaevsTokens,
                    itemsTotales = usuarioActual.Avatar.ItemsEquipados.Select(i => i.ToString()).ToList(),
                    multiplicadorFinal = usuarioActual.Avatar.MultiplicadorTotal
                });
            } catch (Exception ex) {
                return Results.BadRequest(new { mensaje = ex.Message });
            }
        });

        await app.RunAsync();
    }
}

// 📦 CLASES DTO (Data Transfer Objects)
public class RegistroRequest 
{ 
    public string Username { get; set; }
    public string Contraseña { get; set; }
    public string EstiloAvatar { get; set; }
}

public class LoginRequest 
{ 
    public string Username { get; set; }
    public string Contraseña { get; set; }
}

public class CambiarContraseñaRequest 
{ 
    public string ContraseñaAntigua { get; set; }
    public string ContraseñaNueva { get; set; }
}

public class RecargaRequest { public double MontoUsd { get; set; } }
public class ApuestaRequest { public double Apuesta { get; set; } }
public class AbrirCajasRequest { public List<int> IdsSeleccionados { get; set; } }


public class ExtractRequest { public double ApuestaActual { get; set; } }
public class CompraTokensRequest { public double USD { get; set; } }
public class VenderTokensRequest { public double Tokens { get; set; } }

public class IntentarTrampaRequest { public string PersonajeActual { get; set; } }
