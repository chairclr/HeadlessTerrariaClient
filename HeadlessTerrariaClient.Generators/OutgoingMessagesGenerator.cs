using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HeadlessTerrariaClient.Generators;

[Generator]
public class OutgoingMessagesGenerator : ISourceGenerator
{
    private static readonly string AttributeName = "OutgoingMessageAttribute";
    private static readonly string AttributeNameShort = "OutgoingMessage";

    public void Initialize(GeneratorInitializationContext context)
    {

    }

    public void Execute(GeneratorExecutionContext context)
    {
        Compilation compilation = context.Compilation;

        INamedTypeSymbol headlessClientSymbol = compilation.GetTypeByMetadataName("HeadlessTerrariaClient.HeadlessClient")!;

        IEnumerable<IMethodSymbol> methodsWithAttribute = headlessClientSymbol.GetMembers()
            .Where(x => x.Kind == SymbolKind.Method)
            .Cast<IMethodSymbol>()
            .Where(x => x.GetAttributes().Select(x => x.AttributeClass!.Name).Where(n => n == AttributeName).Any());

        StringBuilder source = new StringBuilder();

        source.AppendLine("namespace HeadlessTerrariaClient;");
        source.AppendLine("public partial class HeadlessClient");
        source.AppendLine("{");

        foreach (IMethodSymbol method in methodsWithAttribute)
        {
            if (method.Name.StartsWith("Write") && method.Name.Length > 5)
            {
                string messageTypeString = GetOutgoingMessageTypeFromAttribute(method);

                source.Append("public void Send");
                source.Append(method.Name.Remove(0, 5));
                source.Append("(");
                if (method.Parameters.Length > 0)
                {
                    for (int i = 0; i < method.Parameters.Length; i++)
                    {
                        IParameterSymbol parameter = method.Parameters[i];

                        source.Append(parameter.ToString());

                        if (parameter.HasExplicitDefaultValue)
                        {
                            source.Append(" = ");
                            if (parameter.ExplicitDefaultValue is null)
                            {
                                source.Append("null");
                            }
                            else
                            {
                                source.Append(parameter.ExplicitDefaultValue!.ToString());
                            }
                        }

                        if (i + 1 < method.Parameters.Length)
                        {
                            source.Append(", ");
                        }
                    }
                }
                source.Append(") ");
                source.Append("{ ");
                source.Append($"MessageWriter.BeginMessage({messageTypeString}); ");
                source.Append(method.Name);
                source.Append("(");
                if (method.Parameters.Length > 0)
                {
                    for (int i = 0; i < method.Parameters.Length; i++)
                    {
                        IParameterSymbol parameter = method.Parameters[i];

                        source.Append(parameter.Name.ToString());

                        if (i + 1 < method.Parameters.Length)
                        {
                            source.Append(", ");
                        }
                    }
                }
                source.Append("); TerrariaNetworkClient.Send(MessageWriter.EndMessage());");
                source.AppendLine(" }");

                source.Append("public async ValueTask Send");
                source.Append(method.Name.Remove(0, 5));
                if (method.Parameters.Length > 0)
                {
                    source.Append("Async(");

                    for (int i = 0; i < method.Parameters.Length; i++)
                    {
                        IParameterSymbol parameter = method.Parameters[i];

                        source.Append(parameter.ToString());

                        if (parameter.HasExplicitDefaultValue)
                        {
                            source.Append(" = ");
                            if (parameter.ExplicitDefaultValue is null)
                            {
                                source.Append("null");
                            }
                            else
                            {
                                source.Append(parameter.ExplicitDefaultValue!.ToString());
                            }
                        }

                        if (i + 1 < method.Parameters.Length)
                        {
                            source.Append(", ");
                        }
                    }

                    source.Append(", CancellationToken cancellationToken = default) ");
                }
                else
                {
                    source.Append("Async(CancellationToken cancellationToken = default) ");
                }
                source.Append("{ ");
                source.Append($"MessageWriter.BeginMessage({messageTypeString}); ");
                source.Append(method.Name);
                source.Append("(");
                if (method.Parameters.Length > 0)
                {
                    for (int i = 0; i < method.Parameters.Length; i++)
                    {
                        IParameterSymbol parameter = method.Parameters[i];

                        source.Append(parameter.Name.ToString());

                        if (i + 1 < method.Parameters.Length)
                        {
                            source.Append(", ");
                        }
                    }
                }
                source.Append("); await TerrariaNetworkClient.SendAsync(MessageWriter.EndMessage(), cancellationToken);");
                source.AppendLine(" }");
            }
        }

        source.AppendLine("}");

        context.AddSource("HeadlessClientOutgoingMessages.g.cs", source.ToString());
    }

    private string GetOutgoingMessageTypeFromAttribute(IMethodSymbol methodSymbol)
    {
        AttributeData attribute = methodSymbol.GetAttributes().Single(x => x.AttributeClass is not null && x.AttributeClass!.Name == AttributeName || x.AttributeClass!.Name == AttributeNameShort);

        TypedConstant constant = attribute.ConstructorArguments.First();

        return constant.ToCSharpString();
    }
}
