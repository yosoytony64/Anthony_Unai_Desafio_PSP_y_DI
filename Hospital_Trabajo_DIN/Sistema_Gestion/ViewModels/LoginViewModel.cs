using System.Windows.Input;
using Sistema_Gestion.Data;
using Sistema_Gestion.Helpers;
using Sistema_Gestion.Services;

namespace Sistema_Gestion.ViewModels
{
    // ViewModel de la ventana de inicio de sesión.
    public class LoginViewModel : ViewModelBase
    {
        private const int MaximoIntentos = 3;

        private readonly UsuarioRepository _usuarioRepositorio = new();

        private string _nombreUsuario = "";
        private string _password = "";
        private int _intentosFallidos;

        // Se lanza cuando el usuario y la contraseña son correctos.
        // La ventana se suscribe a este evento para abrir la ventana principal.
        public event Action? AccesoConcedido;

        public string NombreUsuario
        {
            get => _nombreUsuario;
            set => SetProperty(ref _nombreUsuario, value);
        }

        // Se enlaza con el PasswordBox mediante PasswordBoxHelper.
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public ICommand EntrarCommand { get; }

        public LoginViewModel()
        {
            // El botón se bloquea al superar el máximo de intentos.
            EntrarCommand = new RelayCommand(_ => Entrar(), _ => _intentosFallidos < MaximoIntentos);
        }

        private void Entrar()
        {
            // Validación previa.
            if (string.IsNullOrWhiteSpace(NombreUsuario) || string.IsNullOrEmpty(Password))
            {
                MensajeEstado = "Escribe el usuario y la contraseña.";
                return;
            }

            try
            {
                var usuario = _usuarioRepositorio.BuscarPorNombre(NombreUsuario.Trim());

                // Comparamos el HASH de lo escrito con el hash guardado: la contraseña nunca se guarda ni se compara en claro.
                bool correcto = usuario != null &&
                                string.Equals(usuario.PasswordHash, Seguridad.CalcularHash(Password),
                                              StringComparison.OrdinalIgnoreCase);

                if (correcto)
                {
                    Sesion.Iniciar(usuario!);
                    AccesoConcedido?.Invoke();
                    return;
                }

                // Mismo mensaje si falla el usuario o la contraseña, para no dar pistas.
                _intentosFallidos++;
                Password = "";
                MensajeEstado = _intentosFallidos >= MaximoIntentos
                    ? "Has superado los 3 intentos. Cierra y vuelve a abrir la aplicación."
                    : $"Usuario o contraseña incorrectos. Intento {_intentosFallidos} de {MaximoIntentos}.";
            }
            catch (Exception ex)
            {
                MensajeEstado = ErroresBD.Traducir(ex);
            }
        }
    }
}
