using System.Windows;
using System.Windows.Input;
using Sistema_Gestion.Helpers;

namespace Sistema_Gestion.ViewModels
{

    // ViewModel de la ventana principal. Decide QUÉ pantalla se muestra en la zona
    // de contenido (VistaActual) y gestiona los botones del menú lateral.
    
    public class MainViewModel : ViewModelBase
    {
        private object? _vistaActual;
        private string _seccionActual = "Pacientes";

        // ViewModel de la pantalla de pacientes (se crea una vez y se reutiliza).
        private readonly PacientesViewModel _pacientesViewModel;
        private readonly CitasViewModel _citasViewModel;

        // ViewModel de la pantalla visible. La ventana lo muestra con un ContentControl
        // y un DataTemplate elige automáticamente la View que le corresponde.

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

        public ICommand MostrarGestionCommand { get; }
        public ICommand MostrarCitasCommand { get; }
        public ICommand SalirCommand { get; }

        public MainViewModel()
        {
            _pacientesViewModel = new PacientesViewModel();
            _citasViewModel = new CitasViewModel();

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

            SalirCommand = new RelayCommand(_ => Application.Current.Shutdown());

            // Al abrir la aplicación se muestra la pantalla de pacientes.
            VistaActual = _pacientesViewModel;
        }
    }
}
