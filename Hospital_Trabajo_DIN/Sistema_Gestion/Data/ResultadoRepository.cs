using Microsoft.EntityFrameworkCore;
using Sistema_Gestion.Models;

namespace Sistema_Gestion.Data
{
    // Repositorio de resultados de pruebas (tabla resultados_pruebas).
    public class ResultadoRepository
    {
        // LEER: todos los resultados con el nombre del paciente y de la prueba.
        public List<ResultadoPrueba> ObtenerTodos()
        {
            using var contexto = new HospitalContext();

            var consulta = from r in contexto.Resultados.AsNoTracking()
                           join p in contexto.Pacientes on r.IdPaciente equals p.IdPaciente
                           join t in contexto.Pruebas on r.IdPrueba equals t.IdPrueba
                           orderby r.FechaRealizacion descending
                           select new
                           {
                               Resultado = r,
                               PacienteNombre = p.Nombre,
                               PacienteApellidos = p.Apellidos,
                               PruebaNombre = t.NombrePrueba
                           };

            var lista = new List<ResultadoPrueba>();
            foreach (var fila in consulta.ToList())
            {
                fila.Resultado.PacienteNombre = fila.PacienteNombre + " " + fila.PacienteApellidos;
                fila.Resultado.PruebaNombre = fila.PruebaNombre;
                lista.Add(fila.Resultado);
            }
            return lista;
        }

        // CREAR: guarda varios resultados de una vez.
        // SaveChanges se ejecuta como una sola transacción: o se guardan todos o ninguno.
        public void InsertarVarios(List<ResultadoPrueba> resultados)
        {
            using var contexto = new HospitalContext();

            contexto.Resultados.AddRange(resultados);
            contexto.SaveChanges();
        }
    }
}
