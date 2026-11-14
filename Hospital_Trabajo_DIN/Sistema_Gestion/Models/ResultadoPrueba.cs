using Sistema_Gestion.Helpers;

namespace Sistema_Gestion.Models
{
    // Tabla 'resultados_pruebas'. Aquí se guardará lo que devuelva la app 2.
    public class ResultadoPrueba : ObservableObject
    {
        private int _idResultado;
        private int _idPaciente;
        private int _idPrueba;
        private int? _idMedicoSolicitante;
        private DateTime _fechaRealizacion = DateTime.Now;
        private string _resultadoDetalles = "";
        private string? _observaciones = "";

        public int IdResultado { get => _idResultado; set => SetProperty(ref _idResultado, value); }
        public int IdPaciente { get => _idPaciente; set => SetProperty(ref _idPaciente, value); }
        public int IdPrueba { get => _idPrueba; set => SetProperty(ref _idPrueba, value); }
        public int? IdMedicoSolicitante { get => _idMedicoSolicitante; set => SetProperty(ref _idMedicoSolicitante, value); }
        public DateTime FechaRealizacion { get => _fechaRealizacion; set => SetProperty(ref _fechaRealizacion, value); }
        public string ResultadoDetalles { get => _resultadoDetalles; set => SetProperty(ref _resultadoDetalles, value); }
        public string? Observaciones { get => _observaciones; set => SetProperty(ref _observaciones, value); }
    }
}
