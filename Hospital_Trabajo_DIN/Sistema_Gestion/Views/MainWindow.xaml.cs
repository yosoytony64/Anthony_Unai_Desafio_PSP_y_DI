using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Sistema_Gestion.ViewModels;

namespace Sistema_Gestion
{
   
     public partial class MainWindow : Window
        {
            public MainWindow()
            {
                InitializeComponent();
                DataContext = new MainViewModel();
            }
        
    

    private void Procesa(object sender, RoutedEventArgs e)
        {
            
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Gestion_Click(object sender, RoutedEventArgs e)
        {
            // De momento vacío: aquí cargaremos la vista de pacientes.
        }
    }
}