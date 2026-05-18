using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace YAMCqrs.BackgroundWorker.Analyzers.BackgroundServiceUsage;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BackgroundServiceUsageCodeFixProvider)), Shared]
public class BackgroundServiceUsageCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(BackgroundServiceUsageAnalyzer.DiagnosticId);

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var baseTypeSyntax = root?.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf()
            .OfType<SimpleBaseTypeSyntax>().First();

        if (baseTypeSyntax == null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Replace with YABackgroundWorker<TWorkItem>",
                createChangedDocument: c => ReplaceWithYABackgroundWorker(context.Document, baseTypeSyntax, c),
                equivalenceKey: nameof(BackgroundServiceUsageCodeFixProvider)),
            diagnostic);
    }

    private async Task<Document> ReplaceWithYABackgroundWorker(
        Document document,
        SimpleBaseTypeSyntax baseTypeSyntax,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
            return document;

        // Create the new base type: YABackgroundWorker<TWorkItem>
        var newBaseType = SyntaxFactory.SimpleBaseType(
            SyntaxFactory.GenericName(
                SyntaxFactory.Identifier("YABackgroundWorker"))
            .WithTypeArgumentList(
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                        SyntaxFactory.IdentifierName("TWorkItem")))));

        var newRoot = root.ReplaceNode(baseTypeSyntax, newBaseType);
        return document.WithSyntaxRoot(newRoot);
    }
}