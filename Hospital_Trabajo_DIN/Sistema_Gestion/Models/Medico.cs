using Sistema_Gestion.Helpers;

namespace Sistema_Gestion.Models
{
    /// <summary>Tabla 'medicos'. Relación 1:N con Departamento (id_departamento).</summary>
    public class Medico : ObservableObject
    {
        private int _idMedico;
        private string _nombre = "";
        private string _apellidos = "";
        private string _especialidad = "";
        private string _numColegiado = "";
        private string _telefono = "";
        private string _email = "";
        private int _idDepartamento;

        public int IdMedico        { get => _idMedico;       set => SetProperty(ref _idMedico, value); }
        public string Nombre       { get => _nombre;         set { if (SetProperty(ref _nombre, value)) OnPropertyChanged(nameof(NombreCompleto)); } }
        public string Apellidos    { get => _apellidos;      set { if (SetProperty(ref _apellidos, value)) OnPropertyChanged(nameof(NombreCompleto)); } }
        public string Especialidad { get => _especialidad;   set => SetProperty(ref _especialidad, value); }
        public string NumColegiado { get => _numColegiado;   set => SetProperty(ref _numColegiado, value); }
        public string Telefono     { get => _telefono;       set => SetProperty(ref _telefono, value); }
        public string Email        { get => _email;          set => SetProperty(ref _email, value); }
        public int IdDepartamento  { get => _idDepartamento; set => SetProperty(ref _idDepartamento, value); }

        /// <summary>Propiedad calculada (no existe en la BD): útil para ComboBox y DataGrid.</summary>
        public string NombreCompleto => $"{Nombre} {Apellidos}";
    }
}
