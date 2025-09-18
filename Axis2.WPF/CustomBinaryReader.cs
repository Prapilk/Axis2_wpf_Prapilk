using System.IO;
using System.Text;

namespace Axis2.WPF
{
    public class CustomBinaryReader : BinaryReader
    {
        public CustomBinaryReader(Stream input) : base(input)
        {
        }

        public CustomBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
        {
        }


        public CustomBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
        }

        public uint ReadUInt32LE()
        {
            byte[] bytes = ReadBytes(4);
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToUInt32(bytes, 0);
            }
            else
            {
                Array.Reverse(bytes);
                return BitConverter.ToUInt32(bytes, 0);
            }
        }

        public int ReadInt32LE()
        {
            byte[] bytes = ReadBytes(4);
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToInt32(bytes, 0);
            }
            else
            {
                Array.Reverse(bytes);
                return BitConverter.ToInt32(bytes, 0);
            }
        }

        public void Move(int bytes)
        {
            BaseStream.Seek(bytes, SeekOrigin.Current);
        }
    }
}
