using System.ComponentModel.DataAnnotations.Schema;
using Sistema_Gestion.Helpers;

namespace Sistema_Gestion.Models
{
    // Tabla 'resultados_pruebas'. Aquí se guarda lo que devuelve la aplicación de procesamiento.
    public class ResultadoPrueba : ObservableObject
    {
        private int _idResultado;
        private int _idPaciente;
        private int _idPrueba;
        private int? _idMedicoSolicitante;
        private DateTime _fechaRealizacion = DateTime.Now;
        private string _resultadoDetalles = "";
        private string? _observaciones = "";
        private string _pacienteNombre = "";
        private string _pruebaNombre = "";

        public int IdResultado { get => _idResultado; set => SetProperty(ref _idResultado, value); }
        public int IdPaciente { get => _idPaciente; set => SetProperty(ref _idPaciente, value); }
        public int IdPrueba { get => _idPrueba; set => SetProperty(ref _idPrueba, value); }
        public int? IdMedicoSolicitante { get => _idMedicoSolicitante; set => SetProperty(ref _idMedicoSolicitante, value); }
        public DateTime FechaRealizacion { get => _fechaRealizacion; set => SetProperty(ref _fechaRealizacion, value); }
        public string ResultadoDetalles { get => _resultadoDetalles; set => SetProperty(ref _resultadoDetalles, value); }
        public string? Observaciones { get => _observaciones; set => SetProperty(ref _observaciones, value); }

        // No son columnas de la tabla ([NotMapped]): sirven para enseñar nombres en el DataGrid.
        [NotMapped]
        public string PacienteNombre { get => _pacienteNombre; set => SetProperty(ref _pacienteNombre, value); }

        [NotMapped]
        public string PruebaNombre { get => _pruebaNombre; set => SetProperty(ref _pruebaNombre, value); }
    }
}