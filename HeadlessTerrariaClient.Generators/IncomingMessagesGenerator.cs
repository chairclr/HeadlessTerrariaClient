using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HeadlessTerrariaClient.Generators;

[Generator(LanguageNames.CSharp)]
public partial class IncomingMessagesGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        bool FilterSync(SyntaxNode node, CancellationToken cancellationToken)
        {
            if (node is not MethodDeclarationSyntax methodSyntax)
            {
                return false;
            }

            if (methodSyntax.ReturnType is not PredefinedTypeSyntax predefinedType)
            {
                return false;
            }

            if (predefinedType.Keyword.ToString() != "void")
            {
                return false;
            }

            return true;
        };

        bool FilterAsync(SyntaxNode node, CancellationToken cancellationToken)
        {
            if (node is not MethodDeclarationSyntax methodSyntax)
            {
                return false;
            }

            if (methodSyntax.ReturnType is not IdentifierNameSyntax identifierName)
            {
                return false;
            }

            if (identifierName.Identifier.ToString() != "ValueTask" && identifierName.Identifier.ToString() != "Task")
            {
                return false;
            }

            return true;
        };

        (string fullyQualifiedMessageType, string messageHandlerName) TransformMessageAttribute(GeneratorAttributeSyntaxContext attributeSyntaxContext, CancellationToken cancellationToken)
        {
            string messageType = attributeSyntaxContext.Attributes.Single().ConstructorArguments.Single().ToCSharpString();

            string name = attributeSyntaxContext.TargetSymbol.Name;

            return (messageType, name);
        };


        IncrementalValuesProvider<(string fullyQualifiedMessageType, string messageHandlerName)> syncMessages = context.SyntaxProvider.ForAttributeWithMetadataName("HeadlessTerrariaClient.Messages.IncomingMessageAttribute", FilterSync, TransformMessageAttribute);

        context.RegisterSourceOutput(syncMessages.Collect(), (spc, messages) =>
        {
            StringBuilder source = new StringBuilder();

            source.AppendLine("/// Auto-generated ///");

            source.AppendLine(@"
namespace HeadlessTerrariaClient.Messages;
internal partial class TerrariaMessageHandler
{
public void HandleIncomingMessage(HeadlessTerrariaClient.Messages.MessageType type, System.IO.BinaryReader reader)
{
switch (type)
{");


            foreach ((string fullyQualifiedMessageType, string messageHandlerName) in messages)
            {
                source.AppendLine($@"case {fullyQualifiedMessageType}: Client.{messageHandlerName}(reader); break;");
            }

            source.AppendLine(@"}
}
}");

            spc.AddSource("IncomingMessageHandler.g.cs", source.ToString());
        });


        IncrementalValuesProvider<(string fullyQualifiedMessageType, string messageHandlerName)> asyncMessages = context.SyntaxProvider.ForAttributeWithMetadataName("HeadlessTerrariaClient.Messages.IncomingMessageAttribute", FilterAsync, TransformMessageAttribute);

        context.RegisterSourceOutput(asyncMessages.Collect(), (spc, messages) =>
        {
            StringBuilder source = new StringBuilder();

            source.AppendLine("/// Auto-generated ///");

            source.AppendLine(@"
namespace HeadlessTerrariaClient.Messages;
internal partial class TerrariaMessageHandler
{
public async ValueTask HandleIncomingMessageAsync(HeadlessTerrariaClient.Messages.MessageType type, System.IO.BinaryReader reader)
{
switch (type)
{");


            foreach ((string fullyQualifiedMessageType, string messageHandlerName) in messages)
            {
                source.AppendLine($@"case {fullyQualifiedMessageType}: await Client.{messageHandlerName}(reader); break;");
            }

            source.AppendLine(@"}
}
}");

            spc.AddSource("IncomingMessageAsyncHandler.g.cs", source.ToString());
        });
    }
}
