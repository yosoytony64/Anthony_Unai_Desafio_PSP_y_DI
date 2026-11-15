using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using MySql.Data.MySqlClient;
using Sistema_Gestion.Data;
using Sistema_Gestion.Helpers;
using Sistema_Gestion.Models;

namespace Sistema_Gestion.ViewModels
{
    /// <summary>
    /// ViewModel de la pantalla de pacientes. Contiene:
    ///  - los datos que muestra la View (lista, seleccionado, formulario),
    ///  - los comandos de los botones (Nuevo, Guardar, Eliminar, Cancelar),
    ///  - la validación previa a guardar.
    /// No conoce ningún control WPF: la View se enlaza a él con Data Binding.
    /// </summary>
    public class PacientesViewModel : ViewModelBase
    {
        // ============ Campos privados ============

        private readonly PacienteRepository _repositorio = new();

        private Paciente? _pacienteSeleccionado;   // fila elegida en el DataGrid
        private Paciente _pacienteEdicion = new(); // copia que se edita en el formulario
        private bool _esNuevo = true;              // true = creando, false = editando
        private string _textoBusqueda = "";

        // Expresiones regulares: son las MISMAS reglas que los CHECK de la BD.
        private static readonly Regex RegexDni = new(@"^[0-9XYZ][0-9]{7}[A-Z]$");
        private static readonly Regex RegexTelefono = new(@"^[0-9]{9}$");
        private static readonly Regex RegexEmail = new(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$");
        private static readonly Regex RegexGrupo = new(@"^(A|B|AB|O)[+-]$");

        // ============ Propiedades enlazadas a la View ============

        /// <summary>Todos los pacientes. ObservableCollection avisa al DataGrid si se añade o quita uno.</summary>
        public ObservableCollection<Paciente> Pacientes { get; } = new();

        /// <summary>Vista de la lista con filtro (buscador). Es la que usa el DataGrid como ItemsSource.</summary>
        public ICollectionView PacientesVista { get; }

        /// <summary>Opciones para los ComboBox.</summary>
        public string[] Generos { get; } = { "Femenino", "Masculino", "Otro" };
        public string[] GruposSanguineos { get; } = { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };

        /// <summary>Para la tarjeta "Pacientes registrados" del panel.</summary>
        public int TotalPacientes => Pacientes.Count;

        /// <summary>Paciente seleccionado en el DataGrid (SelectedItem, TwoWay).</summary>
        public Paciente? PacienteSeleccionado
        {
            get => _pacienteSeleccionado;
            set
            {
                if (SetProperty(ref _pacienteSeleccionado, value) && value != null)
                {
                    // Al elegir una fila, cargamos una COPIA en el formulario.
                    // Así no se modifica la tabla hasta que se pulse Guardar y se valide.
                    PacienteEdicion = Clonar(value);
                    EsNuevo = false;
                    MensajeEstado = "";
                }
            }
        }

        /// <summary>Paciente que se está editando en el formulario (copia).</summary>
        public Paciente PacienteEdicion
        {
            get => _pacienteEdicion;
            set => SetProperty(ref _pacienteEdicion, value);
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

        public string TituloFormulario => EsNuevo ? "Nuevo paciente" : "Editar paciente";

        /// <summary>Texto del buscador. Cada vez que cambia, se vuelve a aplicar el filtro.</summary>
        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                if (SetProperty(ref _textoBusqueda, value))
                    PacientesVista.Refresh();
            }
        }

        // ============ Comandos (los botones de la View) ============

        public ICommand NuevoCommand { get; }
        public ICommand GuardarCommand { get; }
        public ICommand EliminarCommand { get; }
        public ICommand CancelarCommand { get; }

        // ============ Constructor ============

        public PacientesViewModel()
        {
            // Vista filtrable sobre la colección.
            PacientesVista = CollectionViewSource.GetDefaultView(Pacientes);
            PacientesVista.Filter = FiltrarPaciente;

            // Si se añade o elimina un paciente, se actualiza el contador.
            Pacientes.CollectionChanged += (_, _) => OnPropertyChanged(nameof(TotalPacientes));

            NuevoCommand = new RelayCommand(_ => Nuevo());
            GuardarCommand = new RelayCommand(_ => Guardar());
            // Eliminar solo está habilitado si hay un paciente seleccionado.
            EliminarCommand = new RelayCommand(_ => Eliminar(), _ => PacienteSeleccionado != null);
            CancelarCommand = new RelayCommand(_ => Cancelar());

            CargarPacientes();
        }

