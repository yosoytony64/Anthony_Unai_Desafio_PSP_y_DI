using Microsoft.EntityFrameworkCore;
using Sistema_Gestion.Models;

namespace Sistema_Gestion.Data
{
    // Repositorio del catálogo de pruebas (GLU, COL, TRI...). Solo lectura.
    public class PruebaRepository
    {
        public List<Prueba> ObtenerTodas()
        {
            using var contexto = new HospitalContext();

            return contexto.Pruebas
                           .AsNoTracking()
                           .OrderBy(p => p.Codigo)
                           .ToList();
        }
    }
}
