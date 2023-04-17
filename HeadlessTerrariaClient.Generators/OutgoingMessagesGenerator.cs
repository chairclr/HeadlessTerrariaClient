using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace HeadlessTerrariaClient.Generators;

[Generator(LanguageNames.CSharp)]
public class OutgoingMessagesGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        bool Filter(SyntaxNode node, CancellationToken cancellationToken)
        {
            if (node is not MethodDeclarationSyntax methodSyntax)
            {
                return false;
            }

            return true;
        };

        (string fullyQualifiedMessageType, string sendMethodName, string writeMethodName, string sendMethodParameters, string callWriteParameters) TransformMessageAttribute(GeneratorAttributeSyntaxContext attributeSyntaxContext, CancellationToken cancellationToken)
        {
            IMethodSymbol method = (IMethodSymbol)attributeSyntaxContext.TargetSymbol;

            string messageType = attributeSyntaxContext.Attributes.Single().ConstructorArguments.Single().ToCSharpString();

            string writeMethodName = method.Name;

            string sendMethodName = "Send" + (writeMethodName.StartsWith("Write") ? writeMethodName.Remove(0, 5) : writeMethodName);

            StringBuilder sendMethodParameters = new StringBuilder();

            StringBuilder callWriteParameters = new StringBuilder();

            if (method.Parameters.Length > 0)
            {
                for (int i = 0; i < method.Parameters.Length; i++)
                {
                    IParameterSymbol parameter = method.Parameters[i];

                    sendMethodParameters.Append(parameter.ToString());

                    if (parameter.HasExplicitDefaultValue)
                    {
                        sendMethodParameters.Append(" = ");
                        if (parameter.ExplicitDefaultValue is null)
                        {
                            sendMethodParameters.Append("null");
                        }
                        else
                        {
                            if (parameter.ExplicitDefaultValue is bool booleanValue)
                            {
                                sendMethodParameters.Append(booleanValue ? "true" : "false");
                            }
                            else
                            {
                                sendMethodParameters.Append(parameter.ExplicitDefaultValue!.ToString());
                            }
                        }
                    }

                    if (i + 1 < method.Parameters.Length)
                    {
                        sendMethodParameters.Append(", ");
                    }
                }

                for (int i = 0; i < method.Parameters.Length; i++)
                {
                    IParameterSymbol parameter = method.Parameters[i];

                    callWriteParameters.Append(parameter.Name.ToString());

                    if (i + 1 < method.Parameters.Length)
                    {
                        callWriteParameters.Append(", ");
                    }
                }
            }


            return (messageType, sendMethodName, writeMethodName, sendMethodParameters.ToString(), callWriteParameters.ToString());
        };


        IncrementalValuesProvider<(string fullyQualifiedMessageType, string sendMethodName, string writeMethodName, string sendMethodParameters, string callWriteParameters)> messages = context.SyntaxProvider.ForAttributeWithMetadataName("HeadlessTerrariaClient.Messages.OutgoingMessageAttribute", Filter, TransformMessageAttribute);

        Dictionary<string, int> overloadFileNameBuckets = new Dictionary<string, int>();

        context.RegisterSourceOutput(messages, (spc, message) =>
        {
            string? overloadNumber = null;

            if (overloadFileNameBuckets.ContainsKey(message.sendMethodName))
            {
                overloadFileNameBuckets[message.sendMethodName]++;
                overloadNumber = overloadFileNameBuckets[message.sendMethodName].ToString();
            }
            else
            {
                overloadFileNameBuckets.Add(message.sendMethodName, 0);
            }

            spc.AddSource($"OutgoingMessages.{message.sendMethodName}{overloadNumber}.g.cs", $$"""
                                                                             /// Auto-generated ///
                                                                             namespace HeadlessTerrariaClient;

                                                                             public partial class HeadlessClient
                                                                             {
                                                                                public void {{message.sendMethodName}}({{message.sendMethodParameters}}) { MessageWriter.BeginMessage({{message.fullyQualifiedMessageType}}); {{message.writeMethodName}}({{message.callWriteParameters}}); TCPNetworkClient.Send(MessageWriter.EndMessage()); }
                                                                             }
                                                                             """);



            spc.AddSource($"OutgoingMessages.{message.sendMethodName}Async{overloadNumber}.g.cs", $$"""
                                                                             /// Auto-generated ///
                                                                             namespace HeadlessTerrariaClient;

                                                                             public partial class HeadlessClient
                                                                             {
                                                                                public async ValueTask {{message.sendMethodName}}Async({{message.sendMethodParameters}}) { MessageWriter.BeginMessage({{message.fullyQualifiedMessageType}}); {{message.writeMethodName}}({{message.callWriteParameters}}); await TCPNetworkClient.SendAsync(MessageWriter.EndMessage()); }
                                                                             }
                                                                             """);
        });
    }
}
