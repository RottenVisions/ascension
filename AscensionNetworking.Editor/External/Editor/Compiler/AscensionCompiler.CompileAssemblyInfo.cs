partial class AscensionCompiler
{
    private static void CompileAssemblyInfo(AscensionCompilerOperation op)
    {
        using (AscensionSourceFile file = new AscensionSourceFile(op.assemblyInfoFilePath))
        {
            file.EmitLine("using System.Reflection;");
            file.EmitLine("using System.Runtime.CompilerServices;");
            file.EmitLine("using System.Runtime.InteropServices;");
            file.EmitLine();
            file.EmitLine("[assembly: AssemblyTitle(\"Ascension.Data\")]");
            file.EmitLine("[assembly: Guid(\"bd29ff3d-20fc-49ac-8303-459b4d662c04\")]");
            file.EmitLine("[assembly: AssemblyVersion(\"0.5.0.0\")]");
            file.EmitLine("[assembly: AssemblyFileVersion(\"0.0.0.0\")]");
        }
    }
}