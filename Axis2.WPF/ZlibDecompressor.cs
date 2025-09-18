using System;
using System.IO;
using System.IO.Compression;
using Axis2.WPF.Services;

namespace Axis2.WPF
{
    public static class ZlibDecompressor
    {
        public static byte[]? Decompress(byte[] compressedData, int decompressedSize)
        {
            if (compressedData == null || compressedData.Length == 0)
                return null;

            try
            {
                using (var compressedStream = new MemoryStream(compressedData))
                {
                    using (var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
                    using (var decompressedStream = new MemoryStream())
                    {
                        deflateStream.CopyTo(decompressedStream);
                        return decompressedStream.ToArray();
                    }
                }
            }
            catch (Exception) // Removed unused 'ex' variable
            {
                //Logger.Log($"ZlibDecompressor: Erreur lors de la décompression des données: {ex.Message}");
                return null;
            }
        }
    }
}