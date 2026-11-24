using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Sistema_Gestion.Data;
using Sistema_Gestion.Helpers;
using Sistema_Gestion.Models;

namespace Sistema_Gestion.ViewModels
{
    // ViewModel de la pantalla de citas (relación N:M entre pacientes y médicos).
    // Sigue la misma idea que PacientesViewModel: el formulario edita una COPIA
    // de la cita seleccionada y solo se guarda si pasa la validación.
    public class CitasViewModel : ViewModelBase
    {
        // Campos privados

        private readonly CitaRepository _citaRepositorio = new();
        private readonly PacienteRepository _pacienteRepositorio = new();
        private readonly MedicoRepository _medicoRepositorio = new();

        private Cita? _citaSeleccionada;
        private Cita _citaEdicion = new();
        private bool _esNuevo = true;
        private DateTime? _fechaSeleccionada = DateTime.Today;
        private string _horaTexto = "09:00";
        private string _textoBusqueda = "";
        private bool _soloProgramadas;

        // Horario de la consulta: las citas solo se pueden poner entre estas horas.
        private static readonly TimeSpan HoraApertura = new(8, 0, 0);
        private static readonly TimeSpan HoraCierre = new(20, 0, 0);

        // ============ Propiedades enlazadas a la View ============

        // Datos de las tablas.
        public ObservableCollection<Cita> Citas { get; } = new();
        public ObservableCollection<Paciente> Pacientes { get; } = new();
        public ObservableCollection<Medico> Medicos { get; } = new();

        // Vista de las citas con filtro y orden (es la que usa el DataGrid).
        public ICollectionView CitasVista { get; }

        // Opciones del ComboBox de estado (las mismas que el CHECK de la base de datos).
        public string[] Estados { get; } = { "Programada", "Completada", "Cancelada", "No asistió" };

        // Contadores de las tarjetas.
        public int TotalCitas => Citas.Count;
        public int CitasProgramadas => Citas.Count(c => c.Estado == "Programada");
        public int CitasHoy => Citas.Count(c => c.FechaHora.Date == DateTime.Today);

        // Fila elegida en el DataGrid.
        public Cita? CitaSeleccionada
        {
            get => _citaSeleccionada;
            set
            {
                if (SetProperty(ref _citaSeleccionada, value) && value != null)
                {
                    // Cargamos una copia en el formulario, separando fecha y hora.
                    CitaEdicion = Clonar(value);
                    FechaSeleccionada = value.FechaHora.Date;
                    HoraTexto = value.FechaHora.ToString("HH:mm");
                    EsNuevo = false;
                    MensajeEstado = "";
                }
            }
        }

        // Cita que se edita en el formulario (copia).
        public Cita CitaEdicion
        {
            get => _citaEdicion;
            set => SetProperty(ref _citaEdicion, value);
        }

        // El DatePicker solo da la fecha; la hora se escribe aparte en HoraTexto.
        public DateTime? FechaSeleccionada
        {
            get => _fechaSeleccionada;
            set => SetProperty(ref _fechaSeleccionada, value);
        }

        public string HoraTexto
        {
            get => _horaTexto;
            set => SetProperty(ref _horaTexto, value);
        }

        public bool EsNuevo
        {
            get => _esNuevo;
            set
            {
                if (SetProperty(ref _esNuevo, value))
                    OnPropertyChanged(nameof(TituloFormulario));
            }
        }

