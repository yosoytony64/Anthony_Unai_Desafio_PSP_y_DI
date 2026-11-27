using Microsoft.EntityFrameworkCore;
using Sistema_Gestion.Models;

namespace Sistema_Gestion.Data
{

    // Contexto de Entity Framework Core: representa la sesión con la base de datos.
    // Cada DbSet equivale a una tabla; aquí también se indica a qué tabla y
    // columna de MySQL corresponde cada clase y propiedad (mapeo).

    public class HospitalContext : DbContext
    {
        // CAMBIA User ID / Password por los de tu MySQL.
        private const string CadenaConexion =
            "Server=localhost;Port=3306;Database=Hospital;User ID=root;Password=1234;";

        // Una propiedad DbSet por tabla
        public DbSet<Departamento> Departamentos => Set<Departamento>();
        public DbSet<Medico> Medicos => Set<Medico>();
        public DbSet<Paciente> Pacientes => Set<Paciente>();
        public DbSet<Cita> Citas => Set<Cita>();
        public DbSet<HistorialClinico> Historiales => Set<HistorialClinico>();
        public DbSet<Prueba> Pruebas => Set<Prueba>();
        public DbSet<ResultadoPrueba> Resultados => Set<ResultadoPrueba>();
        public DbSet<Usuario> Usuarios => Set<Usuario>();

        // Indica a EF Core que use MySQL y con qué cadena de conexión.
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(CadenaConexion);
        }

        // Mapeo clase -> tabla y propiedad -> columna (Fluent API).
        // Hace falta porque en C# usamos PascalCase (IdPaciente) y en la BD snake_case (id_paciente).
        // La BD ya está creada con el script SQL, por eso no usamos migraciones.

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>(e =>
            {
                e.ToTable("usuarios");
                e.HasKey(x => x.IdUsuario);
                e.Property(x => x.IdUsuario).HasColumnName("id_usuario");
                e.Property(x => x.NombreUsuario).HasColumnName("nombre_usuario");
                e.Property(x => x.PasswordHash).HasColumnName("password_hash");
                e.Property(x => x.Rol).HasColumnName("rol");
                e.Property(x => x.IdMedico).HasColumnName("id_medico");
            });

            modelBuilder.Entity<Departamento>(e =>
            {
                e.ToTable("departamentos");
                e.HasKey(x => x.IdDepartamento);
                e.Property(x => x.IdDepartamento).HasColumnName("id_departamento");
                e.Property(x => x.Nombre).HasColumnName("nombre");
                e.Property(x => x.Ubicacion).HasColumnName("ubicacion");
                e.Property(x => x.Telefono).HasColumnName("telefono");
            });

            modelBuilder.Entity<Medico>(e =>
            {
                e.ToTable("medicos");
                e.HasKey(x => x.IdMedico);
                e.Property(x => x.IdMedico).HasColumnName("id_medico");
                e.Property(x => x.Nombre).HasColumnName("nombre");
                e.Property(x => x.Apellidos).HasColumnName("apellidos");
                e.Property(x => x.Especialidad).HasColumnName("especialidad");
                e.Property(x => x.NumColegiado).HasColumnName("num_colegiado");
                e.Property(x => x.Telefono).HasColumnName("telefono");
                e.Property(x => x.Email).HasColumnName("email");
                e.Property(x => x.IdDepartamento).HasColumnName("id_departamento");
                e.Ignore(x => x.NombreCompleto); // propiedad calculada: no es columna
            });

            modelBuilder.Entity<Paciente>(e =>
            {
                e.ToTable("pacientes");
                e.HasKey(x => x.IdPaciente);
                e.Property(x => x.IdPaciente).HasColumnName("id_paciente");
                e.Property(x => x.Nombre).HasColumnName("nombre");
                e.Property(x => x.Apellidos).HasColumnName("apellidos");
                e.Property(x => x.Dni).HasColumnName("dni");
                e.Property(x => x.FechaNacimiento).HasColumnName("fecha_nacimiento").HasColumnType("date");
                e.Property(x => x.Genero).HasColumnName("genero");
                e.Property(x => x.Telefono).HasColumnName("telefono");
                e.Property(x => x.Email).HasColumnName("email");
                e.Property(x => x.Direccion).HasColumnName("direccion");
                e.Property(x => x.GrupoSanguineo).HasColumnName("grupo_sanguineo");
                e.Ignore(x => x.Edad);           // calculadas: no son columnas
                e.Ignore(x => x.NombreCompleto);
            });

