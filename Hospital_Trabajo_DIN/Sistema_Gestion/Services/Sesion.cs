using Sistema_Gestion.Models;

namespace Sistema_Gestion.Services
{
    // Guarda quién ha iniciado sesión mientras la aplicación está abierta.
    // Es estática porque hay una única sesión para toda la aplicación.
    public static class Sesion
    {
        public static Usuario? UsuarioActual { get; private set; }

        public static void Iniciar(Usuario usuario) => UsuarioActual = usuario;

        public static void Cerrar() => UsuarioActual = null;
    }
}
