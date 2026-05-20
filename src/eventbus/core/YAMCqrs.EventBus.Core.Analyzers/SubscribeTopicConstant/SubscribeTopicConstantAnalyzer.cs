using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace YAMCqrs.EventBus.Core.Analyzers.SubscribeTopicConstant;

/// <summary>
/// Analyzer that ensures KafkaSubscribeEvent implementations pass a constant topic value to the base constructor.
/// Valid patterns:
/// - base("my.topic.name")
/// - base(TopicName) where TopicName is a const field
/// - base(MyOtherClass.TopicName) where TopicName is a const field
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class KafkaSubscribeTopicConstantAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = "SubscribeEvent constructor must receive a constant topic value";
    private static readonly LocalizableString MessageFormat = "Constructor in '{0}' must pass a string literal or a const field to base(). The topic value should be fixed at compile time.";
    private static readonly LocalizableString Description = "Constructors in classes inheriting from SubscribeEvent must pass a constant topic value (string literal or const field) to the base constructor to ensure topic names are known at compile time.";
    private const string Category = "Design";

    private static readonly DiagnosticDescriptor Rule = new(
        ConstDiagnostics.SubscribeTopicConstant,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        //Debugger.Launch();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        // RegisterSyntaxNodeAction provides context.SemanticModel directly,
        // which avoids the RS1030 violation caused by Compilation.GetSemanticModel().
        context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        if (context.SemanticModel
            .GetDeclaredSymbol(classDeclaration, context.CancellationToken) is not INamedTypeSymbol namedTypeSymbol)
            return;

        // Skip abstract classes
        if (namedTypeSymbol.IsAbstract || namedTypeSymbol.IsTupleType || namedTypeSymbol.IsStatic)
            return;

        // Check if the class inherits from KafkaSubscribeEvent
        if (!InheritsFromSubscribeEvent(namedTypeSymbol))
            return;

        if (CheckPrimaryConstructor(classDeclaration, context, namedTypeSymbol))
            return;

        // Resolve constructors directly from the class declaration syntax so that
        // every constructor node belongs to the same syntax tree as the class,
        // allowing context.SemanticModel to be used without any extra lookup.
        var constructorSyntaxNodes = classDeclaration.Members
        .OfType<ConstructorDeclarationSyntax>()
        .Where(c => !c.Modifiers.Any(SyntaxKind.StaticKeyword));

        foreach (var constructorSyntax in constructorSyntaxNodes)
        {
            if (constructorSyntax.Initializer is null)
                continue;

            // Only check base(...) initializers
            if (!constructorSyntax.Initializer.ThisOrBaseKeyword.IsKind(SyntaxKind.BaseKeyword))
                continue;

            var arguments = constructorSyntax.Initializer.ArgumentList.Arguments;
            if (arguments.Count == 0)
                continue;

            // Get the first argument (topic parameter)
            var topicArgument = arguments[0];

            if (!IsConstantExpression(topicArgument, context.SemanticModel, context, namedTypeSymbol.Name))
            {
                return;
            }
        }
    }

    private static bool CheckPrimaryConstructor(ClassDeclarationSyntax classDeclaration, SyntaxNodeAnalysisContext context, INamedTypeSymbol namedTypeSymbol)
    {
        var primaryConstructor = classDeclaration.ParameterList;
        if (primaryConstructor is null)
            return false;

        var baseType = classDeclaration.BaseList?.Types.OfType<PrimaryConstructorBaseTypeSyntax>().FirstOrDefault();

        if (baseType is null)
            return false;

        var topicItem = baseType?.ArgumentList.Arguments.FirstOrDefault();

        if (topicItem is null)
            return false;

        if (!IsConstantExpression(topicItem, context.SemanticModel, context, namedTypeSymbol.Name))
            return true;

        var ins = topicItem.Expression;
        if (ins is not IdentifierNameSyntax idf)
            return false;

        foreach (var parameter in primaryConstructor?.Parameters ?? new())
        {
            var name = parameter.Identifier.Text;

            if (idf.Identifier.Text == name)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                Rule,
                parameter.GetLocation(),
                namedTypeSymbol.Name));

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines if an expression is a constant value (string literal or const field).
    /// </summary>
    private static bool IsConstantExpression(ArgumentSyntax argSyntax, SemanticModel semanticModel, SyntaxNodeAnalysisContext context, string name)
    {
        var expression = argSyntax.Expression;
        // Case 1: String literal - base("my.topic.name")
        if (expression is LiteralExpressionSyntax literal &&
            literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            return true;
        }

        // Case 2: Identifier referencing a const field or local const variable
        if (expression is IdentifierNameSyntax identifier)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(identifier);

            if (symbolInfo.Symbol is IFieldSymbol fieldSymbol)
                return fieldSymbol.IsConst;

            if (symbolInfo.Symbol is ILocalSymbol localSymbol)
                return localSymbol.IsConst;
        }

        // Case 3: Member access to a const field (e.g., MyOutputEventKafka.TopicName)
        if (expression is MemberAccessExpressionSyntax memberAccess)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(memberAccess);

            if (symbolInfo.Symbol is IFieldSymbol fieldSymbol)
                return fieldSymbol.IsConst;
        }

        context.ReportDiagnostic(Diagnostic.Create(
             Rule,
             argSyntax.GetLocation(),
             name));

        return false;
    }

    /// <summary>
    /// Checks if a type inherits from SubscribeEvent.
    /// </summary>
    private static bool InheritsFromSubscribeEvent(INamedTypeSymbol typeSymbol)
    {
        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            if (baseType.Name == "SubscribeEvent" &&
                baseType.ContainingNamespace?.ToDisplayString() == "YAMCqrs.EventBus.Core.SubscribeEvents.Abstractions")
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }
}