using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FalconBMS.Launcher
{
    internal static class Utils
    {
        public static readonly UTF8Encoding UTF8_NO_BOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        public static StreamWriter CreateUtf8TextWihoutBom(string pathname, bool append=false)
        {
            UTF8Encoding utf8enc = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            return new StreamWriter(pathname, append, utf8enc);
        }
    }
}
