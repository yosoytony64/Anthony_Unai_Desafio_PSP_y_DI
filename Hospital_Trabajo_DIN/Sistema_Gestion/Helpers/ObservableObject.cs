using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Sistema_Gestion.Helpers
{
    // Clase base que implementa INotifyPropertyChanged.
    // Cualquier clase que herede de ella puede avisar a la interfaz 
    // de que una propiedad ha cambiado, para que se refresque sola.
    // La usan tanto los Models como los ViewModels.
    
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        
        public event PropertyChangedEventHandler? PropertyChanged;

        // Lanza el evento PropertyChanged. [CallerMemberName] rellena solo
        // el nombre de la propiedad que lo llama, así no escribimos cadenas a mano.
        
        protected void OnPropertyChanged([CallerMemberName] string? nombrePropiedad = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombrePropiedad));
        }

        // Asigna un valor a un campo privado SOLO si es distinto del actual
        // y, en ese caso, avisa a la interfaz. Devuelve true si hubo cambio.
        
        protected bool SetProperty<T>(ref T campo, T valor, [CallerMemberName] string? nombrePropiedad = null)
        {
            if (EqualityComparer<T>.Default.Equals(campo, valor))
                return false;               // mismo valor: no hacemos nada

            campo = valor;                  // guardamos el nuevo valor
            OnPropertyChanged(nombrePropiedad); // avisamos a la View
            return true;
        }
    }
}
