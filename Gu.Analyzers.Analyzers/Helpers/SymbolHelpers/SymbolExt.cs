﻿namespace Gu.Analyzers
{
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.CodeAnalysis;

    internal static class SymbolExt
    {
        internal static bool TryGetSingleDeclaration<T>(this ISymbol symbol, CancellationToken cancellationToken, out T declaration)
            where T : SyntaxNode
        {
            SyntaxReference syntaxReference;
            if (symbol.DeclaringSyntaxReferences.TryGetSingle(out syntaxReference))
            {
                declaration = (T)syntaxReference.GetSyntax(cancellationToken);
                return declaration != null;
            }

            declaration = null;
            return false;
        }

        internal static IEnumerable<SyntaxNode> Declarations(this ISymbol symbol, CancellationToken cancellationToken)
        {
            foreach (var syntaxReference in symbol.DeclaringSyntaxReferences)
            {
                yield return syntaxReference.GetSyntax(cancellationToken);
            }
        }
    }
}
