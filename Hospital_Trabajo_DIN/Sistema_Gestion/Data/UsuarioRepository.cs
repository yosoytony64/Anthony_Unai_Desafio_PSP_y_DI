using Microsoft.EntityFrameworkCore;
using Sistema_Gestion.Models;

namespace Sistema_Gestion.Data
{
    // Repositorio de usuarios (solo lectura, para el inicio de sesión).
    public class UsuarioRepository
    {
        // Busca un usuario por su nombre. Devuelve null si no existe.
        public Usuario? BuscarPorNombre(string nombreUsuario)
        {
            using var contexto = new HospitalContext();

            return contexto.Usuarios
                           .AsNoTracking()
                           .FirstOrDefault(u => u.NombreUsuario == nombreUsuario);
        }
    }
}
