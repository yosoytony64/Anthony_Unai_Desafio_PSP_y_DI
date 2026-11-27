using System.Security.Cryptography;
using System.Text;

namespace Sistema_Gestion.Helpers
{
    // Utilidades de seguridad para las contraseñas.
    public static class Seguridad
    {
        // Calcula el hash SHA-256 de un texto y lo devuelve en hexadecimal (64 caracteres).
        // Un hash es una huella de un solo sentido: de la huella no se puede recuperar la contraseña.
        public static string CalcularHash(string texto)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(texto));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
