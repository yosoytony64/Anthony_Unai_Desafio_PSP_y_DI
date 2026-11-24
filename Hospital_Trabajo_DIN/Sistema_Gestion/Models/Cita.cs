using Sistema_Gestion.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_Gestion.Models
{
    // Tabla 'citas'. Es la tabla intermedia de la relación N:M entre
    // pacientes y médicos, con atributos propios (fecha_hora, motivo, estado).
    
    public class Cita : ObservableObject
    {
        private int _idCita;
        private int _idPaciente;
        private int _idMedico;
        private DateTime _fechaHora = DateTime.Now;
        private string? _motivo = "";
        private string _estado = "Programada";
        private string _pacienteNombre = "";
        private string _medicoNombre = "";

        public int IdCita { get => _idCita; set => SetProperty(ref _idCita, value); }
        public int IdPaciente { get => _idPaciente; set => SetProperty(ref _idPaciente, value); }
        public int IdMedico { get => _idMedico; set => SetProperty(ref _idMedico, value); }
        public DateTime FechaHora { get => _fechaHora; set => SetProperty(ref _fechaHora, value); }
        public string? Motivo { get => _motivo; set => SetProperty(ref _motivo, value); }
        public string Estado { get => _estado; set => SetProperty(ref _estado, value); }

        // Estas dos propiedades NO son columnas de la tabla ([NotMapped]).
        // Sirven solo para enseñar el nombre del paciente y del médico en el DataGrid.
        [NotMapped]
        public string PacienteNombre { get => _pacienteNombre; set => SetProperty(ref _pacienteNombre, value); }

        [NotMapped]
        public string MedicoNombre { get => _medicoNombre; set => SetProperty(ref _medicoNombre, value); }
    }
}
