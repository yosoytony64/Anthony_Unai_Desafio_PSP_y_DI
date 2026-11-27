using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Input;
using Sistema_Gestion.Data;
using Sistema_Gestion.Helpers;
using Sistema_Gestion.Models;
using Sistema_Gestion.Services;

namespace Sistema_Gestion.ViewModels
{
    // ViewModel de la pantalla de laboratorio. Permite preparar una solicitud de análisis,
    // enviarla a la aplicación de procesamiento por Named Pipe, mostrar la respuesta
    // y guardar los resultados en la base de datos.
    public class PruebasViewModel : ViewModelBase
    {
        // Campos privados

        private readonly PacienteRepository _pacienteRepositorio = new();
        private readonly MedicoRepository _medicoRepositorio = new();
        private readonly PruebaRepository _pruebaRepositorio = new();
        private readonly ResultadoRepository _resultadoRepositorio = new();
        private readonly LaboratorioClient _laboratorio = new();

        private const string EstadoCorrecto = "Procesada correctamente";

        // Unidad que se propone al elegir cada prueba.
        private static readonly Dictionary<string, string> UnidadesPorCodigo = new()
        {
            ["GLU"] = "mg/dL",
            ["COL"] = "mg/dL",
            ["TRI"] = "mg/dL",
            ["HBA"] = "%",
            ["ECG"] = "lpm"
        };

        private Paciente? _pacienteSeleccionado;
        private Medico? _medicoSeleccionado;
        private string _idMuestra = "";
        private string _tipoMuestra = "Sangre";
        private bool _muestraValida = true;
        private Prueba? _pruebaSeleccionada;
        private string _valorTexto = "";
        private string _unidadTexto = "";
        private LineaPrueba? _lineaSeleccionada;
        private bool _estaEnviando;
        private string _estadoRespuesta = "";

        // Propiedades enlazadas a la View

        // Listas para los ComboBox.
        public ObservableCollection<Paciente> Pacientes { get; } = new();
        public ObservableCollection<Medico> Medicos { get; } = new();
        public ObservableCollection<Prueba> PruebasDisponibles { get; } = new();

        public string[] TiposMuestra { get; } = { "Sangre", "Orina", "Saliva", "Suero" };

        // Pruebas añadidas a la solicitud actual (ListBox).
        public ObservableCollection<LineaPrueba> Lineas { get; } = new();

        // Resultados que devuelve el laboratorio (DataGrid de la derecha).
        public ObservableCollection<ResultadoLab> ResultadosRecibidos { get; } = new();

        // Resultados guardados en la base de datos, con filtro por paciente.
        public ObservableCollection<ResultadoPrueba> Historial { get; } = new();
        public ICollectionView HistorialVista { get; }

        public Paciente? PacienteSeleccionado
        {
            get => _pacienteSeleccionado;
            set
            {
                if (SetProperty(ref _pacienteSeleccionado, value))
                    HistorialVista.Refresh();   // el historial se filtra por el paciente elegido
            }
        }

        public Medico? MedicoSeleccionado
        {
            get => _medicoSeleccionado;
            set => SetProperty(ref _medicoSeleccionado, value);
        }

        public string IdMuestra
        {
            get => _idMuestra;
            set => SetProperty(ref _idMuestra, value);
        }

        public string TipoMuestra
        {
            get => _tipoMuestra;
            set => SetProperty(ref _tipoMuestra, value);
        }

        // CheckBox "Muestra válida".
        public bool MuestraValida
        {
            get => _muestraValida;
            set => SetProperty(ref _muestraValida, value);
        }

        public Prueba? PruebaSeleccionada
        {
            get => _pruebaSeleccionada;
            set
            {
                if (SetProperty(ref _pruebaSeleccionada, value) && value != null)
                {
                    // Al elegir una prueba proponemos su unidad habitual.
                    UnidadTexto = UnidadesPorCodigo.GetValueOrDefault(value.Codigo, "mg/dL");
                }
            }
        }

        public string ValorTexto
        {
            get => _valorTexto;
            set => SetProperty(ref _valorTexto, value);
        }

        public string UnidadTexto
        {
            get => _unidadTexto;
            set => SetProperty(ref _unidadTexto, value);
        }

        public LineaPrueba? LineaSeleccionada
        {
            get => _lineaSeleccionada;
            set => SetProperty(ref _lineaSeleccionada, value);
        }

        // true mientras se espera la respuesta del laboratorio.
        public bool EstaEnviando
        {
            get => _estaEnviando;
            set => SetProperty(ref _estaEnviando, value);
        }

        // Estado que devuelve el laboratorio (por ejemplo "Procesada correctamente").
        public string EstadoRespuesta
        {
            get => _estadoRespuesta;
            set => SetProperty(ref _estadoRespuesta, value);
        }

        // Comandos

