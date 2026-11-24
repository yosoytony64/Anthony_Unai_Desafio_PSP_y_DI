using Microsoft.EntityFrameworkCore;
using Sistema_Gestion.Models;

namespace Sistema_Gestion.Data
{
    // Repositorio de citas
    public class CitaRepository
    {
        // LEER
        public List<Cita> ObtenerTodas()
        {
            using var contexto = new HospitalContext();

            // Unimos (JOIN) citas con pacientes y médicos para obtener los nombres.
            var consulta = from c in contexto.Citas.AsNoTracking()
                           join p in contexto.Pacientes on c.IdPaciente equals p.IdPaciente
                           join m in contexto.Medicos on c.IdMedico equals m.IdMedico
                           orderby c.FechaHora descending
                           select new
                           {
                               Cita = c,
                               PacienteNombre = p.Nombre,
                               PacienteApellidos = p.Apellidos,
                               MedicoNombre = m.Nombre,
                               MedicoApellidos = m.Apellidos
                           };

            var lista = new List<Cita>();
            foreach (var fila in consulta.ToList())
            {
                fila.Cita.PacienteNombre = fila.PacienteNombre + " " + fila.PacienteApellidos;
                fila.Cita.MedicoNombre = fila.MedicoNombre + " " + fila.MedicoApellidos;
                lista.Add(fila.Cita);
            }
            return lista;
        }

        // CREAR
        public int Insertar(Cita c)
        {
            using var contexto = new HospitalContext();

            contexto.Citas.Add(c);
            contexto.SaveChanges();

            return c.IdCita;
        }

        // ACTUALIZAR
        public void Actualizar(Cita c)
        {
            using var contexto = new HospitalContext();

            contexto.Citas.Update(c);
            contexto.SaveChanges();
        }

        // BORRAR
        public void Eliminar(int idCita)
        {
            using var contexto = new HospitalContext();

            var cita = contexto.Citas.Find(idCita);
            if (cita == null) return;

            contexto.Citas.Remove(cita);
            contexto.SaveChanges();
        }

        // Validación: ¿el médico ya tiene una cita a esa hora? (las canceladas no cuentan)
        // idExcluir sirve al editar, para no compararla consigo misma. Al crear se pasa 0.
        public bool MedicoOcupado(int idMedico, DateTime fechaHora, int idExcluir = 0)
        {
            using var contexto = new HospitalContext();

            return contexto.Citas.Any(c => c.IdMedico == idMedico
                                        && c.FechaHora == fechaHora
                                        && c.Estado != "Cancelada"
                                        && c.IdCita != idExcluir);
        }

        // Validación: ¿el paciente ya tiene una cita a esa hora?
        public bool PacienteOcupado(int idPaciente, DateTime fechaHora, int idExcluir = 0)
        {
            using var contexto = new HospitalContext();

            return contexto.Citas.Any(c => c.IdPaciente == idPaciente
                                        && c.FechaHora == fechaHora
                                        && c.Estado != "Cancelada"
                                        && c.IdCita != idExcluir);
        }
    }
}
