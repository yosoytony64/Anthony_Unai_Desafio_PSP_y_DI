using System.Windows;
using Sistema_Gestion.ViewModels;
using Sistema_Gestion.Views;

namespace Sistema_Gestion
{
    // Code-behind mínimo: conecta la ventana con su ViewModel y vuelve al login al cerrar sesión.
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var viewModel = new MainViewModel();
            viewModel.CerrarSesionSolicitado += VolverAlLogin;
            DataContext = viewModel;
        }

        private void VolverAlLogin()
        {
            var login = new LoginWindow();
            Application.Current.MainWindow = login;
            login.Show();
            Close();
        }
    }
}   