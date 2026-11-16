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

        // ViewModel de la pantalla de pacientes (se crea una vez y se reutiliza).
        private readonly PacientesViewModel _pacientesViewModel;

        // ViewModel de la pantalla visible. La ventana lo muestra con un ContentControl
        // y un DataTemplate elige automáticamente la View que le corresponde.
        
        public object? VistaActual
        {
            get => _vistaActual;
            set => SetProperty(ref _vistaActual, value);
        }

        public ICommand MostrarGestionCommand { get; }
        public ICommand SalirCommand { get; }

        public MainViewModel()
        {
            _pacientesViewModel = new PacientesViewModel();

            MostrarGestionCommand = new RelayCommand(_ => VistaActual = _pacientesViewModel);
            SalirCommand = new RelayCommand(_ => Application.Current.Shutdown());

            // Al abrir la aplicación se muestra la gestión de pacientes.
            VistaActual = _pacientesViewModel;
        }
    }
}
