using System;
using System.Collections.Generic;

namespace Edmosoft.IO.vbaCompression
{
    public class vbaStreamReader
    {
         class Header
        {
            public UInt16 CompressedChunkSize { get; }
            public bool CompressedChunkFlag { get; }
            public Header(Edmosoft.IO.StreamReader streamReader)
            {
                UInt16 Header = streamReader.ReadUInt16();
                CompressedChunkSize = (UInt16)((Header & 0x0FFF) + 3);
                CompressedChunkFlag = (Header & 0x8000) == 0x8000;
            }
        }

        static public System.IO.Stream Decode(Edmosoft.IO.StreamReader streamReader)
        {
            List<byte> ret = new List<byte>();
            byte SignatureByte = streamReader.ReadByte(); //must be 0x01
            if (SignatureByte == 0x01)
            {
                do
                {
                    ret.AddRange(DecompressCompressedChunk(streamReader));
                } while (streamReader.DataAvailable);
            }
            else
            {
                throw new FormatException("Unexpected byte value in stream");
            }
            var ms = new System.IO.MemoryStream(ret.ToArray());
            ms.Position = 0;
            return ms;
        }

        static byte[] DecompressCompressedChunk(Edmosoft.IO.StreamReader streamReader)
        {
            var CompressedChunkStart = streamReader.BaseStream.Position;
            List<byte> Ret = new List<byte>();
            Header header = new Header(streamReader);
            var CompressedEnd = Math.Min(streamReader.BaseStream.Length, CompressedChunkStart + header.CompressedChunkSize);
            if (header.CompressedChunkFlag)
            {
                do
                {
                    byte FlagByte = streamReader.ReadByte();
                    for (int i = 0; i <= 7; i++)
                    {
                        if (streamReader.BaseStream.Position < CompressedEnd)
                        {
                            bool FlagBit = ((FlagByte >> i) & 0x1) == 0x1;
                            if (FlagBit)
                                CopyToken(streamReader, ref Ret);
                            else
                                Ret.Add(streamReader.ReadByte());
                        }
                    }
                } while (streamReader.BaseStream.Position < CompressedEnd);
            }
            else
            {
                Ret.AddRange(streamReader.ReadBlock(4096));
            }
            return Ret.ToArray();
        }

        static void CopyToken(Edmosoft.IO.StreamReader streamReader, ref List<byte> ret)
        {
            UInt16 Token = streamReader.ReadUInt16();
            UInt16 BitCount = Math.Max((UInt16)Math.Ceiling(Math.Log(ret.Count, 2)), (UInt16)4);
            UInt16 LengthMask = (UInt16)(0xFFFF >> BitCount);
            UInt16 OffsetMask = (UInt16)~LengthMask;
            UInt16 MaximumLength = (UInt16)(LengthMask + 3);
            UInt16 Length = (UInt16)((Token & LengthMask) + 3);
            UInt16 MaskedOffset = (UInt16)(Token & OffsetMask);
            UInt16 OffsetShitf = (UInt16)(16 - BitCount);
            UInt16 Offset = (UInt16)((MaskedOffset >> OffsetShitf) + 1);
            //length may be longer than offset,
            //This is intentional, and can be used to copy bytes twice into the destination stream
            //But this means we can't use shortcut functions like list.AddRange(list.GetRange())
            for (int i = 0; i < Length; i++)
            {
                UInt16 CopySource = (UInt16)(ret.Count - Offset); //This value needs to slide as we add 
                ret.Add(ret[CopySource]);
            }
        }
    }
}
