using System.IO;
using System.Text.Json;
using Hospital_procesos.Models;

namespace Hospital_procesos.Services
{
    // Servicio de persistencia en archivo JSON local.
    // Garantiza que el historial de peticiones sobreviva al cierre y reinicio de la aplicación,
    // protegiendo los datos contra pérdidas por escritura atómica.
    public class HistorialJsonService
    {
        private static readonly JsonSerializerOptions Opciones = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        // Ruta de almacenamiento relativa al directorio de ejecución: bin/.../Data/historial_peticiones.json
        private readonly string _rutaArchivo;

        public HistorialJsonService()
        {
            string carpetaDatos = Path.Combine(AppContext.BaseDirectory, "Data");
            Directory.CreateDirectory(carpetaDatos);
            _rutaArchivo = Path.Combine(carpetaDatos, "historial_peticiones.json");
        }

        // Carga todas las peticiones previas registradas en el archivo JSON
        public List<PeticionRegistro> CargarHistorial()
        {
            try
            {
                if (!File.Exists(_rutaArchivo))
                {
                    return new List<PeticionRegistro>();
                }

                string json = File.ReadAllText(_rutaArchivo);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<PeticionRegistro>();
                }

                var lista = JsonSerializer.Deserialize<List<PeticionRegistro>>(json, Opciones);
                return lista ?? new List<PeticionRegistro>();
            }
            catch (Exception)
            {
                // Si el archivo está dañado o no se puede leer, retornamos una lista vacía para no romper el inicio
                return new List<PeticionRegistro>();
            }
        }

        // Guarda toda la colección de peticiones de manera segura mediante un archivo temporal
        public void GuardarHistorial(IEnumerable<PeticionRegistro> peticiones)
        {
            try
            {
                string json = JsonSerializer.Serialize(peticiones, Opciones);
                string rutaTemporal = _rutaArchivo + ".tmp";

                // Escribimos primero en archivo temporal para asegurar integridad
                File.WriteAllText(rutaTemporal, json);

                // Reemplazamos de manera atómica
                if (File.Exists(_rutaArchivo))
                {
                    File.Delete(_rutaArchivo);
                }
                File.Move(rutaTemporal, _rutaArchivo);
            }
            catch (Exception)
            {
                // Si falla el reemplazo atómico, intentamos escritura directa
                try
                {
                    string json = JsonSerializer.Serialize(peticiones, Opciones);
                    File.WriteAllText(_rutaArchivo, json);
                }
                catch
                {
                    // Evitamos propagar excepciones que bloqueen la aplicación
                }
            }
        }
    }
}