        // ============ Operaciones CRUD ============

        /// <summary>READ: carga los pacientes desde la BD a la colección.</summary>
        private void CargarPacientes()
        {
            try
            {
                Pacientes.Clear();
                foreach (var paciente in _repositorio.ObtenerTodos())
                    Pacientes.Add(paciente);
            }
            catch (Exception ex)
            {
                MensajeEstado = "No se pudieron cargar los pacientes: " + ex.Message;
            }
        }

        /// <summary>Prepara el formulario vacío para crear un paciente.</summary>
        private void Nuevo()
        {
            PacienteSeleccionado = null;       // deselecciona la fila
            PacienteEdicion = new Paciente();  // formulario en blanco
            EsNuevo = true;
            MensajeEstado = "";
        }

        /// <summary>CREATE o UPDATE según EsNuevo, siempre validando antes.</summary>
        private void Guardar()
        {
            // Normalizamos el DNI (sin espacios y en mayúsculas) antes de validar.
            PacienteEdicion.Dni = PacienteEdicion.Dni.Trim().ToUpper();

            try
            {
                string? error = Validar(PacienteEdicion);
                if (error != null)
                {
                    MensajeEstado = error;   // no se guarda nada
                    return;
                }

                if (EsNuevo)
                {
                    // CREATE: insertamos y recogemos el id que generó MySQL.
                    int id = _repositorio.Insertar(PacienteEdicion);

                    var nuevo = Clonar(PacienteEdicion);
                    nuevo.IdPaciente = id;
                    Pacientes.Add(nuevo);
                    PacienteSeleccionado = nuevo; // pasa a modo edición del paciente creado
                }
                else
                {
                    // UPDATE: actualizamos la BD y luego el objeto de la tabla.
                    _repositorio.Actualizar(PacienteEdicion);
                    CopiarValores(PacienteEdicion, PacienteSeleccionado!);
                }

                MensajeEstado = "Paciente guardado correctamente.";
            }
            catch (Exception ex)
            {
                MensajeEstado = TraducirError(ex);
            }
        }

