using Sistema_Gestion.Helpers;

namespace Sistema_Gestion.Models
{
    // Tabla 'departamentos'.
    public class Departamento : ObservableObject
    {
        private int _idDepartamento;
        private string _nombre = "";
        private string? _ubicacion = "";
        private string? _telefono = "";

        public int IdDepartamento { get => _idDepartamento; set => SetProperty(ref _idDepartamento, value); }
        public string Nombre { get => _nombre; set => SetProperty(ref _nombre, value); }
        public string? Ubicacion { get => _ubicacion; set => SetProperty(ref _ubicacion, value); }
        public string? Telefono { get => _telefono; set => SetProperty(ref _telefono, value); }
    }
}
