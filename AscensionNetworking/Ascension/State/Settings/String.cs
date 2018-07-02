using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ascension.Networking
{
    public struct PropertyStringSettings
    {
        public StringEncodings Encoding;
        public Encoding EncodingClass
        {
            get
            {
                return System.Text.Encoding.UTF8;

                //switch (Encoding) {
                //  case StringEncodings.ASCII: return System.Text.ASCIIEncoding.ASCII;
                //  case StringEncodings.UTF8: return System.Text.Encoding.UTF8;
                //}
                //throw new NotSupportedException();
            }
        }
    }
}
