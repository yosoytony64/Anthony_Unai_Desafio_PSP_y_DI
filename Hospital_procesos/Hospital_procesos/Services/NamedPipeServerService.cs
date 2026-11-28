using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using Hospital_procesos.Models;

namespace Hospital_procesos.Services
{
    // Servidor Named Pipe que escucha en segundo plano peticiones de la Aplicación 1 (Sistema_Gestion).
    // Gestiona conexiones concurrentes, deserialización de mensajes, procesamiento de resultados
    // y notificación a la interfaz gráfica sin bloquear el hilo principal de WPF.
    public class NamedPipeServerService
    {
        public const string NombrePipe = "HospitalLabPipe";

        private static readonly JsonSerializerOptions OpcionesCompactas = new()
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        private static readonly JsonSerializerOptions OpcionesIndentadas = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        private static readonly JsonSerializerOptions OpcionesLectura = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly ProcesadorLaboratorioService _procesador = new();
        private CancellationTokenSource? _cts;
        private Task? _tareaServidor;
        private bool _estaIniciado;

        // Eventos para comunicar al ViewModel y la UI los cambios de estado y peticiones recibidas
        public event EventHandler<PeticionRegistro>? PeticionRecibida;
        public event EventHandler<string>? EstadoServidorCambiado;
        public event EventHandler<string>? LogMensaje;

        public bool EstaIniciado => _estaIniciado;

        // Inicia el servidor Named Pipe en segundo plano usando Task.Run y CancellationToken
        public void Iniciar()
        {
            if (_estaIniciado) return;

            _cts = new CancellationTokenSource();
            _estaIniciado = true;
            EstadoServidorCambiado?.Invoke(this, "Escuchando conexiones...");
            LogMensaje?.Invoke(this, $"Servidor iniciado en \\\\.\\pipe\\{NombrePipe}");

            _tareaServidor = Task.Run(() => BucleEscuchaAsync(_cts.Token));
        }

        // Detiene el servidor y cancela la escucha de nuevas conexiones de manera limpia
        public async Task DetenerAsync()
        {
            if (!_estaIniciado) return;

            _estaIniciado = false;
            _cts?.Cancel();

            // Enviamos un cliente de señalización rápido para desbloquear WaitForConnectionAsync si fuera necesario
            try
            {
                using var dummyClient = new NamedPipeClientStream(".", NombrePipe, PipeDirection.InOut);
                await dummyClient.ConnectAsync(100).ConfigureAwait(false);
            }
            catch
            {
                // Ignorado: si no conecta de inmediato, el token cancela la operación
            }

            if (_tareaServidor != null)
            {
                try
                {
                    await _tareaServidor.ConfigureAwait(false);
                }
                catch (OperationCanceledException) { }
                catch (Exception) { }
            }

            _cts?.Dispose();
            _cts = null;

            EstadoServidorCambiado?.Invoke(this, "Detenido");
            LogMensaje?.Invoke(this, "Servidor detenido correctamente.");
        }

        // Bucle continuo en segundo plano que atiende conexiones de clientes
        private async Task BucleEscuchaAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                NamedPipeServerStream? server = null;
                try
                {
                    // Creamos una nueva instancia del servidor para atender a la siguiente petición
                    server = new NamedPipeServerStream(
                        NombrePipe,
                        PipeDirection.InOut,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous);

                    // Esperamos la conexión del cliente (Aplicación 1)
                    await server.WaitForConnectionAsync(token).ConfigureAwait(false);

                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    LogMensaje?.Invoke(this, "Cliente conectado. Procesando mensaje...");

                    // Atendemos la conexión
                    await ProcesarConexionClienteAsync(server, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Cancelación normal al detener el servidor
                    break;
                }
                catch (Exception ex)
                {
                    LogMensaje?.Invoke(this, $"Aviso en bucle del servidor: {ex.Message}");
                }
                finally
                {
                    if (server != null)
                    {
                        try
                        {
                            if (server.IsConnected)
                            {
                                server.Disconnect();
                            }
                        }
                        catch { }
                        server.Dispose();
                    }
                }
            }
        }

