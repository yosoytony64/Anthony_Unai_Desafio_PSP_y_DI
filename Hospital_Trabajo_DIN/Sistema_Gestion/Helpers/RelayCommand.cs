using System.Windows.Input;

namespace Sistema_Gestion.Helpers
{
    
    // Implementación genérica de ICommand. Permite que un Button de la View
    // ejecute un método del ViewModel mediante Command="{Binding ...}"
    // sin escribir código en el code-behind (eventos Click).
    
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _ejecutar;           // qué hace el comando
        private readonly Func<object?, bool>? _puedeEjecutar; // cuándo está habilitado (opcional)

        public RelayCommand(Action<object?> ejecutar, Func<object?, bool>? puedeEjecutar = null)
        {
            _ejecutar = ejecutar ?? throw new ArgumentNullException(nameof(ejecutar));
            _puedeEjecutar = puedeEjecutar;
        }

        // WPF pregunta si el botón debe estar habilitado.
        public bool CanExecute(object? parameter) => _puedeEjecutar == null || _puedeEjecutar(parameter);

        // WPF llama aquí cuando se pulsa el botón.
        public void Execute(object? parameter) => _ejecutar(parameter);

        // Conectamos con CommandManager: WPF vuelve a evaluar CanExecute
        // automáticamente cuando cambia el foco, la selección, etc.
       
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
