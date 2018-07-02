using System;
using System.Collections.Generic;
using System.Linq;

namespace Ascension.Networking
{
    /// <summary>
    /// Base class for all Ascension specific exceptions
    /// </summary>
    public class AscensionException : Exception
    {
        public AscensionException(string message)
    : base(message) {
        }

        public AscensionException(string message, object arg0)
    : base(string.Format(message, arg0)) {
        }

        public AscensionException(string message, object arg0, object arg1)
    : base(string.Format(message, arg0, arg1)) {
        }

        public AscensionException(string message, object arg0, object arg1, object arg2)
    : base(string.Format(message, arg0, arg1, arg2)) {
        }

        public AscensionException(string message, params object[] args)
    : base(string.Format(message, args)) {
        }
    }
}
