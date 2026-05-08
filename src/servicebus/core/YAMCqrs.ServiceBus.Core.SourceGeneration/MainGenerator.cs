using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using YAMCqrs.ServiceBus.Core.SourceGeneration.GeneratorHelper;
using YAMCqrs.ServiceBus.Core.SourceGeneration.Info;


namespace YAMCqrs.ServiceBus.Core.SourceGeneration;

/// <summary>
/// Main source generator for CQRS.
/// </summary>
[Generator]
public class MainGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the source generator.
    /// </summary>
    /// <param name="context"></param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG
        // UNCOMMENT THIS LINE TO DEBUG
        //Debugger.Launch();
#endif
        Debug.WriteLine("CQRS Source Generator initialized.");

        var cqrsDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsPotentialCqrsClass(s),
                transform: static (ctx, _) => GetCqrsInfo(ctx))
            .Where(static m => m is not null);

        var compilationAndDeclarations = context.CompilationProvider.Combine(cqrsDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndDeclarations,
            static (spc, source) => Execute(source.Left, source.Right!, spc));
    }

    private static bool IsPotentialCqrsClass(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax classDeclaration
            && classDeclaration.BaseList is not null;
    }

    private static bool IsPublishEvent(INamedTypeSymbol symbol)
    {
        // ✅ Detectar si hereda de PublishEvent (clase abstracta)
        var baseType = symbol.BaseType;
        while (baseType != null)
        {
            if (baseType.ToDisplayString().Contains("PublishEvent"))
            {
                Debug.WriteLine($"✨ Found PublishEvent child: {symbol.Name}");
                return true;
            }
            baseType = baseType.BaseType;
        }

        return false;
    }

    private static CqrsInfo GetCqrsInfo(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol symbol)
            return null;

        //Ignorar interfaces, enums, structs - solo procesar clases
        if (symbol.TypeKind != TypeKind.Class)
        {
            Debug.WriteLine($"⏭️  Skipping non-class type: {symbol.Name} (TypeKind: {symbol.TypeKind})");
            return null;
        }

        //Ignorar clases abstractas EXCEPTO si son eventos base
        if (symbol.IsAbstract)
        {
            Debug.WriteLine($"⏭️  Skipping abstract class: {symbol.Name}");
            return null;
        }

        var baseClass = symbol.BaseType?.BaseType;
        var topicMappings = new List<string>();
        var eventHandlers = new List<string>();
        var integrationEvents = new List<string>();
        var isGenericDefinition = symbol.IsGenericType;

        if (baseClass != null && baseClass.ToDisplayString().Contains(Const.InterfacesNames.SubscribeEvent))
        {
            var result = ExtractTopicFromConstructor(classDeclaration, context.SemanticModel);
            if (!string.IsNullOrEmpty(result))
            {
                topicMappings.Add(result);
            }
            else
            {
                topicMappings.Add("Missing TOPIC");
            }
        }
        else
        {
            foreach (var @interface in symbol.AllInterfaces)
            {
                var interfaceName = @interface.ConstructedFrom.ToDisplayString();

                // DEBUG: See which interfaces are being detected
                Debug.WriteLine($"Found interface: {interfaceName} on class: {symbol.Name}");

                switch (interfaceName)
                {
                    case Const.InterfacesNames.PublishEvent:
                        eventHandlers.Add(@interface.TypeArguments[0].ToDisplayString());
                        break;
                }
            }

            // Detect integration events without knowing about Kafka
            if (IsPublishEvent(symbol))
            {
                var eventTypeName = symbol.ToDisplayString();
                integrationEvents.Add(eventTypeName);
                Debug.WriteLine($"✨ Found IntegrationEvent: {eventTypeName}");
            }

            // Solo retornar si es handler O interceptor O event handler
            if (eventHandlers.Count == 0 && integrationEvents.Count == 0)
                return null;
        }

        return new CqrsInfo(
            symbol.ToDisplayString(),
            symbol.ContainingNamespace.ToDisplayString(),
            eventHandlers.ToImmutableArray(),
            integrationEvents.ToImmutableArray(),
            topicMappings.ToImmutableArray(),
            isGenericDefinition);
    }

