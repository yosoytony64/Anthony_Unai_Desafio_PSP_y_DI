namespace Sistema_Gestion.Models
{
    // Una prueba añadida a la solicitud antes de enviarla (cada elemento del ListBox).
    public class LineaPrueba
    {
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public decimal Valor { get; set; }
        public string Unidad { get; set; } = "";

        // Texto que se ve en el ListBox.
        public string Texto => $"{Codigo} - {Nombre}: {Valor} {Unidad}";
    }
}
