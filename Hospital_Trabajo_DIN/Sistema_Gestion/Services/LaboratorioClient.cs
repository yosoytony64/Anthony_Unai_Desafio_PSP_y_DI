using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using Sistema_Gestion.Models;

namespace Sistema_Gestion.Services
{
    // Cliente del Named Pipe: envía la solicitud en JSON a la aplicación de procesamiento
    // y devuelve la respuesta. Además guarda los JSON enviados y recibidos en archivos.

    // PROTOCOLO (hay que acordarlo con la otra aplicación):
    //   - Nombre del pipe: HospitalLabPipe
    //   - Un mensaje = un JSON en UNA sola línea, terminado con salto de línea.
    //   - La aplicación 1 envía una línea y la aplicación 2 responde con otra línea.
    //   - Codificación UTF-8.Anthony LEE ESTO 
    public class LaboratorioClient
    {
        private const string NombrePipe = "HospitalLabPipe";
        private const int TiempoEsperaMs = 5000;

        // Para enviar: JSON compacto (una línea). Para los archivos: JSON con sangría.
        private static readonly JsonSerializerOptions OpcionesCompactas = new();
        private static readonly JsonSerializerOptions OpcionesIndentadas = new() { WriteIndented = true };
        private static readonly JsonSerializerOptions OpcionesLectura = new() { PropertyNameCaseInsensitive = true };

        // Carpeta donde se guardan los JSON (junto al ejecutable).
        public static string CarpetaJson => Path.Combine(AppContext.BaseDirectory, "JSON");

        // Genera el siguiente identificador (SOL-2026-001, SOL-2026-002...)
        // contando los archivos de solicitudes ya enviadas este año.
        public string SiguienteIdSolicitud()
        {
            Directory.CreateDirectory(CarpetaJson);

            int anio = DateTime.Now.Year;
            int enviadas = Directory.GetFiles(CarpetaJson, $"SOL-{anio}-*_enviado.json").Length;

            return $"SOL-{anio}-{enviadas + 1:000}";
        }

        // Envía la solicitud y espera la respuesta sin bloquear la interfaz (async/await).
        // Puede lanzar: TimeoutException (no hay servidor), OperationCanceledException
        // (no responde a tiempo), IOException (conexión rota) o JsonException (JSON inválido).
        public async Task<RespuestaLab> EnviarAsync(SolicitudLab solicitud)
        {
            Directory.CreateDirectory(CarpetaJson);

            // 1. Serializamos y guardamos el JSON que vamos a enviar.
            string jsonEnviado = JsonSerializer.Serialize(solicitud, OpcionesCompactas);
            await File.WriteAllTextAsync(
                RutaArchivo(solicitud.IdSolicitud, "enviado"),
                JsonSerializer.Serialize(solicitud, OpcionesIndentadas));

            // 2. Nos conectamos al pipe (el "." significa este mismo ordenador).
            using var cliente = new NamedPipeClientStream(".", NombrePipe, PipeDirection.InOut, PipeOptions.Asynchronous);
            await cliente.ConnectAsync(TiempoEsperaMs);

            using var escritor = new StreamWriter(cliente) { AutoFlush = true };
            using var lector = new StreamReader(cliente);

            // 3. Enviamos la solicitud (una línea).
            await escritor.WriteLineAsync(jsonEnviado);

            // 4. Esperamos la respuesta, con un tiempo máximo.
            using var limite = new CancellationTokenSource(TiempoEsperaMs);
            string? jsonRecibido = await lector.ReadLineAsync(limite.Token);

            if (string.IsNullOrWhiteSpace(jsonRecibido))
                throw new IOException("El laboratorio cerró la conexión sin enviar respuesta.");

            // 5. Guardamos el JSON recibido (formateado) y lo convertimos a objeto.
            using var documento = JsonDocument.Parse(jsonRecibido);
            await File.WriteAllTextAsync(
                RutaArchivo(solicitud.IdSolicitud, "recibido"),
                JsonSerializer.Serialize(documento.RootElement, OpcionesIndentadas));

            return JsonSerializer.Deserialize<RespuestaLab>(jsonRecibido, OpcionesLectura)
                   ?? throw new IOException("La respuesta del laboratorio está vacía.");
        }

        private static string RutaArchivo(string idSolicitud, string tipo) =>
            Path.Combine(CarpetaJson, $"{idSolicitud}_{tipo}.json");
    }
}
