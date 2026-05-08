using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using YAMCqrs.Core.SourceGeneration.GeneratorHelper;
using YAMCqrs.Core.SourceGeneration.Info;

namespace YAMCqrs.Core.SourceGeneration;

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

    private static CqrsInfo GetCqrsInfo(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol symbol)
            return null;

        if (symbol.TypeKind != TypeKind.Class)
        {
            Debug.WriteLine($"Skipping non-class type: {symbol.Name} (TypeKind: {symbol.TypeKind})");
            return null;
        }

        if (symbol.IsAbstract)
        {
            Debug.WriteLine($"Skipping abstract class: {symbol.Name}");
            return null;
        }

        var commandHandlers = new List<(string CommandType, string ResultType)>();
        var queryHandlers = new List<(string QueryType, string ResultType)>();
        var commandInterceptors = new List<(string CommandType, string ResultType)>();
        var queryInterceptors = new List<(string QueryType, string ResultType)>();
        var eventHandlers = new List<string>();
        var integrationEvents = new List<string>();
        var isGenericDefinition = symbol.IsGenericType;

        foreach (var @interface in symbol.AllInterfaces)
        {
            var interfaceName = @interface.ConstructedFrom.ToDisplayString();

            // DEBUG: See which interfaces are being detected
            Debug.WriteLine($"Found interface: {interfaceName} on class: {symbol.Name}");

            switch (interfaceName)
            {
                case Const.InterfaceNames.CommandHandler:
                    commandHandlers.Add((
                        @interface.TypeArguments[0].ToDisplayString(),
                        @interface.TypeArguments[1].ToDisplayString()));
                    break;

                case Const.InterfaceNames.QueryHandler:
                    queryHandlers.Add((
                        @interface.TypeArguments[0].ToDisplayString(),
                        @interface.TypeArguments[1].ToDisplayString()));
                    break;

                case Const.InterfaceNames.CommandInterceptor:
                    commandInterceptors.Add((
                        @interface.TypeArguments[0].ToDisplayString(),
                        @interface.TypeArguments[1].ToDisplayString()));
                    break;

                case Const.InterfaceNames.QueryInterceptor:
                    queryInterceptors.Add((
                        @interface.TypeArguments[0].ToDisplayString(),
                        @interface.TypeArguments[1].ToDisplayString()));
                    break;
            }
        }

        // Solo retornar si es handler O interceptor O event handler
        if (commandHandlers.Count == 0 && queryHandlers.Count == 0 &&
            commandInterceptors.Count == 0 && queryInterceptors.Count == 0 &&
            eventHandlers.Count == 0 && integrationEvents.Count == 0)
            return null;

        Debug.WriteLine($"Registered CQRS class: {symbol.Name} (Handlers: {commandHandlers.Count + queryHandlers.Count}, Interceptors: {commandInterceptors.Count + queryInterceptors.Count})");

        return new CqrsInfo(
            symbol.ToDisplayString(),
            symbol.ContainingNamespace.ToDisplayString(),
            commandHandlers.ToImmutableArray(),
            queryHandlers.ToImmutableArray(),
            commandInterceptors.ToImmutableArray(),
            queryInterceptors.ToImmutableArray(),
            eventHandlers.ToImmutableArray(),
            integrationEvents.ToImmutableArray(),
            isGenericDefinition);
    }

    private static void Execute(Compilation compilation, ImmutableArray<CqrsInfo> cqrsInfos, SourceProductionContext context)
    {
        if (cqrsInfos.IsDefaultOrEmpty)
            Debug.WriteLine("No CQRS classes found.");

        var validInfos = cqrsInfos.Where(c => c is not null && !c.IsIncomplete()).ToList();

        // Separar handlers, interceptors y event handlers
        var handlers = validInfos
            .Where(c => c.CommandHandlers.Length > 0 || c.QueryHandlers.Length > 0)
            .Select(c => new HandlerInfo(c.FullTypeName, c.Namespace, c.CommandHandlers, c.QueryHandlers))
            .ToList();

        var interceptors = validInfos
            .Where(c => c.CommandInterceptors.Length > 0 || c.QueryInterceptors.Length > 0)
            .Select(c => new InterceptorInfo(c.FullTypeName, c.Namespace, c.CommandInterceptors, c.QueryInterceptors, c.IsGenericDefinition))
            .ToList();


        var referencedInterceptors = ScanReferencedAssemblies(compilation);
        interceptors.AddRange(referencedInterceptors);

        //Generate Dispatcher code
        var dispatcherSource = DispatcherHelper.GenerateCode(handlers);
        context.AddSource("Dispatcher.g.cs", SourceText.From(dispatcherSource, Encoding.UTF8));

        //Generate Dependency Injection code
        var source = DependencyInjectionHelper.GenerateCode(handlers, interceptors);
        context.AddSource("ServiceCollectionExtensions.g.cs", SourceText.From(source, Encoding.UTF8));

        var suppressionSource = SuppressionsHelper.GenerateCode(handlers, interceptors);
        if (!string.IsNullOrEmpty(suppressionSource))
        {
            context.AddSource("GlobalSuppressions.g.cs", SourceText.From(suppressionSource, Encoding.UTF8));
        }
    }

    private static List<InterceptorInfo> ScanReferencedAssemblies(Compilation compilation)
    {
        var interceptors = new List<InterceptorInfo>();

        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assembly)
                continue;

            // Find public types that implement ICommandInterceptor or IQueryInterceptor
            var types = GetAllTypes(assembly.GlobalNamespace);

            foreach (var type in types)
            {
                // FIX: Skip interfaces - they cannot be instantiated
                if (type.TypeKind != TypeKind.Class)
                {
                    Debug.WriteLine($"Skipping non-class type in referenced assembly: {type.Name} (TypeKind: {type.TypeKind})");
                    continue;
                }

                //FIX: Skip abstract classes - they cannot be instantiated
                if (type.IsAbstract)
                {
                    Debug.WriteLine($"Skipping abstract class in referenced assembly: {type.Name}");
                    continue;
                }

                if (type.DeclaredAccessibility != Accessibility.Public)
                    continue;

                var commandInterceptors = new List<(string, string)>();
                var queryInterceptors = new List<(string, string)>();

                foreach (var @interface in type.AllInterfaces)
                {
                    var interfaceName = @interface.ConstructedFrom.ToDisplayString();

                    if (interfaceName == Const.InterfaceNames.CommandInterceptor)
                    {
                        commandInterceptors.Add((
                            @interface.TypeArguments[0].ToDisplayString(),
                            @interface.TypeArguments[1].ToDisplayString()));
                    }
                    else if (interfaceName == Const.InterfaceNames.QueryInterceptor)
                    {
                        queryInterceptors.Add((
                            @interface.TypeArguments[0].ToDisplayString(),
                            @interface.TypeArguments[1].ToDisplayString()));
                    }
                }

                if (commandInterceptors.Count > 0 || queryInterceptors.Count > 0)
                {
                    Debug.WriteLine($"Registered interceptor from referenced assembly: {type.Name}");
                    interceptors.Add(new InterceptorInfo(
                        type.ToDisplayString(),
                        type.ContainingNamespace.ToDisplayString(),
                        commandInterceptors.ToImmutableArray(),
                        queryInterceptors.ToImmutableArray(),
                        type.IsGenericType));
                }
            }
        }

        return interceptors;
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
}
