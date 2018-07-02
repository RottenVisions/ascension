using System;
using System.Diagnostics;

public class AscensionCompilerAssertFailedException : Exception
{
    public AscensionCompilerAssertFailedException()
    {
    }

    public AscensionCompilerAssertFailedException(string msg) : base(msg)
    {
    }
}

public static class Assert
{
    [Conditional("DEBUG")]
    public static void Fail()
    {
        throw new AscensionCompilerAssertFailedException();
    }

    [Conditional("DEBUG")]
    public static void Fail(string message)
    {
        throw new AscensionCompilerAssertFailedException(message);
    }

    [Conditional("DEBUG")]
    public static void Same(object a, object b)
    {
        NotNull(a);
        NotNull(b);
        True(ReferenceEquals(a, b));
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
            throw new AscensionCompilerAssertFailedException();
        }
    }

    [Conditional("DEBUG")]
    public static void False(bool condition)
    {
        if (condition)
        {
            throw new AscensionCompilerAssertFailedException();
        }
    }

    [Conditional("DEBUG")]
    public static void False(bool condition, string message)
    {
        if (condition)
        {
            throw new AscensionCompilerAssertFailedException(message);
        }
    }

    [Conditional("DEBUG")]
    public static void True(bool condition, string message)
    {
        if (!condition)
        {
            throw new AscensionCompilerAssertFailedException(message);
        }
    }

    [Conditional("DEBUG")]
    public static void True(bool condition, string message, params object[] args)
    {
        if (!condition)
        {
            throw new AscensionCompilerAssertFailedException(String.Format(message, args));
        }
    }
}