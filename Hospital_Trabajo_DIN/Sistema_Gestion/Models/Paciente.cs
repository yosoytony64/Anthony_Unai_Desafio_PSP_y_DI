using Sistema_Gestion.Helpers;

namespace Sistema_Gestion.Models
{
    /// <summary>Tabla 'pacientes'.</summary>
    public class Paciente : ObservableObject
    {
        private int _idPaciente;
        private string _nombre = "";
        private string _apellidos = "";
        private string _dni = "";
        private DateTime _fechaNacimiento = DateTime.Today;
        private string _genero = "";
        private string _telefono = "";
        private string _email = "";
        private string _direccion = "";
        private string _grupoSanguineo = "";

        public int IdPaciente { get => _idPaciente; set => SetProperty(ref _idPaciente, value); }
        public string Nombre    { get => _nombre;    set { if (SetProperty(ref _nombre, value)) OnPropertyChanged(nameof(NombreCompleto)); } }
        public string Apellidos { get => _apellidos; set { if (SetProperty(ref _apellidos, value)) OnPropertyChanged(nameof(NombreCompleto)); } }
        public string Dni       { get => _dni;       set => SetProperty(ref _dni, value); }

        public DateTime FechaNacimiento
        {
            get => _fechaNacimiento;
            // Si cambia la fecha de nacimiento, también cambia la Edad.
            set { if (SetProperty(ref _fechaNacimiento, value)) OnPropertyChanged(nameof(Edad)); }
        }

        public string Genero         { get => _genero;         set => SetProperty(ref _genero, value); }
        public string Telefono       { get => _telefono;       set => SetProperty(ref _telefono, value); }
        public string Email          { get => _email;          set => SetProperty(ref _email, value); }
        public string Direccion      { get => _direccion;      set => SetProperty(ref _direccion, value); }
        public string GrupoSanguineo { get => _grupoSanguineo; set => SetProperty(ref _grupoSanguineo, value); }

        // ---- Propiedades calculadas (no se guardan en la BD) ----

        public string NombreCompleto => $"{Nombre} {Apellidos}";

        /// <summary>Edad en años cumplidos a día de hoy.</summary>
        public int Edad
        {
            get
            {
                var hoy = DateTime.Today;
                int edad = hoy.Year - FechaNacimiento.Year;
                // Si aún no ha cumplido años este año, restamos uno.
                if (FechaNacimiento.Date > hoy.AddYears(-edad)) edad--;
                return edad;
            }
        }
    }
}
