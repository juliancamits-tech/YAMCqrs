using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Linq;
using YAMCqrs.EventBus.Provider.RabbitMq.SourceGeneration.GeneratorHelper;

namespace YAMCqrs.EventBus.Provider.RabbitMq.SourceGeneration;

/// <summary>
/// Main source generator for Rabbit.
/// </summary>
[Generator]
public class MainGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the source generator for Rabbit.
    /// </summary>
    /// <param name="context"></param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG
        //Debugger.Launch();
#endif
        Debug.WriteLine("Rabbit Topics Source Generator initialized.");

        // Detectar todas las clases que hereden de RabbitInputEvent
        var RabbitInputEventClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsClassDeclaration(node),
                transform: static (ctx, _) => GetClassWithTopicIfInheritsFrom(ctx, Const.InterfacesNames.RabbitSubscribeEvent.Split('.').Last()))
            .Where(static m => m.HasValue)
            .Collect();

        // Registrar la generación de código
        context.RegisterSourceOutput(RabbitInputEventClasses, static (spc, sources) =>
        {
            var topics = sources
                .Where(e => e.HasValue)
                .Select(e => e.Value.Topic)
                .Where(t => !string.IsNullOrEmpty(t))
                .Distinct()
                .ToArray();

            Debug.WriteLine($"Extracted {topics.Length} topics: {string.Join(", ", topics)}");

            var sourceCode = DependencyInjectionHelper.GenerateCode(topics);
            spc.AddSource("RabbitServiceCollectionExtension.g.cs", sourceCode);
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
        var routingKey = ExtractRoutingKey(classDeclaration, semanticModel);
        Debug.WriteLine($"Found RabbitSubscribeEvent: {classSymbol.Name} with topic: {topic} and routingKey: {routingKey}");

        return (classSymbol, $"{topic}||{routingKey}");
    }

    private static string ExtractRoutingKey(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
    {
        var prop = classDeclaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(x => x.Identifier.Text == "RoutingKey");

        if (prop is null)
            return string.Empty;

        ExpressionSyntax expr = null;

        // Expression-bodied property: => "pepe"
        if (prop.ExpressionBody?.Expression is not null)
        {
            expr = prop.ExpressionBody.Expression;
        }
        else if (prop.AccessorList is not null)
        {
            // Buscar el getter
            var getter = prop.AccessorList.Accessors.FirstOrDefault(a => a.Keyword.Text == "get");
            if (getter is not null)
            {
                // Expression-bodied getter: get => "pepe";
                if (getter.ExpressionBody?.Expression is not null)
                {
                    expr = getter.ExpressionBody.Expression;
                }
                else if (getter.Body is not null)
                {
                    // Buscar return dentro del body: get { return "pepe"; }
                    var returnStmt = getter.Body.Statements
                        .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ReturnStatementSyntax>()
                        .FirstOrDefault();
                    if (returnStmt is not null)
                        expr = returnStmt.Expression;
                }
            }
        }

        if (expr is null)
            return string.Empty;

        // Intentar obtener valor constante mediante SemanticModel
        var constantValue = semanticModel.GetConstantValue(expr);
        if (constantValue.HasValue && constantValue.Value is string constStr)
            return constStr;

        // Literal directo
        if (expr is LiteralExpressionSyntax literal)
            return literal.Token.ValueText;

        // Identificador o acceso a miembro: intentar resolver símbolo y ver si es const
        var symbolInfo = semanticModel.GetSymbolInfo(expr);
        var symbol = symbolInfo.Symbol;

        if (symbol is IFieldSymbol fieldSymbol && fieldSymbol.IsConst && fieldSymbol.ConstantValue is string fieldConst)
            return fieldConst;

        if (symbol is ILocalSymbol localSymbol && localSymbol.IsConst && localSymbol.ConstantValue is string localConst)
            return localConst;

        if (symbol is IPropertySymbol propertySymbol)
        {
            // Las propiedades no pueden ser const; pero si la propiedad tiene un getter constante evaluable por el compilador,
            // semanticModel.GetConstantValue ya lo habría devuelto. No hay más que hacer aquí.
        }

        // Si es un MemberAccess o Identifier que apunta a un miembro const en otra declaración (ej: MyEvent.RoutingKeyName)
        if (expr is MemberAccessExpressionSyntax || expr is IdentifierNameSyntax)
        {
            var referenced = semanticModel.GetSymbolInfo(expr).Symbol;
            if (referenced is IFieldSymbol refField && refField.IsConst && refField.ConstantValue is string refConst)
                return refConst;
        }

        return string.Empty;
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

        // Si es un miembro accedido (como MyOutputEventRabbit.TopicName)
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
