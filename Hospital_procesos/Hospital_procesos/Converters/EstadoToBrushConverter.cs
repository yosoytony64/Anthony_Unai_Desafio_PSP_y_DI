using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Hospital_procesos.Converters
{
    // Convierte el estado final en un color distintivo para diferenciar visualmente
    // las peticiones correctas de aquellas con errores de validación o técnicos.
    public class EstadoToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string estado = value as string ?? string.Empty;

            if (estado.Equals("Procesada correctamente", StringComparison.OrdinalIgnoreCase))
            {
                return new SolidColorBrush(Color.FromRgb(34, 139, 34)); // Verde bosque (#228B22)
            }
            if (estado.Equals("Error de validación", StringComparison.OrdinalIgnoreCase))
            {
                return new SolidColorBrush(Color.FromRgb(218, 112, 14)); // Naranja quemado (#DA700E)
            }
            if (estado.StartsWith("Error", StringComparison.OrdinalIgnoreCase))
            {
                return new SolidColorBrush(Color.FromRgb(178, 34, 34)); // Rojo fuego (#B22222)
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
