using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AuthSystem.Infrastructure.Services
{
    /// <summary>
    /// Clase de utilidad para comprimir y descomprimir datos de caché
    /// </summary>
    public static class CacheCompression
    {
        /// <summary>
        /// Umbral de tamaño en bytes a partir del cual se comprimirán los datos
        /// </summary>
        private const int CompressionThreshold = 1024; // 1KB
        
        /// <summary>
        /// Prefijo para identificar datos comprimidos
        /// </summary>
        private const string CompressedPrefix = "COMPRESSED:";
        
        /// <summary>
        /// Comprime un objeto si supera el umbral de tamaño
        /// </summary>
        /// <typeparam name="T">Tipo del objeto</typeparam>
        /// <param name="value">Objeto a comprimir</param>
        /// <returns>Cadena comprimida o serializada</returns>
        public static string CompressIfNeeded<T>(T value)
        {
            if (value == null)
            {
                return null;
            }
            
            // Serializar el objeto
            string serialized = JsonSerializer.Serialize(value);
            byte[] serializedBytes = Encoding.UTF8.GetBytes(serialized);
            
            // Si el tamaño es menor que el umbral, no comprimir
            if (serializedBytes.Length < CompressionThreshold)
            {
                return serialized;
            }
            
            // Comprimir los datos
            byte[] compressedBytes = Compress(serializedBytes);
            
            // Convertir a Base64 y añadir prefijo
            return CompressedPrefix + Convert.ToBase64String(compressedBytes);
        }
        
        /// <summary>
        /// Descomprime una cadena si está comprimida
        /// </summary>
        /// <typeparam name="T">Tipo del objeto</typeparam>
        /// <param name="value">Cadena a descomprimir</param>
        /// <returns>Objeto descomprimido</returns>
        public static T DecompressIfNeeded<T>(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return default;
            }
            
            // Verificar si está comprimido
            if (value.StartsWith(CompressedPrefix))
            {
                // Extraer la parte Base64
                string base64 = value.Substring(CompressedPrefix.Length);
                byte[] compressedBytes = Convert.FromBase64String(base64);
                
                // Descomprimir
                byte[] decompressedBytes = Decompress(compressedBytes);
                string decompressed = Encoding.UTF8.GetString(decompressedBytes);
                
                // Deserializar
                return JsonSerializer.Deserialize<T>(decompressed);
            }
            
            // No está comprimido, deserializar directamente
            return JsonSerializer.Deserialize<T>(value);
        }
        
        /// <summary>
        /// Comprime un array de bytes usando GZip
        /// </summary>
        /// <param name="data">Datos a comprimir</param>
        /// <returns>Datos comprimidos</returns>
        private static byte[] Compress(byte[] data)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(outputStream, CompressionLevel.Optimal))
                {
                    gzipStream.Write(data, 0, data.Length);
                }
                return outputStream.ToArray();
            }
        }
        
        /// <summary>
        /// Descomprime un array de bytes usando GZip
        /// </summary>
        /// <param name="data">Datos comprimidos</param>
        /// <returns>Datos descomprimidos</returns>
        private static byte[] Decompress(byte[] data)
        {
            using (var inputStream = new MemoryStream(data))
            using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                gzipStream.CopyTo(outputStream);
                return outputStream.ToArray();
            }
        }
        
        /// <summary>
        /// Comprime un objeto de forma asíncrona si supera el umbral de tamaño
        /// </summary>
        /// <typeparam name="T">Tipo del objeto</typeparam>
        /// <param name="value">Objeto a comprimir</param>
        /// <returns>Cadena comprimida o serializada</returns>
        public static async Task<string> CompressIfNeededAsync<T>(T value)
        {
            if (value == null)
            {
                return null;
            }
            
            // Serializar el objeto
            string serialized = JsonSerializer.Serialize(value);
            byte[] serializedBytes = Encoding.UTF8.GetBytes(serialized);
            
            // Si el tamaño es menor que el umbral, no comprimir
            if (serializedBytes.Length < CompressionThreshold)
            {
                return serialized;
            }
            
            // Comprimir los datos de forma asíncrona
            byte[] compressedBytes = await CompressAsync(serializedBytes);
            
            // Convertir a Base64 y añadir prefijo
            return CompressedPrefix + Convert.ToBase64String(compressedBytes);
        }
        
        /// <summary>
        /// Descomprime una cadena de forma asíncrona si está comprimida
        /// </summary>
        /// <typeparam name="T">Tipo del objeto</typeparam>
        /// <param name="value">Cadena a descomprimir</param>
        /// <returns>Objeto descomprimido</returns>
        public static async Task<T> DecompressIfNeededAsync<T>(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return default;
            }
            
            // Verificar si está comprimido
            if (value.StartsWith(CompressedPrefix))
            {
                // Extraer la parte Base64
                string base64 = value.Substring(CompressedPrefix.Length);
                byte[] compressedBytes = Convert.FromBase64String(base64);
                
                // Descomprimir de forma asíncrona
                byte[] decompressedBytes = await DecompressAsync(compressedBytes);
                string decompressed = Encoding.UTF8.GetString(decompressedBytes);
                
                // Deserializar
                return JsonSerializer.Deserialize<T>(decompressed);
            }
            
            // No está comprimido, deserializar directamente
            return JsonSerializer.Deserialize<T>(value);
        }
        
        /// <summary>
        /// Comprime un array de bytes de forma asíncrona usando GZip
        /// </summary>
        /// <param name="data">Datos a comprimir</param>
        /// <returns>Datos comprimidos</returns>
        private static async Task<byte[]> CompressAsync(byte[] data)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(outputStream, CompressionLevel.Optimal))
                {
                    await gzipStream.WriteAsync(data, 0, data.Length);
                }
                return outputStream.ToArray();
            }
        }
        
        /// <summary>
        /// Descomprime un array de bytes de forma asíncrona usando GZip
        /// </summary>
        /// <param name="data">Datos comprimidos</param>
        /// <returns>Datos descomprimidos</returns>
        private static async Task<byte[]> DecompressAsync(byte[] data)
        {
            using (var inputStream = new MemoryStream(data))
            using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                await gzipStream.CopyToAsync(outputStream);
                return outputStream.ToArray();
            }
        }
    }
}
