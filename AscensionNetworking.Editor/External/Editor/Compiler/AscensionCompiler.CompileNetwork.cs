using System.Collections.Generic;
using System.Linq;
using Ascension.Compiler;

partial class AscensionCompiler
{
    private static void EmitRegisterFactory<T>(AscensionSourceFile file, IEnumerable<T> decorators)
        where T : AssetDecorator
    {
        foreach (T d in decorators)
        {
            file.EmitLine("Ascension.Networking.Factory.Register({0}.Instance);", d.NameMeta);
        }
    }

    private static void CompileNetwork(AscensionCompilerOperation op)
    {
        using (AscensionSourceFile file = new AscensionSourceFile(op.networkFilePath))
        {
            file.EmitScope("namespace Ascension.Networking.Data", () =>
            {
                file.EmitScope("public static class AscensionNetworkInternalUser", () =>
                {
                    file.EmitScope("public static void EnvironmentSetup()", () =>
                    {
                        EmitRegisterFactory(file, op.project.Structs.Select(x => new ObjectDecorator(x)));
                        EmitRegisterFactory(file, op.project.Commands.Select(x => new CommandDecorator(x)));
                        EmitRegisterFactory(file, op.project.Events.Select(x => new EventDecorator(x)));
                        EmitRegisterFactory(file, op.project.States.Select(x => new StateDecorator(x)));
                    });

                    file.EmitScope("public static void EnvironmentReset()", () => { });
                });
            });
        }
    }
}