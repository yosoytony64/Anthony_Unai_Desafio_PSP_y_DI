using Sistema_Gestion.Helpers;

namespace Sistema_Gestion.Models
{
    // Tabla 'pruebas'. 'Codigo' (GLU, COL, TRI...) es el que usa la app 2 en el JSON.
    public class Prueba : ObservableObject
    {
        private int _idPrueba;
        private string _codigo = "";
        private string _nombrePrueba = "";
        private string? _descripcion = "";
        private string? _valoresReferencia = "";
        private decimal? _precio;
        private int _idDepartamento;

        public int IdPrueba { get => _idPrueba; set => SetProperty(ref _idPrueba, value); }
        public string Codigo { get => _codigo; set => SetProperty(ref _codigo, value); }
        public string NombrePrueba { get => _nombrePrueba; set => SetProperty(ref _nombrePrueba, value); }
        public string? Descripcion { get => _descripcion; set => SetProperty(ref _descripcion, value); }
        public string? ValoresReferencia { get => _valoresReferencia; set => SetProperty(ref _valoresReferencia, value); }
        public decimal? Precio { get => _precio; set => SetProperty(ref _precio, value); }
        public int IdDepartamento { get => _idDepartamento; set => SetProperty(ref _idDepartamento, value); }
    }
}
