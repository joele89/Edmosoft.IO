using System;
using System.Collections.Generic;

namespace Edmosoft.IO
{
  public class StreamReader
  {
    public ByteOrderMode mode = ByteOrderMode.LE;

    public System.IO.Stream BaseStream;
    public StreamReader(byte[] b)
    {
      BaseStream = new System.IO.MemoryStream(b);
    }
    public StreamReader(System.IO.Stream stream)
    {
      BaseStream = stream;
    }
    public byte ReadByte()
    {
      return (byte)BaseStream.ReadByte();
    }
    public UInt16 ReadUInt16()
    {
      byte[] buffer = new byte[2];
      BaseStream.Read(buffer, 0, 2);
      if (mode == ByteOrderMode.BE) Array.Reverse(buffer);
      return System.BitConverter.ToUInt16(buffer, 0);
    }
    public UInt32 ReadUInt32(int length = 4)
    {
      byte[] buffer = new byte[4];
      switch (length)
      {
        case 3:
          {
            BaseStream.Read(buffer, 1, 3);
            break;
          }
        case 4:
          {
            BaseStream.Read(buffer, 0, 4);
            break;
          }
        default:
          {
            throw new ArgumentException();
            break;
          }
      }
      if (mode == ByteOrderMode.BE) Array.Reverse(buffer);
      return System.BitConverter.ToUInt32(buffer, 0);
    }
    public Int32 ReadInt32(int length = 4)
    {
      byte[] buffer = new byte[4];
      switch (length)
      {
        case 3:
          {
            BaseStream.Read(buffer, 1, 3);
            break;
          }
        case 4:
          {
            BaseStream.Read(buffer, 0, 4);
            break;
          }
        default:
          {
            throw new ArgumentException();
            break;
          }
      }
      if (mode == ByteOrderMode.BE) Array.Reverse(buffer);
      return System.BitConverter.ToInt32(buffer, 0);
    }
    public byte[] ReadBlock()
    {
      if (BaseStream.CanSeek)
      {
        int resultLen = (int)BaseStream.Length - (int)BaseStream.Position;
        byte[] buffer = new byte[resultLen];
        BaseStream.Read(buffer, 0, resultLen);
        return buffer;
      }
      else
      {
        List<byte> r = new List<byte>();
        do
        {
          r.Add((byte)BaseStream.ReadByte());
        } while (DataAvailable);
        return r.ToArray();
      }
    }
    public byte[] ReadBlock(int length)
    {
      byte[] buffer = new byte[length];
      BaseStream.Read(buffer, 0, length);
      return buffer;
    }
    public byte Peek()
    {
      if (BaseStream.CanSeek)
      {
        System.IO.StreamReader sr = new System.IO.StreamReader(BaseStream);
        return (byte)sr.Peek();
      }
      else
      {
        throw new NotImplementedException();
      }
    }
    public string ReadToEnd()
    {
      System.IO.StreamReader sr = new System.IO.StreamReader(BaseStream);
      return sr.ReadToEnd();
    }
    public bool DataAvailable
    {
      get
      {
        if (BaseStream.CanSeek)
        {
          return (BaseStream.Length > BaseStream.Position);
        }
        else
        {
          return ((System.Net.Sockets.NetworkStream)BaseStream).DataAvailable;
        }
      }
    }
  }
}
