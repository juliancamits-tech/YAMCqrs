using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace YAMCqrs.BackgroundWorker.Analyzers.BackgroundServiceUsage;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BackgroundServiceUsageAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = ConstDiagnostics.BackgroundServiceUsage;
    private const string Category = "Usage";

    private static readonly LocalizableString Title = "Direct BackgroundService inheritance is not allowed";
    private static readonly LocalizableString MessageFormat = "Do not inherit directly from BackgroundService. Use YABackgroundWorker instead.";
    private static readonly LocalizableString Description = "Classes should inherit from YABackgroundWorker instead of BackgroundService directly.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        // Skip if the class doesn't have a base list
        if (classDeclaration.BaseList == null)
            return;

        var semanticModel = context.SemanticModel;
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

        if (classSymbol == null)
            return;

        // Check if this is YABackgroundWorker itself (the exception)
        if (IsYABackgroundWorker(classSymbol))
            return;

        // Check each base type
        foreach (var baseType in classDeclaration.BaseList.Types)
        {
            var typeInfo = semanticModel.GetTypeInfo(baseType.Type);

            if (typeInfo.Type is not INamedTypeSymbol baseTypeSymbol)
                continue;

            // Check if inheriting directly from BackgroundService
            if (IsBackgroundService(baseTypeSymbol))
            {
                var diagnostic = Diagnostic.Create(Rule, baseType.GetLocation(), classSymbol.Name);
                context.ReportDiagnostic(diagnostic);
                return;
            }
        }
    }

    private static bool IsBackgroundService(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.Name == "BackgroundService" &&
               typeSymbol.ContainingNamespace?.ToString() == "Microsoft.Extensions.Hosting";
    }

    private static bool IsYABackgroundWorker(INamedTypeSymbol classSymbol)
    {
        return classSymbol.Name == "YABackgroundWorker" &&
               classSymbol.ContainingNamespace?.ToString()?.Contains("YAMCqrs.BackgroundWorker.Core.Abstractions") == true;
    }
}
