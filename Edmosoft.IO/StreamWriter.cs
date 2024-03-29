using System;

namespace Edmosoft.IO
{
  public class StreamWriter
  {
    public ByteOrderMode mode = ByteOrderMode.LE;
    public LineTerminator terminator = LineTerminator.CRLF;
    public System.Text.Encoding encoding = System.Text.Encoding.ASCII;
    public System.IO.Stream BaseStream;
    public StreamWriter(ref System.IO.Stream stream)
    {
      BaseStream = stream;
    }
    public void WriteByte(byte value)
    {
      BaseStream.WriteByte(value);
    }
    public void WriteUInt16(UInt16 value)
    {
      byte[] buffer = System.BitConverter.GetBytes(value);
      if (mode == ByteOrderMode.BE) Array.Reverse(buffer);
      BaseStream.Write(buffer, 0, 2);
    }
    public void WriteUInt32(UInt32 value, int length = 4)
    {
      byte[] buffer = System.BitConverter.GetBytes(value);
      if (mode == ByteOrderMode.BE) Array.Reverse(buffer);
      switch (length)
      {
        case 3:
          if (mode == ByteOrderMode.BE)
            BaseStream.Write(buffer, 1, 3);
          else
            BaseStream.Write(buffer, 0, 3);
          break;
        case 4:
          BaseStream.Write(buffer, 0, 4);
          break;
        default:
          throw new ArgumentException("length must be 3 or 4");
          //break;
      }
    }
    public void WriteInt32(Int32 value, int length = 4)
    {
      byte[] buffer = System.BitConverter.GetBytes(value);
      if (mode == ByteOrderMode.BE) Array.Reverse(buffer);
      switch (length)
      {
        case 3:
          if (mode == ByteOrderMode.BE)
            BaseStream.Write(buffer, 1, 3);
          else
            BaseStream.Write(buffer, 0, 3);
          break;
        case 4:
          BaseStream.Write(buffer, 0, 4);
          break;
        default:
          throw new ArgumentException("length must be 3 or 4");
          //break;
      }
    }
    public void WriteLine(string data)
    {
      if ((terminator & LineTerminator.CR) == LineTerminator.CR)
        data += "\r";
      if ((terminator & LineTerminator.LF) == LineTerminator.LF)
        data += "\n";
      foreach (char c in data)
        WriteChar(c);
    }
    public void WriteChar(char data)
    {
      WriteChar(data.ToString());
    }
    public void WriteChar(string data)
    {
      byte[] block = encoding.GetBytes(data);
      if (mode == ByteOrderMode.BE) Array.Reverse(block);
      WriteBlock(block);
    }
    public void WriteBlock(byte[] buffer)
    {
      BaseStream.Write(buffer, 0, buffer.Length);
    }
    public void WriteBlock(byte[] buffer, int length)
    {
      BaseStream.Write(buffer, 0, length);
    }
  }
}
