using System;
using System.IO;

namespace AuthSystem.Domain.Models
{
    /// <summary>
    /// Modelo para representar un archivo adjunto en un correo electrónico
    /// </summary>
    public class EmailAttachment
    {
        /// <summary>
        /// Nombre del archivo adjunto
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Ruta del archivo en el sistema de archivos
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Contenido del archivo como array de bytes
        /// </summary>
        public byte[] Content { get; set; }

        /// <summary>
        /// Tipo de contenido MIME
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Crea un adjunto a partir de una ruta de archivo
        /// </summary>
        /// <param name="filePath">Ruta del archivo</param>
        /// <returns>Adjunto de correo electrónico</returns>
        public static EmailAttachment FromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("La ruta del archivo no puede ser nula o vacía", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("No se encontró el archivo especificado", filePath);

            return new EmailAttachment
            {
                FileName = Path.GetFileName(filePath),
                FilePath = filePath,
                Content = File.ReadAllBytes(filePath),
                ContentType = GetContentType(filePath)
            };
        }

        /// <summary>
        /// Crea un adjunto a partir de un array de bytes
        /// </summary>
        /// <param name="fileName">Nombre del archivo</param>
        /// <param name="content">Contenido del archivo</param>
        /// <param name="contentType">Tipo de contenido MIME</param>
        /// <returns>Adjunto de correo electrónico</returns>
        public static EmailAttachment FromBytes(string fileName, byte[] content, string contentType = null)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("El nombre del archivo no puede ser nulo o vacío", nameof(fileName));

            if (content == null || content.Length == 0)
                throw new ArgumentException("El contenido del archivo no puede ser nulo o vacío", nameof(content));

            return new EmailAttachment
            {
                FileName = fileName,
                Content = content,
                ContentType = contentType ?? GetContentType(fileName)
            };
        }

        /// <summary>
        /// Obtiene el tipo de contenido MIME basado en la extensión del archivo
        /// </summary>
        /// <param name="fileName">Nombre o ruta del archivo</param>
        /// <returns>Tipo de contenido MIME</returns>
        private static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                _ => "application/octet-stream"
            };
        }
    }
}
