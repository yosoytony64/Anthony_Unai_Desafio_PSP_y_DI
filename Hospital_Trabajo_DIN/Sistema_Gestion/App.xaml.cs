using System.Configuration;
using System.Data;
using System.Windows;
using Sistema_Gestion.Data;

namespace Sistema_Gestion
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (!HospitalContext.ProbarConexion(out string error))
            {
                MessageBox.Show($"No se pudo conectar con MySQL:\n{error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

    }
    }
    
