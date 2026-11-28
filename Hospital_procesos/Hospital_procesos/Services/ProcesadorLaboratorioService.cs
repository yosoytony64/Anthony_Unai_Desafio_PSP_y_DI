using Hospital_procesos.Models;

namespace Hospital_procesos.Services
{
    // Servicio encargado de validar las solicitudes médicas y realizar el cálculo de clasificación
    // en base a los intervalos de referencia de las pruebas de laboratorio del hospital.
    public class ProcesadorLaboratorioService
    {
        // Definición de intervalos de referencia basados en el catálogo clínico de la base de datos:
        // GLU: 70-100 mg/dL
        // COL: < 200 mg/dL
        // TRI: < 150 mg/dL
        // HBA: 4.0-5.6 %
        // ECG: 60-100 lpm
        private record RangoReferencia(decimal Min, decimal Max, string UnidadEsperada);

        private static readonly Dictionary<string, RangoReferencia> IntervalosConocidos = new(StringComparer.OrdinalIgnoreCase)
        {
            ["GLU"] = new RangoReferencia(70m, 100m, "mg/dL"),
            ["COL"] = new RangoReferencia(0m, 200m, "mg/dL"),
            ["TRI"] = new RangoReferencia(0m, 150m, "mg/dL"),
            ["HBA"] = new RangoReferencia(4.0m, 5.6m, "%"),
            ["ECG"] = new RangoReferencia(60m, 100m, "lpm")
        };

        // Procesa la solicitud validando todos los requisitos del enunciado y generando la respuesta JSON esperada.
        public RespuestaLab Procesar(SolicitudLab? solicitud, out string? errorValidacion)
        {
            errorValidacion = null;

            // 1. Validación de existencia básica de la solicitud
            if (solicitud == null || string.IsNullOrWhiteSpace(solicitud.IdSolicitud))
            {
                errorValidacion = "El identificador de solicitud (idSolicitud) es obligatorio.";
                return CrearRespuestaError(solicitud?.IdSolicitud ?? "DESCONOCIDO", errorValidacion);
            }

            // 2. Validación de la muestra
            if (solicitud.Muestra == null || string.IsNullOrWhiteSpace(solicitud.Muestra.IdMuestra))
            {
                errorValidacion = "La muestra es obligatoria y debe incluir un identificador válido (idMuestra).";
                return CrearRespuestaError(solicitud.IdSolicitud, errorValidacion);
            }

            // Si la muestra está marcada como no válida (muestra alterada, hemolizada, etc.)
            if (!solicitud.Muestra.Valida)
            {
                errorValidacion = "La muestra recibida no es válida.";
                return CrearRespuestaError(solicitud.IdSolicitud, errorValidacion);
            }

            // 3. Validación de la existencia de pruebas solicitadas
            if (solicitud.Pruebas == null || solicitud.Pruebas.Count == 0)
            {
                errorValidacion = "La solicitud no contiene ninguna prueba para analizar.";
                return CrearRespuestaError(solicitud.IdSolicitud, errorValidacion);
            }

            // 4. Procesamiento y cálculo de cada prueba solicitada
            var resultados = new List<ResultadoLab>();

            foreach (var prueba in solicitud.Pruebas)
            {
                string codigo = (prueba.Codigo ?? string.Empty).Trim().ToUpperInvariant();

                // Validación de código de prueba reconocido
                if (!IntervalosConocidos.TryGetValue(codigo, out var rango))
                {
                    errorValidacion = $"El código de prueba '{prueba.Codigo}' no es reconocido por el laboratorio.";
                    return CrearRespuestaError(solicitud.IdSolicitud, errorValidacion);
                }

                // Validación de valores numéricos coherentes (no negativos)
                if (prueba.ValorMedido < 0)
                {
                    errorValidacion = $"El valor medido para la prueba {codigo} ({prueba.ValorMedido}) no puede ser negativo.";
                    return CrearRespuestaError(solicitud.IdSolicitud, errorValidacion);
                }

                // Cálculo real de clasificación según los intervalos clínicos
                string clasificacion = Clasificar(codigo, prueba.ValorMedido, rango);

                resultados.Add(new ResultadoLab
                {
                    Codigo = codigo,
                    ValorMedido = prueba.ValorMedido,
                    Unidad = string.IsNullOrWhiteSpace(prueba.Unidad) ? rango.UnidadEsperada : prueba.Unidad,
                    Clasificacion = clasificacion
                });
            }

            // Solicitud procesada exitosamente
            return new RespuestaLab
            {
                IdSolicitud = solicitud.IdSolicitud,
                Estado = "Procesada correctamente",
                Resultados = resultados,
                Error = null
            };
        }

        // Determina si el valor medido está dentro o fuera del intervalo clínico de referencia.
        private static string Clasificar(string codigo, decimal valor, RangoReferencia rango)
        {
            if (codigo == "COL" || codigo == "TRI")
            {
                // Para colesterol y triglicéridos el criterio clínico estándar es no superar el límite superior
                return valor <= rango.Max ? "Dentro del intervalo" : "Fuera del intervalo";
            }

            // Para glucosa, hemoglobina y electrocardiograma se evalúa rango cerrado [Min, Max]
            return (valor >= rango.Min && valor <= rango.Max)
                ? "Dentro del intervalo"
                : "Fuera del intervalo";
        }

        private static RespuestaLab CrearRespuestaError(string idSolicitud, string mensajeError)
        {
            return new RespuestaLab
            {
                IdSolicitud = idSolicitud,
                Estado = "Error de validación",
                Resultados = new List<ResultadoLab>(),
                Error = mensajeError
            };
        }
    }
}
