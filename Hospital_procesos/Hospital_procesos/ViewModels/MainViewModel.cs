using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Hospital_procesos.Models;
using Hospital_procesos.Services;

namespace Hospital_procesos.ViewModels
{
    // ViewModel principal de la aplicación.
    // Conecta el servidor Named Pipes, la persistencia JSON y la interfaz gráfica de usuario.
    // Todas las operaciones procedentes de hilos secundarios se sincronizan con la interfaz
    // a través del Dispatcher de WPF.
    public class MainViewModel : ViewModelBase
    {
        private readonly NamedPipeServerService _servidor = new();
        private readonly HistorialJsonService _historialService = new();

        private PeticionRegistro? _peticionSeleccionada;
        private string _estadoServidor = "Detenido";
        private bool _servidorIniciado;
        private int _totalPeticiones;
        private int _totalCorrectas;
        private int _totalErrores;

        public MainViewModel()
        {
            Peticiones = new ObservableCollection<PeticionRegistro>();
            Logs = new ObservableCollection<string>();

            // Comandos
            IniciarServidorCommand = new RelayCommand(IniciarServidor, () => !ServidorIniciado);
            DetenerServidorCommand = new RelayCommand(async () => await DetenerServidorAsync(), () => ServidorIniciado);
            LimpiarHistorialCommand = new RelayCommand(LimpiarHistorial);

            // Suscripción a eventos del servidor
            _servidor.PeticionRecibida += OnPeticionRecibida;
            _servidor.EstadoServidorCambiado += OnEstadoServidorCambiado;
            _servidor.LogMensaje += OnLogMensaje;

            // Cargamos el historial persistido al iniciar
            CargarHistorialPrevio();

            // Iniciamos el servidor automáticamente al abrir la aplicación
            IniciarServidor();
        }

        // Colecciones enlazadas a la UI
        public ObservableCollection<PeticionRegistro> Peticiones { get; }
        public ObservableCollection<string> Logs { get; }

        public PeticionRegistro? PeticionSeleccionada
        {
            get => _peticionSeleccionada;
            set => SetProperty(ref _peticionSeleccionada, value);
        }

        public string EstadoServidor
        {
            get => _estadoServidor;
            set => SetProperty(ref _estadoServidor, value);
        }

        public bool ServidorIniciado
        {
            get => _servidorIniciado;
            set
            {
                if (SetProperty(ref _servidorIniciado, value))
                {
                    (IniciarServidorCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (DetenerServidorCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public int TotalPeticiones
        {
            get => _totalPeticiones;
            set => SetProperty(ref _totalPeticiones, value);
        }

        public int TotalCorrectas
        {
            get => _totalCorrectas;
            set => SetProperty(ref _totalCorrectas, value);
        }

        public int TotalErrores
        {
            get => _totalErrores;
            set => SetProperty(ref _totalErrores, value);
        }

        // Información institucional y académica para la pestaña Acerca de
        public string NombreAplicacion => "Hospital_procesos — Servidor de Análisis Clínicos";
        public string Autores => "Anthony y Unai";
        public string Asignaturas => "Diseño de Interfaces (DI) & Programación de Servicios y Procesos (PSP)";
        public string FechaVersion => "Febrero 2026 — Versión 1.0 (.NET 10 WPF)";
        public string NombrePipe => NamedPipeServerService.NombrePipe;

        // Comandos de acción
        public ICommand IniciarServidorCommand { get; }
        public ICommand DetenerServidorCommand { get; }
        public ICommand LimpiarHistorialCommand { get; }

        private void IniciarServidor()
        {
            if (ServidorIniciado) return;

            _servidor.Iniciar();
            ServidorIniciado = true;
        }

        public async Task DetenerServidorAsync()
        {
            if (!ServidorIniciado) return;

            await _servidor.DetenerAsync();
            ServidorIniciado = false;
        }

        // Carga los registros del archivo JSON en la ObservableCollection
        private void CargarHistorialPrevio()
        {
            try
            {
                var guardadas = _historialService.CargarHistorial();
                foreach (var item in guardadas)
                {
                    Peticiones.Add(item);
                }

                if (Peticiones.Count > 0)
                {
                    PeticionSeleccionada = Peticiones[0];
                }

                ActualizarEstadisticas();
                AgregarLog($"Historial cargado: {Peticiones.Count} peticiones previas recuperadas.");
            }
            catch (Exception ex)
            {
                AgregarLog($"No se pudo recuperar el historial previo: {ex.Message}");
            }
        }

        private void LimpiarHistorial()
        {
            Peticiones.Clear();
            PeticionSeleccionada = null;
            ActualizarEstadisticas();
            _historialService.GuardarHistorial(Peticiones);
            AgregarLog("El historial de peticiones ha sido limpiado.");
        }

        // Manejador del evento cuando llega una petición procesada desde el hilo secundario
        private async void OnPeticionRecibida(object? sender, PeticionRegistro registro)
        {
            // OBLIGATORIO: Se utiliza el Dispatcher de WPF para actualizar la interfaz gráfica
            // de forma segura desde el hilo en segundo plano que atiende la conexión Named Pipe
            if (Application.Current != null)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    // Añadimos la petición al principio para ver primero la más reciente
                    Peticiones.Insert(0, registro);
                    PeticionSeleccionada = registro;

                    ActualizarEstadisticas();

                    // Persistimos inmediatamente en el archivo JSON
                    _historialService.GuardarHistorial(Peticiones);
                });
            }
        }

        private async void OnEstadoServidorCambiado(object? sender, string nuevoEstado)
        {
            if (Application.Current != null)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    EstadoServidor = nuevoEstado;
                });
            }
        }

        private async void OnLogMensaje(object? sender, string mensaje)
        {
            if (Application.Current != null)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    AgregarLog(mensaje);
                });
            }
        }

        private void AgregarLog(string mensaje)
        {
            string entrada = $"[{DateTime.Now:HH:mm:ss}] {mensaje}";
            Logs.Insert(0, entrada);

            // Limitamos a 100 entradas de log para optimizar memoria
            if (Logs.Count > 100)
            {
                Logs.RemoveAt(Logs.Count - 1);
            }
        }

        private void ActualizarEstadisticas()
        {
            TotalPeticiones = Peticiones.Count;
            TotalCorrectas = Peticiones.Count(p => p.EsCorrecta);
            TotalErrores = Peticiones.Count(p => p.EsError);
        }
    }
}