        public ICommand AnadirPruebaCommand { get; }
        public ICommand QuitarPruebaCommand { get; }
        public ICommand EnviarCommand { get; }
        public ICommand LimpiarCommand { get; }

        // Constructor

        public PruebasViewModel()
        {
            HistorialVista = CollectionViewSource.GetDefaultView(Historial);
            HistorialVista.Filter = FiltrarHistorial;

            AnadirPruebaCommand = new RelayCommand(_ => AnadirPrueba());
            QuitarPruebaCommand = new RelayCommand(_ => QuitarPrueba(), _ => LineaSeleccionada != null);
            // async: la interfaz no se bloquea mientras se espera la respuesta.
            EnviarCommand = new RelayCommand(async _ => await EnviarAsync(), _ => !EstaEnviando);
            LimpiarCommand = new RelayCommand(_ => Limpiar());

            // Los datos se cargan con CargarDatos(), que llama MainViewModel al abrir la pantalla.
        }

        // Carga de datos

        // Lee de la base de datos las listas de los ComboBox y el historial de resultados.
        public void CargarDatos()
        {
            try
            {
                Pacientes.Clear();
                foreach (var p in _pacienteRepositorio.ObtenerTodos())
                    Pacientes.Add(p);

                Medicos.Clear();
                foreach (var m in _medicoRepositorio.ObtenerTodos())
                    Medicos.Add(m);

                PruebasDisponibles.Clear();
                foreach (var t in _pruebaRepositorio.ObtenerTodas())
                    PruebasDisponibles.Add(t);

                CargarHistorial();
            }
            catch (Exception ex)
            {
                MensajeEstado = "No se pudieron cargar los datos: " + ex.Message;
                return;
            }

            Limpiar();
        }

        private void CargarHistorial()
        {
            Historial.Clear();
            foreach (var r in _resultadoRepositorio.ObtenerTodos())
                Historial.Add(r);
        }

        // Operaciones

        // Añade la prueba elegida (con su valor y unidad) a la lista de la solicitud.
        private void AnadirPrueba()
        {
            if (PruebaSeleccionada == null)
            {
                MensajeEstado = "Selecciona una prueba.";
                return;
            }

            if (!TryLeerValor(ValorTexto, out decimal valor) || valor <= 0)
            {
                MensajeEstado = "El valor medido debe ser un número mayor que cero.";
                return;
            }

            if (string.IsNullOrWhiteSpace(UnidadTexto))
            {
                MensajeEstado = "La unidad es obligatoria.";
                return;
            }

            if (Lineas.Any(l => l.Codigo == PruebaSeleccionada.Codigo))
            {
                MensajeEstado = "Esa prueba ya está añadida a la solicitud.";
                return;
            }

            Lineas.Add(new LineaPrueba
            {
                Codigo = PruebaSeleccionada.Codigo,
                Nombre = PruebaSeleccionada.NombrePrueba,
                Valor = valor,
                Unidad = UnidadTexto.Trim()
            });

            ValorTexto = "";
            MensajeEstado = "";
        }

        private void QuitarPrueba()
        {
            if (LineaSeleccionada != null)
                Lineas.Remove(LineaSeleccionada);
        }

        // Prepara la pantalla para una solicitud nueva.
        private void Limpiar()
        {
            Lineas.Clear();
            ResultadosRecibidos.Clear();
            EstadoRespuesta = "";
            ValorTexto = "";
            TipoMuestra = "Sangre";
            MuestraValida = true;
            ProponerIdMuestra();
            MensajeEstado = "";
        }

        // Propone un identificador de muestra con el mismo número que la próxima solicitud.
        private void ProponerIdMuestra()
        {
            string idSolicitud = _laboratorio.SiguienteIdSolicitud();
            IdMuestra = "M-" + idSolicitud.Substring(idSolicitud.Length - 3);
        }

