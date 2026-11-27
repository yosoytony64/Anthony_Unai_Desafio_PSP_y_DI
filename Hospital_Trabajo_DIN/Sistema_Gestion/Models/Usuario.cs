namespace Sistema_Gestion.Models
{
    // Tabla 'usuarios'. Se usa para el inicio de sesión.
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = "";
        public string PasswordHash { get; set; } = "";   // hash SHA-256, nunca la contraseña en claro
        public string Rol { get; set; } = "Recepcion";   // Administrador, Medico o Recepcion
        public int? IdMedico { get; set; }               // médico asociado (opcional)
    }
}
