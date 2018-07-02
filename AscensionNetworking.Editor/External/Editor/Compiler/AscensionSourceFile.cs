using System;
using System.IO;
using System.Text;

public class AscensionSourceFile : IDisposable
{
    private readonly StringBuilder buffer = new StringBuilder();
    private readonly string file = "";

    public AscensionSourceFile(string path)
    {
        IndentLevel = 0;
        file = path;
    }

    public int IndentLevel { get; set; }

    public void Dispose()
    {
        Save();
    }

    public void Emit(string text)
    {
        buffer.Append(text);
    }

    public void Emit(string text, params object[] args)
    {
        buffer.Append(String.Format(text, args));
    }

    public void EmitBOL()
    {
        EmitBOL("");
    }

    public void EmitBOL(string text, params object[] args)
    {
        EmitBOL(string.Format(text, args));
    }

    public void EmitBOL(string text)
    {
        Emit(new string(' ', IndentLevel * 2));
        Emit(text);
    }

    public void EmitEOL(string text, params object[] args)
    {
        EmitEOL(string.Format(text, args));
    }

    public void EmitEOL(string text)
    {
        Emit(text);
        Emit("\r\n");
    }

    public void EmitEOL()
    {
        EmitEOL("");
    }

    public void EmitLine(string text)
    {
        EmitBOL();
        Emit(text);
        EmitEOL();
    }

    public void EmitLine(string text, params object[] args)
    {
        EmitBOL();
        Emit(String.Format(text, args));
        EmitEOL();
    }

    public void EmitIfElse(string condition, Action @if, Action @else)
    {
        EmitScope("if (" + condition + ")", @if);
        EmitScope("else", @else);
    }

    public void EmitScope(string text, Action callback)
    {
        EmitLine(text.Trim() + " {");
        Indented(() => { callback(); });
        EmitLine("}");
    }

    public void EmitScope(string text, object arg0, Action callback)
    {
        EmitScope(string.Format(text, arg0), callback);
    }

    public void EmitScope(string text, object arg0, object arg1, Action callback)
    {
        EmitScope(string.Format(text, arg0, arg1), callback);
    }

    public void EmitScope(string text, object arg0, object arg1, object arg2, Action callback)
    {
        EmitScope(string.Format(text, arg0, arg1, arg2), callback);
    }

    public void EmitLine()
    {
        EmitLine("");
    }

    public void Indented(Action action)
    {
        IndentLevel += 1;
        action();
        IndentLevel -= 1;
    }

    public void Save()
    {
        if (File.Exists(file))
        {
            File.Delete(file);
        }

        File.WriteAllText(file, buffer.ToString());
    }
}