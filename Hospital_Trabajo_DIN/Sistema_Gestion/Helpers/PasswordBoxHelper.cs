using System.Windows;
using System.Windows.Controls;

namespace Sistema_Gestion.Helpers
{
    // El PasswordBox de WPF no permite Data Binding en su propiedad Password (por seguridad).
    // Esta propiedad adjunta (attached property) añade una propiedad "BoundPassword" que sí
    // se puede enlazar, y la mantiene sincronizada con el PasswordBox.

    // Uso en XAML:
    //   <PasswordBox h:PasswordBoxHelper.BoundPassword="{Binding Password, Mode=TwoWay}"/>
    public static class PasswordBoxHelper
    {
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BoundPassword",
                typeof(string),
                typeof(PasswordBoxHelper),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AlCambiarBoundPassword));

        public static string GetBoundPassword(DependencyObject objeto) =>
            (string)objeto.GetValue(BoundPasswordProperty);

        public static void SetBoundPassword(DependencyObject objeto, string valor) =>
            objeto.SetValue(BoundPasswordProperty, valor);

        // ViewModel -> PasswordBox: cuando cambia la propiedad enlazada, se actualiza el control.
        private static void AlCambiarBoundPassword(DependencyObject objeto, DependencyPropertyChangedEventArgs e)
        {
            if (objeto is not PasswordBox caja) return;

            // Nos desuscribimos un momento para no provocar un bucle de avisos.
            caja.PasswordChanged -= AlCambiarPassword;

            string nueva = (string?)e.NewValue ?? "";
            if (caja.Password != nueva)
                caja.Password = nueva;

            caja.PasswordChanged += AlCambiarPassword;
        }

        // PasswordBox -> ViewModel: cuando el usuario escribe, se actualiza la propiedad enlazada.
        private static void AlCambiarPassword(object sender, RoutedEventArgs e)
        {
            var caja = (PasswordBox)sender;
            SetBoundPassword(caja, caja.Password);
        }
    }
}
