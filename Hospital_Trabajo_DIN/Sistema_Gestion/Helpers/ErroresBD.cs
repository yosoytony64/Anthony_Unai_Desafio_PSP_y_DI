using MySql.Data.MySqlClient;

namespace Sistema_Gestion.Helpers
{

    // Esto me ha dicho que lo ponga la IA Para entender mejor los errores
    // Convierte los errores técnicos de la base de datos en mensajes para el usuario.
    // Lo usan todos los ViewModels, así no se repite el código.
    public static class ErroresBD
    {
        public static string Traducir(Exception ex)
        {
            // EF Core envuelve el error real de MySQL dentro de otras excepciones.
            // Las vamos abriendo (InnerException) hasta encontrar la MySqlException original.
            Exception? actual = ex;
            while (actual != null && actual is not MySqlException)
                actual = actual.InnerException;

            if (actual is MySqlException mysql)
            {
                return mysql.Number switch
                {
                    1062 => "Ya existe un registro con ese valor único (por ejemplo, el DNI).",
                    3819 => "Algún dato no cumple las reglas de la base de datos.",
                    1451 => "No se puede eliminar porque tiene datos relacionados.",
                    1452 => "El paciente o el médico seleccionado ya no existe.",
                    _ => "Error de base de datos: " + mysql.Message
                };
            }

            return "Error: " + ex.Message;
        }
    }
}
