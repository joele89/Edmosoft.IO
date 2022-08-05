using System;
using System.Collections.Generic;

namespace Edmosoft.IO
{
  public class StreamReader
  {
    public ByteOrderMode mode = ByteOrderMode.LE;
    public System.Text.Encoding encoding = System.Text.Encoding.ASCII;

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
          BaseStream.Read(buffer, 1, 3);
          break;
        case 4:
          BaseStream.Read(buffer, 0, 4);
          break;
        default:
          throw new ArgumentException("length must be 3 or 4");
          //break;
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
          if (mode == ByteOrderMode.BE)
            BaseStream.Read(buffer, 1, 3);
          else
            BaseStream.Read(buffer, 0, 3);
          break;
        case 4:
          BaseStream.Read(buffer, 0, 4);
          break;
        default:
          throw new ArgumentException("length must be 3 or 4");
          //break;
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
          int b = BaseStream.ReadByte();
          if (b == -1)
            break;
          else
            r.Add((byte)b);
        } while (true);
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
        long startPos = BaseStream.Position;
        int peekByte = BaseStream.ReadByte();
        BaseStream.Position = startPos;
        if (peekByte == -1)
          throw new System.IO.EndOfStreamException();
        return (byte)peekByte;
      }
      else
      {
        throw new NotImplementedException();
      }
    }
    public string ReadLine(bool nullTerminated = false)
    {
      bool EndOfLine = false;
      string ret = "";
      long start = BaseStream.Position;
      int count = 0;
      do
      {
        count++;
        try
        {
          ret = encoding.GetString(ReadBlock(count));
          if (nullTerminated)
          {
            if (ret.EndsWith("\x00")) EndOfLine = true;
          }
          else
          {
            if (ret.EndsWith("\x0d") | ret.EndsWith("\x0a"))
            {
              if (ret.Contains("\x0d") & ret.Contains("\x0a")) EndOfLine = true;
            }
            else
              if (ret.Contains("\x0d") | ret.Contains("\x0a")) EndOfLine = true;
          }
        }
        catch { }
        BaseStream.Position = start;
      } while (DataAvailable & !EndOfLine);
      string[] line;
      int skip = 0;
      if (nullTerminated)
      {
        line = ret.Split(new char[] { '\x00' }, 2);
        if (ret.Contains("\x00")) skip += encoding.GetByteCount("\x00");
      }
      else
      {
        line = ret.Split(new char[] { '\x0a', '\x0d' }, 2);
        if (ret.Contains("\x0a")) skip += encoding.GetByteCount("\x0a");
        if (ret.Contains("\x0d")) skip += encoding.GetByteCount("\x0d");
      }
      BaseStream.Position = start + encoding.GetByteCount(line[0]) + skip;
      return line[0];
    }
    public string PeekChar()
    {
      if (BaseStream.CanSeek)
      {
        long startPos = BaseStream.Position;
        string ret = ReadChar();
        BaseStream.Position = startPos;
        return ret;
      }
      else
      {
        throw new NotImplementedException();
      }
    }

    public string ReadChar()
    {
      byte bom = Peek();
      byte len = 1;
      byte[] block;
      //can't use switch block as encodings don't evaluate to constants
      if (encoding == System.Text.Encoding.ASCII)
      {
        return encoding.GetString(ReadBlock(1));
      }
      else if (encoding == System.Text.Encoding.UTF8)
      {
        if ((bom & 0x80) == 0x0) return encoding.GetString(ReadBlock(1));
        if (mode == ByteOrderMode.LE)
        {
          long startPos = BaseStream.Position;
          for (int i = 2; i <= 4; i++)
          {
            bom = ReadByte();
            if ((bom & 0xF0) != 0x80) break;
          }
          BaseStream.Position = startPos;
        }
        if ((bom & 0xC0) == 0xC0) len = 2;
        if ((bom & 0xE0) == 0xE0) len = 3;
        if ((bom & 0xF0) == 0xF0) len = 4;
        block = ReadBlock(len);
      }
      else if (encoding == System.Text.Encoding.Unicode)
      {
        len = 2;
        if (mode == ByteOrderMode.LE)
        {
          long startPos = BaseStream.Position;
          ReadByte(); // discard Little
          bom = ReadByte();
          BaseStream.Position = startPos;
        }
        if ((bom & 0xD8) == 0xD8) len = 4;
        block = ReadBlock(len);
      }
      else if (encoding == System.Text.Encoding.UTF32)
      {
        block = ReadBlock(len);
      }
      else
      {
        throw new NotImplementedException();
      }
      if (mode == ByteOrderMode.BE) Array.Reverse(block);
      return encoding.GetString(block);
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
        else if (BaseStream.GetType() == typeof(System.Net.Sockets.NetworkStream))
        {
          try
          {
            return ((System.Net.Sockets.NetworkStream)BaseStream).DataAvailable;
          }
          catch (InvalidCastException)
          {
            throw new NotImplementedException();
          }
        }
        else
        {
          throw new NotImplementedException();
        }
      }
    }
  }
}