            modelBuilder.Entity<Cita>(e =>
            {
                e.ToTable("citas");
                e.HasKey(x => x.IdCita);
                e.Property(x => x.IdCita).HasColumnName("id_cita");
                e.Property(x => x.IdPaciente).HasColumnName("id_paciente");
                e.Property(x => x.IdMedico).HasColumnName("id_medico");
                e.Property(x => x.FechaHora).HasColumnName("fecha_hora");
                e.Property(x => x.Motivo).HasColumnName("motivo");
                e.Property(x => x.Estado).HasColumnName("estado");
            });

            modelBuilder.Entity<HistorialClinico>(e =>
            {
                e.ToTable("historial_clinico");
                e.HasKey(x => x.IdHistorial);
                e.Property(x => x.IdHistorial).HasColumnName("id_historial");
                e.Property(x => x.IdPaciente).HasColumnName("id_paciente");
                e.Property(x => x.IdMedico).HasColumnName("id_medico");
                e.Property(x => x.IdCita).HasColumnName("id_cita");
                e.Property(x => x.FechaRegistro).HasColumnName("fecha_registro");
                e.Property(x => x.Diagnostico).HasColumnName("diagnostico");
                e.Property(x => x.Tratamiento).HasColumnName("tratamiento");
                e.Property(x => x.Observaciones).HasColumnName("observaciones");
            });

            modelBuilder.Entity<Prueba>(e =>
            {
                e.ToTable("pruebas");
                e.HasKey(x => x.IdPrueba);
                e.Property(x => x.IdPrueba).HasColumnName("id_prueba");
                e.Property(x => x.Codigo).HasColumnName("codigo");
                e.Property(x => x.NombrePrueba).HasColumnName("nombre_prueba");
                e.Property(x => x.Descripcion).HasColumnName("descripcion");
                e.Property(x => x.ValoresReferencia).HasColumnName("valores_referencia");
                e.Property(x => x.Precio).HasColumnName("precio").HasColumnType("decimal(10,2)");
                e.Property(x => x.IdDepartamento).HasColumnName("id_departamento");
            });

            modelBuilder.Entity<ResultadoPrueba>(e =>
            {
                e.ToTable("resultados_pruebas");
                e.HasKey(x => x.IdResultado);
                e.Property(x => x.IdResultado).HasColumnName("id_resultado");
                e.Property(x => x.IdPaciente).HasColumnName("id_paciente");
                e.Property(x => x.IdPrueba).HasColumnName("id_prueba");
                e.Property(x => x.IdMedicoSolicitante).HasColumnName("id_medico_solicitante");
                e.Property(x => x.FechaRealizacion).HasColumnName("fecha_realizacion");
                e.Property(x => x.ResultadoDetalles).HasColumnName("resultado_detalles");
                e.Property(x => x.Observaciones).HasColumnName("observaciones");
            });
        }

        // Se ejecuta justo antes de guardar. Limpia los textos de todo lo que se va a
        // insertar o modificar: quita espacios y convierte "" en NULL en las columnas opcionales.
        // IMPORTANTE: los CHECK de teléfono, email y grupo sanguíneo rechazan "" pero aceptan NULL.

        public override int SaveChanges()
        {
            var cambios = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .ToList();

            foreach (var entrada in cambios)
            {
                foreach (var propiedad in entrada.Properties)
                {
                    if (propiedad.CurrentValue is string texto)
                    {
                        string limpio = texto.Trim();
                        bool esOpcional = propiedad.Metadata.IsNullable;
                        propiedad.CurrentValue = (limpio.Length == 0 && esOpcional) ? null : limpio;
                    }
                }
            }

            return base.SaveChanges();
        }

        // Comprueba si se puede conectar. Si falla, devuelve el motivo en 'error'.
        public static bool ProbarConexion(out string error)
        {
            try
            {
                using var contexto = new HospitalContext();
                contexto.Database.OpenConnection();
                contexto.Database.CloseConnection();
                error = "";
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}