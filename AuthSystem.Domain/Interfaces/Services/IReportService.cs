using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuthSystem.Domain.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de generación de reportes
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Genera un reporte basado en el tipo y parámetros especificados
        /// </summary>
        /// <param name="reportType">Tipo de reporte a generar</param>
        /// <param name="parameters">Parámetros para la generación del reporte</param>
        /// <param name="outputFormat">Formato de salida del reporte (PDF, Excel, etc.)</param>
        /// <returns>Resultado del reporte generado</returns>
        Task<ReportResult> GenerateReportAsync(string reportType, Dictionary<string, object> parameters, string outputFormat = "PDF");
    }

    /// <summary>
    /// Resultado de la generación de un reporte
    /// </summary>
    public class ReportResult
    {
        /// <summary>
        /// Datos binarios del reporte generado
        /// </summary>
        public byte[] ReportData { get; set; }

        /// <summary>
        /// Tipo de contenido MIME del reporte
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Nombre del archivo del reporte
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Tamaño del reporte en bytes
        /// </summary>
        public long Size => ReportData?.Length ?? 0;

        /// <summary>
        /// Metadatos adicionales del reporte
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
}
