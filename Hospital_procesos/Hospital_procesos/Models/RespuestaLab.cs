using System.Text.Json.Serialization;

namespace Hospital_procesos.Models
{
    // Estructura de la respuesta devuelta a la Aplicación 1 a través del Named Pipe.
    public class RespuestaLab
    {
        [JsonPropertyName("idSolicitud")]
        public string IdSolicitud { get; set; } = string.Empty;

        [JsonPropertyName("estado")]
        public string Estado { get; set; } = string.Empty;

        [JsonPropertyName("resultados")]
        public List<ResultadoLab> Resultados { get; set; } = new();

        [JsonPropertyName("error")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Error { get; set; }
    }

    // Resultado procesado y clasificado de una prueba individual.
    public class ResultadoLab
    {
        [JsonPropertyName("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [JsonPropertyName("valorMedido")]
        public decimal ValorMedido { get; set; }

        [JsonPropertyName("unidad")]
        public string Unidad { get; set; } = string.Empty;

        [JsonPropertyName("clasificacion")]
        public string Clasificacion { get; set; } = string.Empty;
    }
}
