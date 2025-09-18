using System;
using System.Runtime.InteropServices;

namespace Axis2.WPF.Services
{
    public class Zlib
    {
        #region Version
        [DllImport("Zlib64", EntryPoint = "zlibVersion")]
        private static extern string zlibVersion64();

        [DllImport("Zlib32", EntryPoint = "zlibVersion")]
        private static extern string zlibVersion();

        public static string Version
        {
            get
            {
                if (SystemInfo.IsX64)
                    return zlibVersion64();

                return zlibVersion();
            }
        }
        #endregion

        #region Decompress
        [DllImport("Zlib64", EntryPoint = "uncompress")]
        private static extern ZLibError uncompress64(byte[] dest, ref int destLen, byte[] source, int sourceLen);

        [DllImport("Zlib32", EntryPoint = "uncompress")]
        private static extern ZLibError uncompress(byte[] dest, ref int destLen, byte[] source, int sourceLen);

        public static ZLibError Decompress(byte[] dest, ref int destLength, byte[] source, int sourceLength)
        {
            if (SystemInfo.IsX64)
                return uncompress64(dest, ref destLength, source, sourceLength);

            return uncompress(dest, ref destLength, source, sourceLength);
        }
        #endregion

        #region Compress
        [DllImport("Zlib64", EntryPoint = "compress")]
        private static extern ZLibError compress64(byte[] dest, ref int destLen, byte[] source, int sourceLen);

        [DllImport("Zlib32", EntryPoint = "compress")]
        private static extern ZLibError compress(byte[] dest, ref int destLen, byte[] source, int sourceLen);

        public static ZLibError Compress(byte[] dest, ref int destLength, byte[] source, int sourceLength)
        {
            if (SystemInfo.IsX64)
                return compress64(dest, ref destLength, source, sourceLength);

            return compress(dest, ref destLength, source, sourceLength);
        }

        [DllImport("Zlib64", EntryPoint = "compress2")]
        private static extern ZLibError compress264(byte[] dest, ref int destLen, byte[] source, int sourceLen, ZLibQuality quality);

        [DllImport("Zlib32", EntryPoint = "compress2")]
        private static extern ZLibError compress2(byte[] dest, ref int destLen, byte[] source, int sourceLen, ZLibQuality quality);

        public static ZLibError Compress(byte[] dest, ref int destLength, byte[] source, int sourceLength, ZLibQuality quality)
        {
            if (SystemInfo.IsX64)
                return compress264(dest, ref destLength, source, sourceLength, quality);

            return compress2(dest, ref destLength, source, sourceLength, quality);
        }
        #endregion

        #region Adler32
        [DllImport("Zlib64", EntryPoint = "adler32")]
        private static extern uint adler3264(uint adler, byte[] buf, int len);

        [DllImport("Zlib32", EntryPoint = "adler32")]
        private static extern uint adler32(uint adler, byte[] buf, int len);

        public static uint Adler32(uint adler, byte[] buf, int len)
        {
            if (SystemInfo.IsX64)
                return adler3264(adler, buf, len);

            return adler32(adler, buf, len);
        }
        #endregion
    }
}