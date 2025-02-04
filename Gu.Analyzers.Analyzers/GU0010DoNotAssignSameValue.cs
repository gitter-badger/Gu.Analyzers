﻿namespace Gu.Analyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class GU0010DoNotAssignSameValue : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GU0010";
        private const string Title = "Assigning same value.";
        private const string MessageFormat = "Assigning made to same, did you mean to assign something else?";
        private const string Description = "Assigning same value does not make sense.";
        private static readonly string HelpLink = Analyzers.HelpLink.ForId(DiagnosticId);

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            AnalyzerCategory.Correctness,
            DiagnosticSeverity.Error,
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
            context.RegisterSyntaxNodeAction(HandleAssignment, SyntaxKind.SimpleAssignmentExpression);
        }

        private static void HandleAssignment(SyntaxNodeAnalysisContext context)
        {
            var assignment = (AssignmentExpressionSyntax)context.Node;
            if (assignment.IsMissing)
            {
                return;
            }

            if (AreSame(assignment.Left, assignment.Right))
            {
                if (assignment.FirstAncestorOrSelf<InitializerExpressionSyntax>() != null)
                {
                    return;
                }

                var left = context.SemanticModel.GetSymbolInfo(assignment.Left).Symbol;
                var right = context.SemanticModel.GetSymbolInfo(assignment.Right).Symbol;
                if (!ReferenceEquals(left, right))
                {
                    return;
                }

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, assignment.GetLocation()));
            }
        }

        private static bool AreSame(ExpressionSyntax left, ExpressionSyntax right)
        {
            IdentifierNameSyntax leftName;
            IdentifierNameSyntax rightName;
            if (TryGetIdentifierName(left, out leftName) ^ TryGetIdentifierName(right, out rightName))
            {
                return false;
            }

            if (leftName != null)
            {
                return leftName.Identifier.ValueText == rightName.Identifier.ValueText;
            }

            var leftMember = left as MemberAccessExpressionSyntax;
            var rightMember = right as MemberAccessExpressionSyntax;
            if (leftMember == null || rightMember == null)
            {
                return false;
            }

            return AreSame(leftMember.Name, rightMember.Name) && AreSame(leftMember.Expression, rightMember.Expression);
        }

        private static bool TryGetIdentifierName(ExpressionSyntax expression, out IdentifierNameSyntax result)
        {
            result = expression as IdentifierNameSyntax;
            if (result != null)
            {
                return true;
            }

            var memberAccess = expression as MemberAccessExpressionSyntax;
            if (memberAccess?.Expression is ThisExpressionSyntax)
            {
                return TryGetIdentifierName(memberAccess.Name, out result);
            }

            return false;
        }
    }
}