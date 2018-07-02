using System;
using System.Diagnostics;

namespace Ascension.Networking
{
    /// <summary>
    /// Thrown if a debug assert fails somewhere in the code
    /// </summary>
    public class NetAssertFailedException : Exception
    {
        public NetAssertFailedException()
        {
            NetLog.Error("ASSERT FAILED");
        }

        public NetAssertFailedException(string msg)
            : base(msg)
        {
            NetLog.Error("ASSERT FAILED: " + msg);
        }
    }

    public static class NetAssert
    {
        [Conditional("DEBUG")]
        public static void Fail()
        {
            throw new NetAssertFailedException();
        }

        [Conditional("DEBUG")]
        public static void Fail(string message)
        {
            throw new NetAssertFailedException(message);
        }

        [Conditional("DEBUG")]
        public static void Same(object a, object b)
        {
            Same(a, b, "");
        }

        [Conditional("DEBUG")]
        public static void Same(object a, object b, string error)
        {
            NotNull(a);
            NotNull(b);
            True(ReferenceEquals(a, b), error);
        }

        [Conditional("DEBUG")]
        public static void NotSame(object a, object b)
        {
            NotNull(a);
            NotNull(b);
            False(ReferenceEquals(a, b));
        }

        [Conditional("DEBUG")]
        public static void Null(object a)
        {
            True(ReferenceEquals(a, null), "object was not null");
        }

        [Conditional("DEBUG")]
        public static void Null(object a, string msg)
        {
            True(ReferenceEquals(a, null), msg);
        }

        [Conditional("DEBUG")]
        public static void NotNull(object a)
        {
            False(ReferenceEquals(a, null), "object was null");
        }

        [Conditional("DEBUG")]
        public static void Equal(object a, object b)
        {
            NotNull(a);
            NotNull(b);
            True(a.Equals(b));
        }

        [Conditional("DEBUG")]
        public static void Equal<T>(T a, T b) where T : IEquatable<T>
        {
            True(a.Equals(b));
        }

        [Conditional("DEBUG")]
        public static void NotEqual(object a, object b)
        {
            NotNull(a);
            NotNull(b);
            False(a.Equals(b));
        }

        [Conditional("DEBUG")]
        public static void NotEqual<T>(T a, T b) where T : IEquatable<T>
        {
            False(a.Equals(b));
        }

        [Conditional("DEBUG")]
        public static void True(bool condition)
        {
            if (!condition)
            {
                throw new NetAssertFailedException();
            }
        }

        [Conditional("DEBUG")]
        public static void False(bool condition)
        {
            if (condition)
            {
                throw new NetAssertFailedException();
            }
        }


        [Conditional("DEBUG")]
        public static void False(bool condition, string message)
        {
            if (condition)
            {
                throw new NetAssertFailedException(message);
            }
        }

        [Conditional("DEBUG")]
        public static void True(bool condition, string message)
        {
            if (!condition)
            {
                throw new NetAssertFailedException(message);
            }
        }

        [Conditional("DEBUG")]
        public static void True(bool condition, string message, params object[] args)
        {
            if (!condition)
            {
                throw new NetAssertFailedException(String.Format(message, args));
            }
        }
    }
}