        public string TituloFormulario => EsNuevo ? "Nueva cita" : "Editar cita";

        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                if (SetProperty(ref _textoBusqueda, value))
                    CitasVista.Refresh();
            }
        }

        // CheckBox "Solo programadas".
        public bool SoloProgramadas
        {
            get => _soloProgramadas;
            set
            {
                if (SetProperty(ref _soloProgramadas, value))
                    CitasVista.Refresh();
            }
        }

        // Comandos

        public ICommand NuevoCommand { get; }
        public ICommand GuardarCommand { get; }
        public ICommand EliminarCommand { get; }
        public ICommand CancelarCommand { get; }

        // Constructor

        public CitasViewModel()
        {
            CitasVista = CollectionViewSource.GetDefaultView(Citas);
            CitasVista.Filter = FiltrarCita;
            // Las citas más recientes primero.
            CitasVista.SortDescriptions.Add(new SortDescription(nameof(Cita.FechaHora), ListSortDirection.Descending));

            NuevoCommand = new RelayCommand(_ => Nuevo());
            GuardarCommand = new RelayCommand(_ => Guardar());
            EliminarCommand = new RelayCommand(_ => Eliminar(), _ => CitaSeleccionada != null);
            CancelarCommand = new RelayCommand(_ => Cancelar());

            // Los datos no se cargan aquí: lo hace MainViewModel con CargarDatos()
            // cada vez que se abre esta pantalla, para que los pacientes y médicos estén actualizados.
        }

        // Carga de datos

        // Lee de la base de datos las citas y las listas de los ComboBox.
        public void CargarDatos()
        {
            try
            {
                Pacientes.Clear();
                foreach (var p in _pacienteRepositorio.ObtenerTodos())
                    Pacientes.Add(p);

                Medicos.Clear();
                foreach (var m in _medicoRepositorio.ObtenerTodos())
                    Medicos.Add(m);

                Citas.Clear();
                foreach (var c in _citaRepositorio.ObtenerTodas())
                    Citas.Add(c);

                ActualizarContadores();
            }
            catch (Exception ex)
            {
                MensajeEstado = "No se pudieron cargar los datos: " + ex.Message;
                return;
            }

            Nuevo();
        }

        // Avisa a la interfaz de que los números de las tarjetas han cambiado.
        private void ActualizarContadores()
        {
            OnPropertyChanged(nameof(TotalCitas));
            OnPropertyChanged(nameof(CitasProgramadas));
            OnPropertyChanged(nameof(CitasHoy));
        }

        // Operaciones

        // Deja el formulario vacío para crear una cita.
        private void Nuevo()
        {
            CitaSeleccionada = null;
            CitaEdicion = new Cita { Estado = "Programada" };
            FechaSeleccionada = DateTime.Today;
            HoraTexto = "09:00";
            EsNuevo = true;
            MensajeEstado = "";
        }

        // CREAR o ACTUALIZAR según EsNuevo, siempre validando antes.
        private void Guardar()
        {
            try
            {
                string? error = Validar(out DateTime fechaHora);
                if (error != null)
                {
                    MensajeEstado = error;
                    return;
                }

                // Pasamos al objeto los datos que vienen de controles separados.
                CitaEdicion.FechaHora = fechaHora;
                CitaEdicion.Motivo = CitaEdicion.Motivo!.Trim();
                CitaEdicion.PacienteNombre = Pacientes.FirstOrDefault(p => p.IdPaciente == CitaEdicion.IdPaciente)?.NombreCompleto ?? "";
                CitaEdicion.MedicoNombre = Medicos.FirstOrDefault(m => m.IdMedico == CitaEdicion.IdMedico)?.NombreCompleto ?? "";

                if (EsNuevo)
                {
                    int id = _citaRepositorio.Insertar(CitaEdicion);

                    var nueva = Clonar(CitaEdicion);
                    nueva.IdCita = id;
                    Citas.Add(nueva);
                    CitaSeleccionada = nueva;
                }
                else
                {
                    _citaRepositorio.Actualizar(CitaEdicion);
                    CopiarValores(CitaEdicion, CitaSeleccionada!);
                }

                CitasVista.Refresh();      // reordena y vuelve a filtrar
                ActualizarContadores();
                MensajeEstado = "Cita guardada correctamente.";
            }
            catch (Exception ex)
            {
                MensajeEstado = ErroresBD.Traducir(ex);
            }
        }

        // Pide confirmación y borra la cita seleccionada.
        private void Eliminar()
        {
            if (CitaSeleccionada == null) return;

            var respuesta = MessageBox.Show(
                $"¿Eliminar la cita de {CitaSeleccionada.PacienteNombre} del {CitaSeleccionada.FechaHora:dd/MM/yyyy HH:mm}?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (respuesta != MessageBoxResult.Yes) return;

            try
            {
                _citaRepositorio.Eliminar(CitaSeleccionada.IdCita);
                Citas.Remove(CitaSeleccionada);
                Nuevo();
                ActualizarContadores();
                MensajeEstado = "Cita eliminada.";
            }
            catch (Exception ex)
            {
                MensajeEstado = ErroresBD.Traducir(ex);
            }
        }

        // Descarta los cambios del formulario.
        private void Cancelar()
        {
            if (CitaSeleccionada != null)
            {
                CitaEdicion = Clonar(CitaSeleccionada);
                FechaSeleccionada = CitaSeleccionada.FechaHora.Date;
                HoraTexto = CitaSeleccionada.FechaHora.ToString("HH:mm");
            }
            else
            {
                Nuevo();
            }
            MensajeEstado = "";
        }

        // Validación

        // Devuelve el primer error encontrado o null si todo es correcto.
        // Además devuelve en fechaHora la fecha y la hora ya combinadas.
        private string? Validar(out DateTime fechaHora)
        {
            fechaHora = DateTime.MinValue;

            // Claves foráneas: hay que elegir paciente y médico.
            if (CitaEdicion.IdPaciente <= 0)
                return "Selecciona un paciente.";
            if (CitaEdicion.IdMedico <= 0)
                return "Selecciona un médico.";

            // Fecha y hora.
            if (FechaSeleccionada == null)
                return "Selecciona la fecha de la cita.";

            if (!TimeSpan.TryParseExact(HoraTexto.Trim(), @"h\:mm", CultureInfo.InvariantCulture, out TimeSpan hora))
                return "La hora no es válida. Usa el formato HH:mm (por ejemplo 09:30).";

            if (hora < HoraApertura || hora > HoraCierre)
                return "La hora debe estar entre las 08:00 y las 20:00.";

            fechaHora = FechaSeleccionada.Value.Date + hora;

            // Motivo (obligatorio, máximo 255 caracteres como en la BD).
            if (string.IsNullOrWhiteSpace(CitaEdicion.Motivo))
                return "El motivo es obligatorio.";
            if (CitaEdicion.Motivo.Trim().Length > 255)
                return "El motivo no puede superar los 255 caracteres.";

            // Estado: tiene que ser uno de los permitidos.
            if (!Estados.Contains(CitaEdicion.Estado))
                return "Selecciona un estado válido.";

            // Una cita nueva programada no puede estar en el pasado.
            if (EsNuevo && CitaEdicion.Estado == "Programada" && fechaHora < DateTime.Now)
                return "No se puede programar una cita en el pasado.";

            // Solapamientos (las citas canceladas no ocupan hueco).
            if (CitaEdicion.Estado != "Cancelada")
            {
                int idExcluir = EsNuevo ? 0 : CitaEdicion.IdCita;

                if (_citaRepositorio.MedicoOcupado(CitaEdicion.IdMedico, fechaHora, idExcluir))
                    return "El médico ya tiene una cita a esa hora.";
                if (_citaRepositorio.PacienteOcupado(CitaEdicion.IdPaciente, fechaHora, idExcluir))
                    return "El paciente ya tiene una cita a esa hora.";
            }

            return null;
        }

        // Buscador y filtro

        // Decide si una cita se muestra en el DataGrid.
        private bool FiltrarCita(object obj)
        {
            if (obj is not Cita c) return false;

            // CheckBox "Solo programadas".
            if (SoloProgramadas && c.Estado != "Programada") return false;

            if (string.IsNullOrWhiteSpace(TextoBusqueda)) return true;

            string texto = TextoBusqueda.Trim();
            return c.PacienteNombre.Contains(texto, StringComparison.OrdinalIgnoreCase)
                || c.MedicoNombre.Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (c.Motivo?.Contains(texto, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        // Utilidades

        private static Cita Clonar(Cita origen)
        {
            var copia = new Cita();
            CopiarValores(origen, copia);
            return copia;
        }

        private static void CopiarValores(Cita origen, Cita destino)
        {
            destino.IdCita = origen.IdCita;
            destino.IdPaciente = origen.IdPaciente;
            destino.IdMedico = origen.IdMedico;
            destino.FechaHora = origen.FechaHora;
            destino.Motivo = origen.Motivo;
            destino.Estado = origen.Estado;
            destino.PacienteNombre = origen.PacienteNombre;
            destino.MedicoNombre = origen.MedicoNombre;
        }
    }
}