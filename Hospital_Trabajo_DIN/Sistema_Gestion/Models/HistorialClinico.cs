using Sistema_Gestion.Helpers;

namespace Sistema_Gestion.Models
{
    // Tabla 'historial_clinico'. IdCita es opcional (int? = puede ser NULL).
    public class HistorialClinico : ObservableObject
    {
        private int _idHistorial;
        private int _idPaciente;
        private int _idMedico;
        private int? _idCita;
        private DateTime _fechaRegistro = DateTime.Now;
        private string _diagnostico = "";
        private string? _tratamiento = "";
        private string? _observaciones = "";

        public int IdHistorial { get => _idHistorial; set => SetProperty(ref _idHistorial, value); }
        public int IdPaciente { get => _idPaciente; set => SetProperty(ref _idPaciente, value); }
        public int IdMedico { get => _idMedico; set => SetProperty(ref _idMedico, value); }
        public int? IdCita { get => _idCita; set => SetProperty(ref _idCita, value); }
        public DateTime FechaRegistro { get => _fechaRegistro; set => SetProperty(ref _fechaRegistro, value); }
        public string Diagnostico { get => _diagnostico; set => SetProperty(ref _diagnostico, value); }
        public string? Tratamiento { get => _tratamiento; set => SetProperty(ref _tratamiento, value); }
        public string? Observaciones { get => _observaciones; set => SetProperty(ref _observaciones, value); }
    }
}
