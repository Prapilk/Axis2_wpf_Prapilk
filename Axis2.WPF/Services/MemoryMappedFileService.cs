using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Axis2.WPF.Services
{
    public class MemoryMappedFileService : IDisposable
    {
        private MemoryMappedFile? _mmf;
        private MemoryMappedViewAccessor? _accessor;
        public string FilePath { get; private set; }

        public MemoryMappedFileService(string filePath)
        {
            FilePath = filePath; // Store the file path
            try
            {
                _mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
                _accessor = _mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
                Logger.Log($"MemoryMappedFileService: Opened {filePath} successfully.");
            }
            catch (Exception ex)
            {
                Logger.Log($"MemoryMappedFileService: Error opening {filePath}: {ex.Message}");
                _mmf?.Dispose();
                _accessor?.Dispose();
                _mmf = null;
                _accessor = null;
            }
        }

        public byte ReadByte(long position)
        {
            if (_accessor == null) throw new InvalidOperationException("MemoryMappedFileService: File not open.");
            return _accessor.ReadByte(position);
        }

        public ushort ReadUInt16(long position)
        {
            if (_accessor == null) throw new InvalidOperationException("MemoryMappedFileService: File not open.");
            return _accessor.ReadUInt16(position);
        }

        public int ReadInt32(long position)
        {
            if (_accessor == null) throw new InvalidOperationException("MemoryMappedFileService: File not open.");
            return _accessor.ReadInt32(position);
        }

        public sbyte ReadSByte(long position)
        {
            if (_accessor == null) throw new InvalidOperationException("MemoryMappedFileService: File not open.");
            return _accessor.ReadSByte(position);
        }

        public void ReadArray<T>(long position, T[] array, int offset, int count) where T : struct
        {
            if (_accessor == null) throw new InvalidOperationException("MemoryMappedFileService: File not open.");
            _accessor.ReadArray(position, array, offset, count);
        }

        public long Capacity => _accessor?.Capacity ?? 0;

        public bool IsOpen => _accessor != null;

        public void Dispose()
        {
            _accessor?.Dispose();
            _mmf?.Dispose();
            _accessor = null;
            _mmf = null;
        }
    }
}