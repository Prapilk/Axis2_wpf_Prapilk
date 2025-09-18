using System;
using System.IO;
using System.Collections.Generic;
using Axis2.WPF.Mvvm;
using Axis2.WPF.Models;
using Axis2.WPF.Services;


namespace Axis2.WPF
{
    public class FileManager
    {
        public UopFileReader[] AnimationFrameUop { get; private set; }
        public UopFileReader? AnimationSequenceUop { get; private set; }
        public UopFileReader? ArtUopReader { get; private set; }

        public FileManager(Dictionary<string, string> uopFilePaths)
        {
            AnimationFrameUop = new UopFileReader[Constants.MAX_ANIMATION_FRAME_UOP_FILES];
            Logger.Log("DEBUG: Loading UOP files...");

            // Load artLegacyMUL.uop
            if (uopFilePaths.TryGetValue("artLegacyMUL.uop", out string? artUopPath) && artUopPath != null)
            {
                ArtUopReader = new UopFileReader(artUopPath);
                if (!ArtUopReader.Load())
                {
                    Logger.Log($"ERROR: Failed to load artLegacyMUL.uop from {artUopPath}");
                }
                else
                {
                    Logger.Log($"DEBUG: Successfully loaded artLegacyMUL.uop from {artUopPath}");
                }
            }
            else
            {
                Logger.Log("WARNING: artLegacyMUL.uop path not provided.");
            }

            // Load AnimationFrameX.uop files
            for (int i = 1; i < Constants.MAX_ANIMATION_FRAME_UOP_FILES; i++)
            {
                string animationFrameName = $"AnimationFrame{i}.uop";
                if (uopFilePaths.TryGetValue(animationFrameName, out string? filePath) && filePath != null)
                {
                    AnimationFrameUop[i] = new UopFileReader(filePath);
                    if (!AnimationFrameUop[i].Load())
                    {
                        Logger.Log($"ERROR: Failed to load {animationFrameName} from {filePath}");
                    }
                    else
                    {
                        Logger.Log($"DEBUG: Successfully loaded {animationFrameName} from {filePath}");
                    }
                }
                else
                {
                    Logger.Log($"WARNING: {animationFrameName} path not provided.");
                }
            }

            // Load AnimationSequence.uop
            string animationSequenceName = "AnimationSequence.uop";
            if (uopFilePaths.TryGetValue(animationSequenceName, out string? animationSequencePath) && animationSequencePath != null)
            {
                AnimationSequenceUop = new UopFileReader(animationSequencePath);
                if (!AnimationSequenceUop.Load())
                {
                    Logger.Log($"ERROR: Failed to load {animationSequenceName} from {animationSequencePath}");
                }
                else
                {
                    Logger.Log($"DEBUG: Successfully loaded {animationSequenceName} from {animationSequencePath}");
                }
            }
            else
            {
                Logger.Log($"WARNING: {animationSequenceName} path not provided.");
            }

            Logger.Log("DEBUG: UOP files loading complete.");
        }

        // This method is a placeholder for g_FileManager.GetIndexData
        // In C++, it reads data from a file based on IndexDataFileInfo and UopDataHeader.
        // In C#, this would involve reading from the UopFileReader's stream.
        public byte[]? GetIndexData(IndexDataFileInfo info, UopDataHeader uopHeader)
        {
            if (info == null || info.File == null || info.File.FilePath == null || uopHeader.Offset == 0 || uopHeader.DecompressedSize == 0)
            {
                return null;
            }

            // Assuming the UopFileReader has access to the underlying stream
            // and can seek to the correct offset and read the data.
            // This is a simplified representation. Decompression would also happen here.
            try
            {
                using (var stream = new FileStream(info.File.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    stream.Seek((long)uopHeader.Offset, SeekOrigin.Begin);
                    byte[] buffer = new byte[uopHeader.DecompressedSize];
                    stream.Read(buffer, 0, (int)uopHeader.DecompressedSize);
                    return buffer;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading index data: {ex.Message}");
                return null;
            }
        }

        // Placeholder for GetCurrentDataSize
        public int GetCurrentDataSize(UopDataHeader uopHeader)
        {
            return (int)uopHeader.DecompressedSize;
        }
    }
}