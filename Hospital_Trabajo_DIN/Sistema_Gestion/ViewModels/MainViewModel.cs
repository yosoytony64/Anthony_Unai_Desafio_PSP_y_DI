using System.Windows;
using System.Windows.Input;
using Sistema_Gestion.Helpers;
using Sistema_Gestion.Services;

namespace Sistema_Gestion.ViewModels
{
    // ViewModel de la ventana principal. Decide qué pantalla se muestra en la zona
    // de contenido (VistaActual) y gestiona el menú lateral y el menú superior.
    public class MainViewModel : ViewModelBase
    {
        private object? _vistaActual;
        private string _seccionActual = "Pacientes";

        // Un ViewModel por pantalla; se crean una vez y se reutilizan.
        private readonly PacientesViewModel _pacientesViewModel;
        private readonly CitasViewModel _citasViewModel;
        private readonly PruebasViewModel _pruebasViewModel;

        // Se lanza al pulsar "Cerrar sesión"; la ventana se suscribe para volver al login.
        public event Action? CerrarSesionSolicitado;

        // ViewModel de la pantalla visible. La ventana lo muestra con un ContentControl
        // y un DataTemplate elige la View que le corresponde.
        public object? VistaActual
        {
            get => _vistaActual;
            set => SetProperty(ref _vistaActual, value);
        }

        // Nombre de la sección activa; sirve para resaltar su botón en el menú.
        public string SeccionActual
        {
            get => _seccionActual;
            set => SetProperty(ref _seccionActual, value);
        }

        // Texto del usuario conectado, por ejemplo "admin (Administrador)".
        public string UsuarioTexto =>
            Sesion.UsuarioActual == null
                ? ""
                : $"{Sesion.UsuarioActual.NombreUsuario} ({Sesion.UsuarioActual.Rol})";

        // Control de acceso por rol: recepción no puede entrar en el laboratorio.
        public bool PuedeVerLaboratorio => Sesion.UsuarioActual?.Rol != "Recepcion";

        public ICommand MostrarPacientesCommand { get; }
        public ICommand MostrarCitasCommand { get; }
        public ICommand MostrarLaboratorioCommand { get; }
        public ICommand CerrarSesionCommand { get; }
        public ICommand AcercaDeCommand { get; }
        public ICommand SalirCommand { get; }

        public MainViewModel()
        {
            _pacientesViewModel = new PacientesViewModel();
            _citasViewModel = new CitasViewModel();
            _pruebasViewModel = new PruebasViewModel();

            MostrarPacientesCommand = new RelayCommand(_ =>
            {
                SeccionActual = "Pacientes";
                VistaActual = _pacientesViewModel;
            });

            MostrarCitasCommand = new RelayCommand(_ =>
            {
                // Recargamos al entrar, por si se han creado o borrado pacientes.
                _citasViewModel.CargarDatos();
                SeccionActual = "Citas";
                VistaActual = _citasViewModel;
            });

            MostrarLaboratorioCommand = new RelayCommand(_ =>
            {
                _pruebasViewModel.CargarDatos();
                SeccionActual = "Laboratorio";
                VistaActual = _pruebasViewModel;
            });

            CerrarSesionCommand = new RelayCommand(_ =>
            {
                Sesion.Cerrar();
                CerrarSesionSolicitado?.Invoke();
            });

            AcercaDeCommand = new RelayCommand(_ =>
                MessageBox.Show(
                    "Hospital - Sistema de Gestión\nVersión 1.0.0\n\nAutores: [escribe aquí los nombres de la pareja]",
                    "Acerca de",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information));

            SalirCommand = new RelayCommand(_ => Application.Current.Shutdown());

            // Al abrir la aplicación se muestra la pantalla de pacientes.
            VistaActual = _pacientesViewModel;
        }
    }
}