namespace Gu.Analyzers
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class GU0004AssignAllReadOnlyMembers : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GU0004";
        private const string Title = "Assign all readonly members.";
        private const string MessageFormat = "Assign all readonly members.";
        private const string Description = "Assign all readonly members.";
        private static readonly string HelpLink = Analyzers.HelpLink.ForId(DiagnosticId);

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            AnalyzerCategory.Correctness,
            DiagnosticSeverity.Hidden,
            AnalyzerConstants.EnabledByDefault,
            Description,
            HelpLink);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleConstructor, SyntaxKind.ConstructorDeclaration);
        }

        private static void HandleConstructor(SyntaxNodeAnalysisContext context)
        {
            var constructorDeclarationSyntax = (ConstructorDeclarationSyntax)context.Node;
            if (constructorDeclarationSyntax.ParameterList.Parameters.Count == 0)
            {
                return;
            }

            var walker = new CtorWalker(constructorDeclarationSyntax, context.SemanticModel, context.CancellationToken);
            walker.Visit(constructorDeclarationSyntax);
            if (walker.readOnlies.Any())
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, constructorDeclarationSyntax.GetLocation()));
            }
        }

        private class CtorWalker : CSharpSyntaxWalker
        {
            private readonly ConstructorDeclarationSyntax ctor;
            internal readonly List<string> readOnlies;

            public CtorWalker(ConstructorDeclarationSyntax ctor, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                this.ctor = ctor;
                this.readOnlies = ReadOnlies(ctor, semanticModel, cancellationToken).ToList();
            }

            public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
            {
                IdentifierNameSyntax left;
                if (TryGetIdentifier(node.Left, out left))
                {
                    this.readOnlies.Remove(left.Identifier.ValueText);
                }

                base.VisitAssignmentExpression(node);
            }

            private static IEnumerable<string> ReadOnlies(ConstructorDeclarationSyntax ctor, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                var classDeclarationSyntax = (ClassDeclarationSyntax)ctor.Parent;
                foreach (var member in classDeclarationSyntax.Members)
                {
                    var fieldDeclarationSyntax = member as FieldDeclarationSyntax;
                    if (fieldDeclarationSyntax != null)
                    {
                        var symbol = (IFieldSymbol)semanticModel.GetDeclaredSymbol(fieldDeclarationSyntax.Declaration.Variables[0], cancellationToken);
                        if (symbol.IsReadOnly && !symbol.IsStatic)
                        {
                            yield return fieldDeclarationSyntax.Identifier().ValueText;
                        }
                    }

                    var propertyDeclarationSyntax = member as PropertyDeclarationSyntax;
                    if (propertyDeclarationSyntax != null)
                    {
                        var symbol = (IPropertySymbol)semanticModel.GetDeclaredSymbol(propertyDeclarationSyntax, cancellationToken);
                        if (symbol.IsReadOnly && !symbol.IsStatic)
                        {
                            yield return propertyDeclarationSyntax.Identifier().ValueText;
                        }
                    }
                }
            }

            private static bool TryGetIdentifier(ExpressionSyntax expression, out IdentifierNameSyntax result)
            {
                result = expression as IdentifierNameSyntax;
                if (result != null)
                {
                    return true;
                }

                var member = expression as MemberAccessExpressionSyntax;
                if (member?.Expression is ThisExpressionSyntax)
                {
                    return TryGetIdentifier(member.Name, out result);
                }

                return false;
            }
        }
    }
}