        /// <summary>DELETE: pide confirmación y borra al paciente seleccionado.</summary>
        private void Eliminar()
        {
            if (PacienteSeleccionado == null) return;

            var respuesta = MessageBox.Show(
                $"¿Eliminar a {PacienteSeleccionado.NombreCompleto}?\n" +
                "También se borrarán sus citas, historial y resultados de pruebas.",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (respuesta != MessageBoxResult.Yes) return;

            try
            {
                _repositorio.Eliminar(PacienteSeleccionado.IdPaciente);
                Pacientes.Remove(PacienteSeleccionado);
                Nuevo();
                MensajeEstado = "Paciente eliminado.";
            }
            catch (Exception ex)
            {
                MensajeEstado = TraducirError(ex);
            }
        }

        /// <summary>Descarta los cambios del formulario.</summary>
        private void Cancelar()
        {
            if (PacienteSeleccionado != null)
                PacienteEdicion = Clonar(PacienteSeleccionado); // restaura lo guardado
            else
                Nuevo();
            MensajeEstado = "";
        }

        // ============ Validación ============

        /// <summary>
        /// Comprueba todas las reglas. Devuelve el texto del primer error
        /// encontrado, o null si los datos son correctos.
        /// </summary>
        private string? Validar(Paciente p)
        {
            // Campos obligatorios (NOT NULL en la BD).
            if (string.IsNullOrWhiteSpace(p.Nombre))
                return "El nombre es obligatorio.";
            if (string.IsNullOrWhiteSpace(p.Apellidos))
                return "Los apellidos son obligatorios.";

            // DNI: formato, letra de control y que no esté repetido.
            if (!RegexDni.IsMatch(p.Dni))
                return "El DNI no tiene un formato válido (8 números y una letra).";
            if (!LetraDniCorrecta(p.Dni))
                return "La letra del DNI no es correcta.";
            // Al editar, excluimos al propio paciente para no marcarlo como duplicado de sí mismo.
            if (_repositorio.ExisteDni(p.Dni, EsNuevo ? 0 : p.IdPaciente))
                return "Ya existe un paciente con ese DNI.";

            // Fecha de nacimiento: la edad debe ser mayor que cero y razonable.
            if (p.FechaNacimiento.Date >= DateTime.Today)
                return "La fecha de nacimiento debe ser anterior a hoy.";
            if (p.FechaNacimiento.Date < DateTime.Today.AddYears(-120))
                return "La fecha de nacimiento no es válida (más de 120 años).";

            // Campos opcionales: solo se validan si el usuario los rellena.
            if (!string.IsNullOrWhiteSpace(p.Telefono) && !RegexTelefono.IsMatch(p.Telefono.Trim()))
                return "El teléfono debe tener 9 dígitos.";
            if (!string.IsNullOrWhiteSpace(p.Email) && !RegexEmail.IsMatch(p.Email.Trim()))
                return "El email no tiene un formato válido.";
            if (!string.IsNullOrWhiteSpace(p.GrupoSanguineo) && !RegexGrupo.IsMatch(p.GrupoSanguineo.Trim()))
                return "El grupo sanguíneo no es válido (por ejemplo A+, O-, AB+).";

            return null; // todo correcto
        }

        /// <summary>
        /// Comprueba la letra del DNI/NIE: número módulo 23 -> posición en la cadena de letras.
        /// En un NIE, X, Y y Z se sustituyen por 0, 1 y 2.
        /// </summary>
        private static bool LetraDniCorrecta(string dni)
        {
            const string letras = "TRWAGMYFPDXBNJZSQVHLCKE";

            string primero = dni[0] switch
            {
                'X' => "0",
                'Y' => "1",
                'Z' => "2",
                _ => dni[0].ToString()
            };

            int numero = int.Parse(primero + dni.Substring(1, 7));
            return letras[numero % 23] == dni[8];
        }

        // ============ Buscador ============

        /// <summary>Decide si una fila se muestra: coincide con nombre, DNI o teléfono.</summary>
        private bool FiltrarPaciente(object obj)
        {
            if (obj is not Paciente p) return false;
            if (string.IsNullOrWhiteSpace(TextoBusqueda)) return true;

            string texto = TextoBusqueda.Trim();
            return p.NombreCompleto.Contains(texto, StringComparison.OrdinalIgnoreCase)
                || p.Dni.Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (p.Telefono?.Contains(texto, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        // ============ Utilidades ============

        /// <summary>Crea una copia independiente de un paciente.</summary>
        private static Paciente Clonar(Paciente o)
        {
            var copia = new Paciente();
            CopiarValores(o, copia);
            return copia;
        }

        /// <summary>Copia todos los valores de un paciente a otro.</summary>
        private static void CopiarValores(Paciente origen, Paciente destino)
        {
            destino.IdPaciente = origen.IdPaciente;
            destino.Nombre = origen.Nombre;
            destino.Apellidos = origen.Apellidos;
            destino.Dni = origen.Dni;
            destino.FechaNacimiento = origen.FechaNacimiento;
            destino.Genero = origen.Genero;
            destino.Telefono = origen.Telefono;
            destino.Email = origen.Email;
            destino.Direccion = origen.Direccion;
            destino.GrupoSanguineo = origen.GrupoSanguineo;
        }

        /// <summary>
        /// Convierte los errores en mensajes que entienda el usuario. EF Core envuelve el error
        /// real de MySQL dentro de otra excepción (DbUpdateException), así que recorremos
        /// las excepciones internas hasta encontrar la MySqlException original.
        /// </summary>
        private static string TraducirError(Exception ex)
        {
            Exception? actual = ex;
            while (actual != null && actual is not MySqlException)
                actual = actual.InnerException;

            if (actual is MySqlException mysql)
            {
                return mysql.Number switch
                {
                    1062 => "Ya existe un registro con ese valor único (por ejemplo, el DNI).",
                    3819 => "Algún dato no cumple las reglas de la base de datos (teléfono, email o grupo sanguíneo).",
                    1451 => "No se puede eliminar porque tiene datos relacionados.",
                    _ => "Error de base de datos: " + mysql.Message
                };
            }

            return "Error: " + ex.Message;
        }
    }
}
