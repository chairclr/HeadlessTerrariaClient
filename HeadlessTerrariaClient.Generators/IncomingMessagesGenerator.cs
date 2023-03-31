using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace HeadlessTerrariaClient.Generators;

[Generator]
public class IncomingMessagesGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        Compilation compilation = context.Compilation;

        INamedTypeSymbol headlessClientSymbol = compilation.GetTypeByMetadataName("HeadlessTerrariaClient.HeadlessClient")!;
        INamedTypeSymbol incomingMessagesAttributeSymbol = compilation.GetTypeByMetadataName("HeadlessTerrariaClient.Messages.IncomingMessageAttribute")!;
        INamedTypeSymbol taskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task")!;
        INamedTypeSymbol valueTaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask")!;

        IEnumerable<IMethodSymbol> methodsWithAttribute = headlessClientSymbol.GetMembers()
            .Where(x => x.Kind == SymbolKind.Method)
            .Cast<IMethodSymbol>()
            .Where(x => x.GetAttributes().Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, incomingMessagesAttributeSymbol)));

        StringBuilder source = new StringBuilder();

        source.AppendLine("namespace HeadlessTerrariaClient.Messages;");
        source.AppendLine("internal partial class TerrariaMessageHandler");
        source.AppendLine("{");

        source.AppendLine("public async ValueTask HandleIncomingMessageAsync(HeadlessTerrariaClient.Messages.MessageType type, System.IO.BinaryReader reader)");
        source.AppendLine("{");

        source.AppendLine("switch (type)");
        source.AppendLine("{");
        foreach (IMethodSymbol method in methodsWithAttribute.Where(x => SymbolEqualityComparer.Default.Equals(x.ReturnType, taskSymbol) || SymbolEqualityComparer.Default.Equals(x.ReturnType, valueTaskSymbol)))
        {
            source.Append("case ");

            source.Append(GetIncomingMessageTypeFromAttribute(method, incomingMessagesAttributeSymbol));

            source.Append(": await Client.");

            source.Append(method.Name);

            source.AppendLine("(reader); break;");
        }
        source.AppendLine("}");

        source.AppendLine("}");


        source.AppendLine("public void HandleIncomingMessage(HeadlessTerrariaClient.Messages.MessageType type, System.IO.BinaryReader reader)");
        source.AppendLine("{");

        source.AppendLine("switch (type)");
        source.AppendLine("{");
        foreach (IMethodSymbol method in methodsWithAttribute.Where(x => SymbolEqualityComparer.Default.Equals(x.ReturnType, compilation.GetSpecialType(SpecialType.System_Void))))
        {
            source.Append("case ");

            source.Append(GetIncomingMessageTypeFromAttribute(method, incomingMessagesAttributeSymbol));

            source.Append(": Client.");

            source.Append(method.Name);

            source.AppendLine("(reader); break;");
        }
        source.AppendLine("}");

        source.AppendLine("}");

        source.AppendLine("}");
        context.AddSource("TerrariaMessageHandlerIncomingMessageMappings.g.cs", source.ToString());

    }

    public void Initialize(GeneratorInitializationContext context)
    {

    }

    private string GetIncomingMessageTypeFromAttribute(IMethodSymbol methodSymbol, INamedTypeSymbol incomingMessagesAttributeSymbol)
    {
        AttributeData attribute = methodSymbol.GetAttributes().Single(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, incomingMessagesAttributeSymbol));

        TypedConstant constant = attribute.ConstructorArguments.First();

        return constant.ToCSharpString();
    }
}
