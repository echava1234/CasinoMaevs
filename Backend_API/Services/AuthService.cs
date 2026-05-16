namespace CasinoMaevs.Services;

using CasinoMaevs.Models;
using System.Security.Cryptography;
using System.Text;

public class AuthService
{
    // Diccionario que simula una base de datos: username → (passwordHash, usuario)
    private Dictionary<string, (string PasswordHash, Usuario Usuario)> _usuariosRegistrados = 
        new Dictionary<string, (string, Usuario)>();

    // Usuario actualmente autenticado en la sesión
    private Usuario? _usuarioActual = null;

    // ─── REGISTRAR NUEVO USUARIO ──────────────────────────────────────────────
    public bool Registrar(string username, string contraseña, string estiloAvatar = "Clásico")
    {
        // Validar que el username no esté vacío
        if (string.IsNullOrWhiteSpace(username))
        {
            Console.WriteLine("❌  El nombre de usuario no puede estar vacío.");
            return false;
        }

        // Validar que la contraseña no esté vacía
        if (string.IsNullOrWhiteSpace(contraseña))
        {
            Console.WriteLine("❌  La contraseña no puede estar vacía.");
            return false;
        }

        // Validar longitud mínima del username
        if (username.Length < 3)
        {
            Console.WriteLine("❌  El nombre de usuario debe tener al menos 3 caracteres.");
            return false;
        }

        // Validar longitud mínima de la contraseña
        if (contraseña.Length < 6)
        {
            Console.WriteLine("❌  La contraseña debe tener al menos 6 caracteres.");
            return false;
        }

        // Validar que el usuario no exista ya
        if (_usuariosRegistrados.ContainsKey(username))
        {
            Console.WriteLine($"❌  El usuario '{username}' ya está registrado.");
            return false;
        }

        // Encriptar la contraseña con SHA256
        string passwordHash = EncriptarContraseña(contraseña);

        // Crear nuevo usuario
        Usuario nuevoUsuario = new Usuario(username, estiloAvatar);

        // Almacenar en el diccionario
        _usuariosRegistrados[username] = (passwordHash, nuevoUsuario);

        Console.WriteLine($"✅  Usuario '{username}' registrado exitosamente.");
        return true;
    }

    // ─── LOGIN ────────────────────────────────────────────────────────────────
    public bool Login(string username, string contraseña)
    {
        // Validar que ingresó usuario y contraseña
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(contraseña))
        {
            Console.WriteLine("❌  Usuario y contraseña son requeridos.");
            return false;
        }

        // Verificar si el usuario existe
        if (!_usuariosRegistrados.ContainsKey(username))
        {
            Console.WriteLine($"❌  El usuario '{username}' no existe.");
            return false;
        }

        var (passwordHash, usuario) = _usuariosRegistrados[username];

        // Verificar contraseña
        string contraseñaIngresadaHash = EncriptarContraseña(contraseña);
        if (passwordHash != contraseñaIngresadaHash)
        {
            Console.WriteLine("❌  Contraseña incorrecta.");
            return false;
        }

        // Autenticar al usuario
        _usuarioActual = usuario;
        Console.WriteLine($"✅  Sesión iniciada como '{username}'.");
        return true;
    }

    // ─── LOGOUT ───────────────────────────────────────────────────────────────
    public bool Logout()
    {
        if (_usuarioActual == null)
        {
            Console.WriteLine("❌  No hay una sesión activa.");
            return false;
        }

        string username = _usuarioActual.Username;
        _usuarioActual = null;
        Console.WriteLine($"✅  Sesión de '{username}' cerrada.");
        return true;
    }

    // ─── OBTENER USUARIO AUTENTICADO ──────────────────────────────────────────
    public Usuario? ObtenerUsuarioAutenticado()
    {
        if (_usuarioActual == null)
        {
            Console.WriteLine("⚠️  No hay usuario autenticado.");
            return null;
        }

        return _usuarioActual;
    }

    // ─── VERIFICAR SI ESTÁ AUTENTICADO ────────────────────────────────────────
    public bool EstaAutenticado()
    {
        return _usuarioActual != null;
    }

    // ─── CAMBIAR CONTRASEÑA ───────────────────────────────────────────────────
    public bool CambiarContraseña(string contraseñaAntigua, string contraseñaNueva)
    {
        if (_usuarioActual == null)
        {
            Console.WriteLine("❌  Debes estar autenticado para cambiar la contraseña.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(contraseñaNueva) || contraseñaNueva.Length < 6)
        {
            Console.WriteLine("❌  La nueva contraseña debe tener al menos 6 caracteres.");
            return false;
        }

        var (passwordHashActual, usuario) = _usuariosRegistrados[_usuarioActual.Username];

        // Verificar que la contraseña antigua sea correcta
        string contraseñaAntiguaHash = EncriptarContraseña(contraseñaAntigua);
        if (passwordHashActual != contraseñaAntiguaHash)
        {
            Console.WriteLine("❌  La contraseña antigua es incorrecta.");
            return false;
        }

        // Actualizar la contraseña
        string nuevaContrasenaHash = EncriptarContraseña(contraseñaNueva);
        _usuariosRegistrados[_usuarioActual.Username] = (nuevaContrasenaHash, usuario);

        Console.WriteLine("✅  Contraseña actualizada exitosamente.");
        return true;
    }

    // ─── ENCRIPTAR CONTRASEÑA (SHA256) ────────────────────────────────────────
    private string EncriptarContraseña(string contraseña)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(contraseña));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }

    // ─── LISTAR TODOS LOS USUARIOS REGISTRADOS (DEBUG) ────────────────────────
    public void ListarUsuariosRegistrados()
    {
        if (_usuariosRegistrados.Count == 0)
        {
            Console.WriteLine("📭  No hay usuarios registrados.");
            return;
        }

        Console.WriteLine($"\n📋  Usuarios registrados ({_usuariosRegistrados.Count}):");
        foreach (var kvp in _usuariosRegistrados)
        {
            Console.WriteLine($"    • {kvp.Key} (Avatar: {kvp.Value.Usuario.Avatar.EstiloBase})");
        }
        Console.WriteLine();
    }
}
