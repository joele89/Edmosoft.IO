using System;
using System.Collections.Generic;

namespace Edmosoft.IO
{
  public enum ByteOrderMode
  {
    LE = 1,
    BE = 2
  }
  [Flags]
  public enum LineTerminator
  {
    CR = 1,
    LF = 2,
    CRLF = 3
  }
}
