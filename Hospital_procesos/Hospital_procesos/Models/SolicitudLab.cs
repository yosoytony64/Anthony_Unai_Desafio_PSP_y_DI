using System.Text.Json.Serialization;

namespace Hospital_procesos.Models
{
    // Representa la solicitud de análisis enviada desde la Aplicación 1 (Sistema_Gestion).
    public class SolicitudLab
    {
        [JsonPropertyName("idSolicitud")]
        public string IdSolicitud { get; set; } = string.Empty;

        [JsonPropertyName("muestra")]
        public MuestraJson Muestra { get; set; } = new();

        [JsonPropertyName("pruebas")]
        public List<PruebaSolicitadaJson> Pruebas { get; set; } = new();
    }

    // Datos descriptivos de la muestra recibida.
    public class MuestraJson
    {
        [JsonPropertyName("idMuestra")]
        public string IdMuestra { get; set; } = string.Empty;

        [JsonPropertyName("tipo")]
        public string Tipo { get; set; } = string.Empty;

        [JsonPropertyName("valida")]
        public bool Valida { get; set; }
    }

    // Detalle de una prueba individual dentro de la solicitud.
    public class PruebaSolicitadaJson
    {
        [JsonPropertyName("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [JsonPropertyName("valorMedido")]
        public decimal ValorMedido { get; set; }

        [JsonPropertyName("unidad")]
        public string Unidad { get; set; } = string.Empty;
    }
}
