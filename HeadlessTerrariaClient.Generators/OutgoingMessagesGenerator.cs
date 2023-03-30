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

        //if (!Debugger.IsAttached)
        //    Debugger.Launch();

        Compilation compilation = context.Compilation;

        IEnumerable<SyntaxNode> allNodes = compilation.SyntaxTrees.SelectMany(s => s.GetRoot().DescendantNodes());
        IEnumerable<MethodDeclarationSyntax> methodsToGeneratorSendMethodsFor = allNodes
            .Where(d => d.IsKind(SyntaxKind.MethodDeclaration))
            .OfType<MethodDeclarationSyntax>()
            .Where(s => s.AttributeLists.SelectMany(a => a.Attributes).Select(a => a.Name.ToString()).Where(n => n == AttributeName || n == AttributeNameShort).Any());

        StringBuilder source = new StringBuilder();

        source.AppendLine("namespace HeadlessTerrariaClient;");
        source.AppendLine("public partial class HeadlessClient");
        source.AppendLine("{");

        foreach (MethodDeclarationSyntax method in methodsToGeneratorSendMethodsFor)
        {
            IMethodSymbol methodSymbol = compilation.GetSemanticModel(method.SyntaxTree).GetDeclaredSymbol(method)!;

            if (methodSymbol.Name.StartsWith("Write") && methodSymbol.Name.Length > 5)
            {
                source.Append("public void Send");
                source.Append(methodSymbol.Name.Remove(0, 5));
                source.Append("(");

                if (methodSymbol.Parameters.Length > 0)
                {
                    for (int i = 0; i < methodSymbol.Parameters.Length; i++)
                    {
                        IParameterSymbol parameter = methodSymbol.Parameters[i];

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

                        if (i + 1 < methodSymbol.Parameters.Length)
                        {
                            source.Append(", ");
                        }
                    }
                }

                source.Append(") ");
                source.Append("{ ");
                source.Append(methodSymbol.Name);
                source.Append("(");

                if (methodSymbol.Parameters.Length > 0)
                {
                    for (int i = 0; i < methodSymbol.Parameters.Length; i++)
                    {
                        IParameterSymbol parameter = methodSymbol.Parameters[i];

                        source.Append(parameter.Name.ToString());

                        if (i + 1 < methodSymbol.Parameters.Length)
                        {
                            source.Append(", ");
                        }
                    }
                }

                source.Append("); TerrariaNetworkClient.Send(MessageWriter.EndMessage());");
                source.AppendLine(" }");

                source.Append("public async Task Send");
                source.Append(methodSymbol.Name.Remove(0, 5));
                if (methodSymbol.Parameters.Length > 0)
                {
                    source.Append("Async(");

                    for (int i = 0; i < methodSymbol.Parameters.Length; i++)
                    {
                        IParameterSymbol parameter = methodSymbol.Parameters[i];

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

                        if (i + 1 < methodSymbol.Parameters.Length)
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
                source.Append(methodSymbol.Name);
                source.Append("(");

                if (methodSymbol.Parameters.Length > 0)
                {
                    for (int i = 0; i < methodSymbol.Parameters.Length; i++)
                    {
                        IParameterSymbol parameter = methodSymbol.Parameters[i];

                        source.Append(parameter.Name.ToString());

                        if (i + 1 < methodSymbol.Parameters.Length)
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
}
