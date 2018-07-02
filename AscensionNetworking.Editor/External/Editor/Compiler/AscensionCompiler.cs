static partial class AscensionCompiler
{
    public static void Run(AscensionCompilerOperation op, bool emitAssemblyInfo = true)
    {
        CompileMaps(op);
        CompilePrefabs(op);
        CompileNetwork(op);

        //Should we emit the assembly info?
        if(emitAssemblyInfo)
            CompileAssemblyInfo(op);

        op.project.GenerateCode(op.projectFilePath);
    }

    private static void EmitFileHeader(AscensionSourceFile file)
    {
        file.EmitLine("using System;");
        file.EmitLine("using System.Collections.Generic;");
        file.EmitLine("using Ascension.Networking;");
        file.EmitLine("using Ascension.Networking.Sockets;");
        file.EmitLine();
    }
}