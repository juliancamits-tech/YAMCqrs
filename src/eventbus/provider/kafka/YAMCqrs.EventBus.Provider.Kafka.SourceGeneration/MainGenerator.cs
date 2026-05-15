using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Diagnostics;
using YAMCqrs.EventBus.Provider.Kafka.SourceGeneration.GeneratorHelper;

namespace YAMCqrs.EventBus.Provider.Kafka.SourceGeneration;

/// <summary>
/// Main source generator for Kafka.
/// </summary>
[Generator]
public class MainGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the source generator for Kafka.
    /// </summary>
    /// <param name="context"></param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG
        //Debugger.Launch();
#endif
        Debug.WriteLine("Kafka Topics Source Generator initialized.");

        // Detectar todas las clases que hereden de KafkaInputEvent
        var kafkaInputEventClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsClassDeclaration(node),
                transform: static (ctx, _) => GetClassWithTopicIfInheritsFrom(ctx, Const.InterfacesNames.KafkaSubscribeEvent.Split('.').Last()))
            .Where(static m => m.HasValue)
            .Collect();

        // Registrar la generación de código
        context.RegisterSourceOutput(kafkaInputEventClasses, static (spc, sources) =>
        {
            var topics = sources
                .Where(e => e.HasValue)
                .Select(e => e.Value.Topic)
                .Where(t => !string.IsNullOrEmpty(t))
                .Distinct()
                .ToArray();

            Debug.WriteLine($"Extracted {topics.Length} topics: {string.Join(", ", topics)}");

            var sourceCode = DependencyInjectionHelper.GenerateCode(topics);
            spc.AddSource("KafkaServiceCollectionExtension.g.cs", sourceCode);
        });
    }

    private static bool IsClassDeclaration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax classDeclaration &&
               classDeclaration.BaseList is not null;
    }

    private static (INamedTypeSymbol Symbol, string Topic)? GetClassWithTopicIfInheritsFrom(
        GeneratorSyntaxContext context,
        string baseClassName)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        if (semanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
            return null;

        // Verificar si la clase hereda directa o indirectamente de la clase base
        if (!InheritsFrom(classSymbol, baseClassName))
            return null;

        // Extraer el topic del constructor base
        var topic = ExtractTopicFromConstructor(classDeclaration, semanticModel);

        Debug.WriteLine($"Found KafkaSubscribeEvent: {classSymbol.Name} with topic: {topic}");

        return (classSymbol, topic);
    }

    private static string ExtractTopicFromConstructor(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
    {
        // Buscar en los constructores primarios (C# 12+)
        if (classDeclaration.ParameterList is not null)
        {
            var baseList = classDeclaration.BaseList;
            if (baseList is not null)
            {
                foreach (var baseType in baseList.Types)
                {
                    if (baseType is PrimaryConstructorBaseTypeSyntax primaryCtor)
                    {
                        var argument = primaryCtor.ArgumentList?.Arguments.FirstOrDefault();
                        if (argument?.Expression is not null)
                        {
                            var topic = ExtractConstantValue(argument.Expression, semanticModel);
                            if (!string.IsNullOrEmpty(topic))
                                return topic;
                        }
                    }
                }
            }
        }

        // Buscar en constructores explícitos
        var constructor = classDeclaration.Members
            .OfType<ConstructorDeclarationSyntax>()
            .FirstOrDefault();

        if (constructor?.Initializer?.ArgumentList?.Arguments.FirstOrDefault()?.Expression is ExpressionSyntax expression)
        {
            var topic = ExtractConstantValue(expression, semanticModel);
            if (!string.IsNullOrEmpty(topic))
                return topic;
        }

        return string.Empty;
    }

    private static string ExtractConstantValue(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        // Intentar obtener el valor constante usando el semantic model
        var constantValue = semanticModel.GetConstantValue(expression);
        if (constantValue.HasValue && constantValue.Value is string strValue)
        {
            return strValue;
        }

        // Si no es una constante, intentar con literales directos
        if (expression is LiteralExpressionSyntax literal)
        {
            return literal.Token.ValueText;
        }

        // Si es un miembro accedido (como MyOutputEventKafka.TopicName)
        if (expression is MemberAccessExpressionSyntax memberAccess)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(memberAccess);
            if (symbolInfo.Symbol is IFieldSymbol fieldSymbol && fieldSymbol.IsConst)
            {
                if (fieldSymbol.ConstantValue is string constValue)
                {
                    return constValue;
                }
            }
        }

        return string.Empty;
    }

    private static bool InheritsFrom(INamedTypeSymbol classSymbol, string baseClassName)
    {
        //Ignorar interfaces, enums, structs - solo procesar clases
        if (classSymbol.TypeKind != TypeKind.Class || classSymbol.IsAbstract)
            return false;
        
        var currentType = classSymbol.BaseType;

        while (currentType is not null)
        {
            if (currentType.Name == baseClassName)
                return true;

            currentType = currentType.BaseType;
        }

        return false;
    }
}