#pragma warning disable IDE0060 // Remove unused parameter
    private static void Execute(Compilation compilation, ImmutableArray<CqrsInfo> cqrsInfos, SourceProductionContext context)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        if (cqrsInfos.IsDefaultOrEmpty)
            Debug.WriteLine("No CQRS classes found.");

        var validInfos = cqrsInfos.Where(c => c is not null && !c.IsIncomplete()).ToList();

        var eventHandlers = validInfos
            .Where(c => c.EventHandlers.Length > 0)
            .Select(c => new EventHandlerInfo(c.FullTypeName, c.Namespace, c.EventHandlers))
            .ToList();

        var integrationEvents = validInfos
            .SelectMany(c => c.IntegrationEvents)
            .Distinct()
            .ToList();

        var allEventTypes = eventHandlers
            .SelectMany(e => e.EventTypes)
            .Concat(integrationEvents)
            .Distinct()
            .ToList();

        var topicMappings = validInfos
            .Where(c => c.Topics.Length > 0)
            .Distinct()
            .ToList();


        // ✅ Crear un EventHandlerInfo consolidado con TODOS los eventos
        var consolidatedEventInfo = new EventHandlerInfo(
            "EventDispatcher",
            "CQRS.AutoGenerated",
            allEventTypes.ToImmutableArray());

        // Generate EventDispatcher (now includes deserialization)
        var eventDispatcherSource = EventDispatcherHelper.GenerateCode([consolidatedEventInfo]);
        context.AddSource("EventDispatcher.g.cs", SourceText.From(eventDispatcherSource, Encoding.UTF8));

        //Generate Dependency Injection code
        var source = DependencyInjectionHelper.GenerateCode();
        context.AddSource("ServiceCollectionExtensions.g.cs", SourceText.From(source, Encoding.UTF8));

        var topicsSource = TopicToCommandHelper.GenerateCode(topicMappings);
        context.AddSource("TopicToCommand.g.cs", SourceText.From(topicsSource, Encoding.UTF8));
    }

    private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol @namespace)
    {
        foreach (var type in @namespace.GetTypeMembers())
        {
            yield return type;

            foreach (var nestedType in GetNestedTypes(type))
                yield return nestedType;
        }

        foreach (var nestedNamespace in @namespace.GetNamespaceMembers())
        {
            foreach (var type in GetAllTypes(nestedNamespace))
                yield return type;
        }
    }

    private static IEnumerable<INamedTypeSymbol> GetNestedTypes(INamedTypeSymbol type)
    {
        foreach (var nestedType in type.GetTypeMembers())
        {
            yield return nestedType;

            foreach (var deeplyNested in GetNestedTypes(nestedType))
                yield return deeplyNested;
        }
    }


    private static string ExtractTopicFromConstructor(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
    {
        // Handle primary constructor: class Foo() : Base(arg)
        var primaryBase = classDeclaration.BaseList?.Types
            .OfType<PrimaryConstructorBaseTypeSyntax>()
            .FirstOrDefault();

        if (primaryBase is not null)
        {
            var firstArg = primaryBase.ArgumentList.Arguments.FirstOrDefault();
            if (firstArg is not null)
                return ResolveTopicArgument(firstArg, classDeclaration, semanticModel);
        }

        // Handle regular constructor with base() initializer
        foreach (var constructor in classDeclaration.Members.OfType<ConstructorDeclarationSyntax>())
        {
            var baseInitializer = constructor.Initializer;
            if (baseInitializer?.ThisOrBaseKeyword.IsKind(SyntaxKind.BaseKeyword) != true)
                continue;

            var firstArg = baseInitializer.ArgumentList.Arguments.FirstOrDefault();
            if (firstArg is null)
                continue;

            return ResolveTopicArgument(firstArg, classDeclaration, semanticModel);
        }

        return string.Empty;
    }

    private static string ResolveTopicArgument(ArgumentSyntax arg, ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
    {
        // Best: let the semantic model resolve any constant expression (covers MyOtherClass.ConstField, local consts, literals, etc.)
        var constantValue = semanticModel.GetConstantValue(arg.Expression);
        if (constantValue.HasValue && constantValue.Value is string topicValue)
            return topicValue;

        // Fallback: direct string literal
        if (arg.Expression is LiteralExpressionSyntax literal &&
            literal.IsKind(SyntaxKind.StringLiteralExpression))
            return literal.Token.ValueText;

        // Fallback: const field declared in the same class
        if (arg.Expression is IdentifierNameSyntax identifier)
        {
            var field = classDeclaration.Members
                .OfType<FieldDeclarationSyntax>()
                .Where(f => f.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword)))
                .SelectMany(f => f.Declaration.Variables)
                .FirstOrDefault(v => v.Identifier.Text == identifier.Identifier.Text);

            if (field?.Initializer?.Value is LiteralExpressionSyntax fieldLiteral)
                return fieldLiteral.Token.ValueText;
        }

        return string.Empty;
    }
}