        // Valida, envía la solicitud al laboratorio, muestra la respuesta y guarda los resultados.
        private async Task EnviarAsync()
        {
            string? error = ValidarSolicitud();
            if (error != null)
            {
                MensajeEstado = error;
                return;
            }

            // Guardamos estas referencias antes de esperar, por si el usuario cambia la selección.
            var paciente = PacienteSeleccionado!;
            var medico = MedicoSeleccionado!;

            // Construimos el objeto que se convertirá en JSON.
            var solicitud = new SolicitudLab
            {
                IdSolicitud = _laboratorio.SiguienteIdSolicitud(),
                Muestra = new MuestraJson
                {
                    IdMuestra = IdMuestra.Trim(),
                    Tipo = TipoMuestra,
                    Valida = MuestraValida
                },
                Pruebas = Lineas.Select(l => new PruebaSolicitadaJson
                {
                    Codigo = l.Codigo,
                    ValorMedido = l.Valor,
                    Unidad = l.Unidad
                }).ToList()
            };

            EstaEnviando = true;
            MensajeEstado = "Enviando solicitud al laboratorio...";

            try
            {
                RespuestaLab respuesta = await _laboratorio.EnviarAsync(solicitud);

                // Mostramos lo que ha devuelto el laboratorio.
                ResultadosRecibidos.Clear();
                foreach (var r in respuesta.Resultados)
                    ResultadosRecibidos.Add(r);
                EstadoRespuesta = respuesta.Estado;

                if (string.Equals(respuesta.Estado, EstadoCorrecto, StringComparison.OrdinalIgnoreCase))
                {
                    // Todo correcto: guardamos los resultados en la base de datos.
                    var nuevos = ConstruirResultados(respuesta, solicitud, paciente, medico);

                    if (nuevos.Count == 0)
                    {
                        MensajeEstado = "El laboratorio no devolvió resultados reconocibles.";
                    }
                    else
                    {
                        _resultadoRepositorio.InsertarVarios(nuevos);
                        CargarHistorial();
                        Lineas.Clear();
                        ProponerIdMuestra();
                        MensajeEstado = $"Solicitud {solicitud.IdSolicitud} procesada. Se guardaron {nuevos.Count} resultados.";
                    }
                }
                else
                {
                    // El laboratorio ha detectado un problema: no se guarda nada.
                    MensajeEstado = $"El laboratorio respondió: {respuesta.Estado}. {respuesta.Error}";
                }
            }
            catch (TimeoutException)
            {
                MensajeEstado = "No se pudo conectar con el laboratorio. ¿Está abierta la aplicación de procesamiento?";
            }
            catch (OperationCanceledException)
            {
                MensajeEstado = "El laboratorio no respondió a tiempo.";
            }
            catch (IOException ex)
            {
                MensajeEstado = "Error de comunicación con el laboratorio: " + ex.Message;
            }
            catch (JsonException)
            {
                MensajeEstado = "La respuesta del laboratorio no es un JSON válido.";
            }
            catch (Exception ex)
            {
                MensajeEstado = ErroresBD.Traducir(ex);
            }
            finally
            {
                EstaEnviando = false;
                CommandManager.InvalidateRequerySuggested();   // vuelve a habilitar el botón
            }
        }

        // Validación

        // Devuelve el primer error de la solicitud o null si es correcta.
        private string? ValidarSolicitud()
        {
            if (PacienteSeleccionado == null)
                return "Selecciona un paciente.";
            if (MedicoSeleccionado == null)
                return "Selecciona el médico solicitante.";
            if (string.IsNullOrWhiteSpace(IdMuestra))
                return "El identificador de la muestra es obligatorio.";
            if (!Regex.IsMatch(IdMuestra.Trim(), @"^M-[0-9]{3,}$"))
                return "El identificador de la muestra debe tener el formato M-001.";
            if (string.IsNullOrWhiteSpace(TipoMuestra))
                return "Selecciona el tipo de muestra.";
            if (Lineas.Count == 0)
                return "Añade al menos una prueba a la solicitud.";

            return null;
        }

        // Acepta tanto "98.5" como "98,5".
        private static bool TryLeerValor(string texto, out decimal valor)
        {
            string normalizado = (texto ?? "").Trim().Replace(',', '.');
            return decimal.TryParse(normalizado, NumberStyles.Number, CultureInfo.InvariantCulture, out valor);
        }

        // Utilidades

        // Convierte cada resultado del JSON recibido en una fila para la tabla resultados_pruebas.
        private List<ResultadoPrueba> ConstruirResultados(RespuestaLab respuesta, SolicitudLab solicitud, Paciente paciente, Medico medico)
        {
            var lista = new List<ResultadoPrueba>();

            foreach (var r in respuesta.Resultados)
            {
                // Buscamos la prueba por su código (GLU, COL...) para obtener su id.
                var prueba = PruebasDisponibles.FirstOrDefault(p => p.Codigo == r.Codigo);
                if (prueba == null) continue;

                lista.Add(new ResultadoPrueba
                {
                    IdPaciente = paciente.IdPaciente,
                    IdPrueba = prueba.IdPrueba,
                    IdMedicoSolicitante = medico.IdMedico,
                    FechaRealizacion = DateTime.Now,
                    ResultadoDetalles = $"{r.ValorMedido} {r.Unidad} - {r.Clasificacion}",
                    Observaciones = $"{solicitud.IdSolicitud} | Muestra {solicitud.Muestra.IdMuestra} ({solicitud.Muestra.Tipo})"
                });
            }

            return lista;
        }

        // Si hay un paciente elegido, el historial solo enseña sus resultados.
        private bool FiltrarHistorial(object obj)
        {
            if (obj is not ResultadoPrueba r) return false;
            if (PacienteSeleccionado == null) return true;

            return r.IdPaciente == PacienteSeleccionado.IdPaciente;
        }
    }
}
