using System.Text.Json.Serialization;

namespace Sistema_Gestion.Models
{
    // Clases que representan los JSON que se envían y reciben por el Named Pipe.
    // [JsonPropertyName] fija el nombre exacto que aparece en el JSON (el del guion).

    // JSON que ENVIAMOS a la aplicación de procesamiento
    public class SolicitudLab
    {
        [JsonPropertyName("idSolicitud")]
        public string IdSolicitud { get; set; } = "";

        [JsonPropertyName("muestra")]
        public MuestraJson Muestra { get; set; } = new();

        [JsonPropertyName("pruebas")]
        public List<PruebaSolicitadaJson> Pruebas { get; set; } = new();
    }

    public class MuestraJson
    {
        [JsonPropertyName("idMuestra")]
        public string IdMuestra { get; set; } = "";

        [JsonPropertyName("tipo")]
        public string Tipo { get; set; } = "";

        [JsonPropertyName("valida")]
        public bool Valida { get; set; }
    }

    public class PruebaSolicitadaJson
    {
        [JsonPropertyName("codigo")]
        public string Codigo { get; set; } = "";

        [JsonPropertyName("valorMedido")]
        public decimal ValorMedido { get; set; }

        [JsonPropertyName("unidad")]
        public string Unidad { get; set; } = "";
    }

    // JSON que RECIBIMOS
    public class RespuestaLab
    {
        [JsonPropertyName("idSolicitud")]
        public string IdSolicitud { get; set; } = "";

        [JsonPropertyName("estado")]
        public string Estado { get; set; } = "";

        [JsonPropertyName("resultados")]
        public List<ResultadoLab> Resultados { get; set; } = new();

        // Detalle del error si la solicitud no se pudo procesar (puede venir vacío).
        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }

    public class ResultadoLab
    {
        [JsonPropertyName("codigo")]
        public string Codigo { get; set; } = "";

        [JsonPropertyName("valorMedido")]
        public decimal ValorMedido { get; set; }

        [JsonPropertyName("unidad")]
        public string Unidad { get; set; } = "";

        [JsonPropertyName("clasificacion")]
        public string Clasificacion { get; set; } = "";
    }
}
