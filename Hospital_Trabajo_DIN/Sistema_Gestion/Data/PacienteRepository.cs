using Microsoft.EntityFrameworkCore;
using Sistema_Gestion.Models;

namespace Sistema_Gestion.Data
{

    // Repositorio de pacientes con Entity Framework Core. Ya no hay SQL escrito a mano:
    // EntityFrameworkCore lo genera a partir de estas operaciones (LINQ).
    // Cada método abre su propio contexto y lo cierra al terminar (using).

    public class PacienteRepository
    {
        // LEER
        public List<Paciente> ObtenerTodos()
        {
            using var contexto = new HospitalContext();

            // SELECT * FROM pacientes ORDER BY id_paciente
            // EntityFrameworkCore no vigila estos objetos (solo los leemos para mostrarlos).
            return contexto.Pacientes
                           .AsNoTracking()
                           .OrderBy(p => p.IdPaciente)
                           .ToList();
        }

        // CREAR
        public int Insertar(Paciente p)
        {
            using var contexto = new HospitalContext();

            contexto.Pacientes.Add(p);   // marca el objeto como "nuevo"
            contexto.SaveChanges();      // ejecuta el INSERT

            return p.IdPaciente;         // EntityFramework rellena el id autogenerado por MySQL
        }

        // ACTUALIZAR
        public void Actualizar(Paciente p)
        {
            using var contexto = new HospitalContext();

            contexto.Pacientes.Update(p); // marca el objeto como "modificado"
            contexto.SaveChanges();       // ejecuta el UPDATE
        }

        // BORRAR
        // citas, historial_clinico y resultados_pruebas tienen ON DELETE CASCADE:
        // MySQL borra también sus registros.
        public void Eliminar(int idPaciente)
        {
            using var contexto = new HospitalContext();

            var paciente = contexto.Pacientes.Find(idPaciente); // busca por clave primaria
            if (paciente == null) return;

            contexto.Pacientes.Remove(paciente);
            contexto.SaveChanges();                              // ejecuta el DELETE
        }

        // ---------- Validación: ¿el DNI ya existe? ----------
        public bool ExisteDni(string dni, int idExcluir = 0)
        {
            using var contexto = new HospitalContext();

            string dniNormalizado = dni.Trim().ToUpper();
            return contexto.Pacientes.Any(p => p.Dni == dniNormalizado && p.IdPaciente != idExcluir);
        }
    }
}
