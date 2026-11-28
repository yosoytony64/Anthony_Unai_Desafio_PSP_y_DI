using System.Text.Json.Serialization;

namespace Hospital_procesos.Models
{
    // Modelo para el historial de peticiones, enlazable al DataGrid de la interfaz y persistido en JSON.
    public class PeticionRegistro
    {
        // 1. Columnas obligatorias del DataGrid
        [JsonPropertyName("idPeticion")]
        public string IdPeticion { get; set; } = string.Empty;

        [JsonPropertyName("fechaHora")]
        public DateTime FechaHora { get; set; } = DateTime.Now;

        [JsonIgnore]
        public string FechaHoraFormateada => FechaHora.ToString("dd/MM/yyyy HH:mm:ss");

        [JsonPropertyName("tipoOperacion")]
        public string TipoOperacion { get; set; } = "Análisis de Laboratorio";

        [JsonPropertyName("estadoFinal")]
        public string EstadoFinal { get; set; } = string.Empty;

        [JsonPropertyName("errorTecnico")]
        public string? ErrorTecnico { get; set; }

        // 2. Información detallada para el panel de selección
        [JsonPropertyName("idMuestra")]
        public string IdMuestra { get; set; } = string.Empty;

        [JsonPropertyName("tipoMuestra")]
        public string TipoMuestra { get; set; } = string.Empty;

        [JsonPropertyName("muestraValida")]
        public bool MuestraValida { get; set; }

        [JsonPropertyName("jsonRecibido")]
        public string JsonRecibido { get; set; } = string.Empty;

        [JsonPropertyName("jsonRespuesta")]
        public string JsonRespuesta { get; set; } = string.Empty;

        [JsonPropertyName("pruebasRecibidas")]
        public List<PruebaSolicitadaJson> PruebasRecibidas { get; set; } = new();

        [JsonPropertyName("resultadosCalculados")]
        public List<ResultadoLab> ResultadosCalculados { get; set; } = new();

        [JsonIgnore]
        public bool EsCorrecta => EstadoFinal.Equals("Procesada correctamente", StringComparison.OrdinalIgnoreCase);

        [JsonIgnore]
        public bool EsError => !EsCorrecta;
    }
}
