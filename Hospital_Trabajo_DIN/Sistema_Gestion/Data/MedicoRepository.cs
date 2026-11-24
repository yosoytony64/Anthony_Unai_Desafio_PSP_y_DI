using Microsoft.EntityFrameworkCore;
using Sistema_Gestion.Models;

namespace Sistema_Gestion.Data
{
    // Repositorio de médicos. De momento solo lectura: se usa para rellenar
    // el ComboBox de médicos en la pantalla de citas.
    public class MedicoRepository
    {
        public List<Medico> ObtenerTodos()
        {
            using var contexto = new HospitalContext();

            return contexto.Medicos
                           .AsNoTracking()
                           .OrderBy(m => m.Apellidos)
                           .ThenBy(m => m.Nombre)
                           .ToList();
        }
    }
}
