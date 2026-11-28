using System.Windows;
using Sistema_Gestion.ViewModels;

namespace Sistema_Gestion.Views
{
    // Ventana de inicio de sesión. Solo conecta con su ViewModel y abre la ventana principal.
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            var viewModel = new LoginViewModel();
            viewModel.AccesoConcedido += AbrirVentanaPrincipal;
            DataContext = viewModel;
        }

        private void AbrirVentanaPrincipal()
        {
            var principal = new MainWindow();
            Application.Current.MainWindow = principal;
            principal.Show();
            Close();
        }
    }
}