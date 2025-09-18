using System.IO;
using Axis2.WPF.Services;
using System;

namespace Axis2.WPF
{
    public class IndexDataFileInfo
    {
        public UopFileReader File { get; private set; }
        public UopDataHeader UopHeader { get; private set; }
        public int FrameCount { get; private set; }

        public IndexDataFileInfo(UopFileReader file, UopDataHeader uopHeader)
        {
            File = file;
            UopHeader = uopHeader;
            FrameCount = 0; // Default value
        }

        public byte[]? GetData()
        {
            if (File == null)
            {
                Logger.Log("IndexDataFileInfo.GetData: File is null.");
                return null;
            }
            if (!File.IsLoaded)
            {
                Logger.Log($"IndexDataFileInfo.GetData: File {File.FilePath} is not loaded.");
                return null;
            }
            if (UopHeader.DecompressedSize == 0)
            {
               // Logger.Log($"IndexDataFileInfo.GetData: UopHeader.DecompressedSize is 0 for hash {UopHeader.Hash:X16}.");
                return null;
            }
            try
            {
                return File.ReadData(UopHeader);
            }
            catch (Exception) // Removed unused 'ex' variable
            {
               // Logger.Log($"IndexDataFileInfo.GetData: Exception during ReadData for hash {UopHeader.Hash:X16}: {ex.Message}");
                return null;
            }
        }

        public void SetFrameCount(int frameCount)
        {
            FrameCount = frameCount;
        }
    }
}