        // Lee el mensaje JSON del cliente, valida, calcula resultados y devuelve la respuesta
        private async Task ProcesarConexionClienteAsync(NamedPipeServerStream server, CancellationToken token)
        {
            using var reader = new StreamReader(server, Encoding.UTF8, leaveOpen: true);
            using var writer = new StreamWriter(server, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };

            string? lineaJson = await reader.ReadLineAsync(token).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(lineaJson))
            {
                LogMensaje?.Invoke(this, "El cliente se desconectó sin enviar datos.");
                return;
            }

            var registro = new PeticionRegistro
            {
                FechaHora = DateTime.Now,
                JsonRecibido = FormatearJson(lineaJson)
            };

            SolicitudLab? solicitud = null;
            RespuestaLab respuesta;

            try
            {
                // Deserializamos la solicitud
                solicitud = JsonSerializer.Deserialize<SolicitudLab>(lineaJson, OpcionesLectura);

                if (solicitud != null)
                {
                    registro.IdPeticion = solicitud.IdSolicitud;
                    registro.IdMuestra = solicitud.Muestra?.IdMuestra ?? string.Empty;
                    registro.TipoMuestra = solicitud.Muestra?.Tipo ?? string.Empty;
                    registro.MuestraValida = solicitud.Muestra?.Valida ?? false;
                    registro.PruebasRecibidas = solicitud.Pruebas ?? new List<PruebaSolicitadaJson>();
                    registro.TipoOperacion = $"Análisis de Laboratorio ({registro.TipoMuestra})";

                    // Procesamos y clasificamos con el servicio de laboratorio
                    respuesta = _procesador.Procesar(solicitud, out string? errorValidacion);
                    registro.EstadoFinal = respuesta.Estado;
                    registro.ResultadosCalculados = respuesta.Resultados;
                    registro.ErrorTecnico = errorValidacion;
                }
                else
                {
                    registro.IdPeticion = "SOL-ERROR";
                    registro.EstadoFinal = "Error de validación";
                    registro.ErrorTecnico = "No se pudieron interpretar los datos de la solicitud.";
                    respuesta = new RespuestaLab
                    {
                        IdSolicitud = "DESCONOCIDO",
                        Estado = "Error de validación",
                        Error = registro.ErrorTecnico
                    };
                }
            }
            catch (JsonException ex)
            {
                // Error de deserialización JSON
                registro.IdPeticion = "JSON-INVALIDO";
                registro.EstadoFinal = "Error técnico";
                registro.ErrorTecnico = $"Formato JSON mal formado: {ex.Message}";
                respuesta = new RespuestaLab
                {
                    IdSolicitud = "ERROR",
                    Estado = "Error técnico",
                    Error = "El JSON enviado no tiene una estructura válida."
                };
            }
            catch (Exception ex)
            {
                // Excepción técnica inesperada
                registro.IdPeticion = "EXCEPCION";
                registro.EstadoFinal = "Error técnico";
                registro.ErrorTecnico = $"Excepción interna: {ex.Message}";
                respuesta = new RespuestaLab
                {
                    IdSolicitud = "ERROR",
                    Estado = "Error técnico",
                    Error = "Se produjo un error interno en el servidor."
                };
            }

            // Serializamos la respuesta en una sola línea compacta (requerido por el protocolo)
            string lineaRespuesta = JsonSerializer.Serialize(respuesta, OpcionesCompactas);
            registro.JsonRespuesta = FormatearJson(lineaRespuesta);

            // Enviamos la respuesta a la Aplicación 1
            await writer.WriteLineAsync(lineaRespuesta).ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);

            try
            {
                // Esperamos que el cliente drene el pipe antes de desconectar
                server.WaitForPipeDrain();
            }
            catch { }

            LogMensaje?.Invoke(this, $"Petición {registro.IdPeticion} atendida con resultado: {registro.EstadoFinal}");

            // Notificamos al ViewModel para que actualice la interfaz a través del Dispatcher
            PeticionRecibida?.Invoke(this, registro);
        }

        // Formatea el JSON para mostrarlo indentado y legible en el panel de detalle
        private static string FormatearJson(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                return JsonSerializer.Serialize(doc.RootElement, OpcionesIndentadas);
            }
            catch
            {
                return json;
            }
        }
    }
}
