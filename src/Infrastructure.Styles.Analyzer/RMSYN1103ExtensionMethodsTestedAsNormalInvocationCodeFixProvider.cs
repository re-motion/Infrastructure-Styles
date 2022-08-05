// This file is part of the re-motion Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-motion Framework is free software; you can redistribute it
// and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 2.1 of the
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace Infrastructure.Styles.Analyzer
{
    public class RMSYN1103ExtensionMethodsTestedAsNormalInvocationCodeFixProvider : CodeFixProvider
    {
        public override FixAllProvider? GetFixAllProvider ()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override Task RegisterCodeFixesAsync (CodeFixContext context)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Fix Extension method invocation",
                    c => CodeFix(c, context),
                    RMSYN1103ExtensionMethodsTestedAsNormalInvocationSyntaxAnalyzer.DiagnosticId),
                context.Diagnostics.First());
            return Task.CompletedTask;
        }

        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(RMSYN1103ExtensionMethodsTestedAsNormalInvocationSyntaxAnalyzer.DiagnosticId);

        private static async Task<Document> CodeFix (CancellationToken cancellationToken, CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var syntaxRoot = await context.Document.GetSyntaxRootAsync(cancellationToken);
            if (syntaxRoot == null)
                return context.Document;

            var oldNode = syntaxRoot.FindNode(diagnosticSpan).FirstAncestorOrSelf<InvocationExpressionSyntax>()!;

            var semanticModel = await context.Document.GetSemanticModelAsync(cancellationToken);
            if (semanticModel == null)
                return context.Document;

            var symbolInfo = semanticModel.GetSymbolInfo(oldNode, cancellationToken);
            if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
                return context.Document;

            var containingType = methodSymbol.ContainingType;
            if (containingType == null)
                return context.Document;

            var expression = CreateInvocationExpressionNamespaceTree(oldNode, containingType);
            if (expression == null)
                return context.Document;

            var invocation = (MemberAccessExpressionSyntax)oldNode.Expression;

            var nodeWithNewArguments = NodeWithNewFirstArgument(oldNode.WithExpression(expression), SyntaxFactory.Argument(invocation.Expression));

            var newSyntaxRoot = syntaxRoot.ReplaceNode(oldNode, nodeWithNewArguments);
            
            return context.Document.WithSyntaxRoot(newSyntaxRoot);
        }

        private static MemberAccessExpressionSyntax? CreateInvocationExpressionNamespaceTree (InvocationExpressionSyntax oldNode, INamedTypeSymbol containingType)
        {
            var invocationChildExpression = (MemberAccessExpressionSyntax)oldNode.Expression;

            var stack = new Stack<SimpleNameSyntax>();
            stack.Push(invocationChildExpression!.Name);
            stack.Push(SyntaxFactory.IdentifierName(containingType.Name));

            var containingNamespace = containingType.ContainingNamespace;
            while (containingNamespace != null)
            {
                if (!containingNamespace.IsGlobalNamespace && (containingNamespace.Name != string.Empty))
                    stack.Push(SyntaxFactory.IdentifierName(containingNamespace.Name));
                containingNamespace = containingNamespace.ContainingNamespace;
            }

            if (stack.Count < 2)
                return null;

            var newExpression = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression, 
				stack.Pop().WithTriviaFrom(invocationChildExpression).WithoutTrailingTrivia(), 
				stack.Pop())
					.WithAdditionalAnnotations(Simplifier.Annotation);

            while (stack.Any())
            {
                newExpression = SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression, newExpression,
                    stack.Pop());
            }
            return newExpression;
        }

        private static InvocationExpressionSyntax NodeWithNewFirstArgument (InvocationExpressionSyntax oldNode, ArgumentSyntax firstArgument)
        {
            if(oldNode.ArgumentList.Arguments.Count > 0)
                firstArgument = firstArgument.WithTriviaFrom(oldNode.ArgumentList.Arguments.First());
            else
            {
                firstArgument = firstArgument.WithoutTrivia();
            }
            var argumentAsArray = new[] { firstArgument };

            var allArguments = SyntaxFactory.SeparatedList(argumentAsArray.Concat(oldNode.ArgumentList.Arguments));
            var oldSeparators = allArguments.GetSeparators().ToArray();
            if (oldSeparators.Any() && oldNode.ArgumentList.Arguments.SeparatorCount > 0)
            {
                foreach (var separator in oldSeparators)
                {
                    allArguments = allArguments.ReplaceSeparator(separator, oldNode.ArgumentList.Arguments.GetSeparators().Last());
                }
            }

            var newArgumentsList = SyntaxFactory.ArgumentList(allArguments);
            newArgumentsList = newArgumentsList.WithOpenParenToken(oldNode.ArgumentList.OpenParenToken);

            return oldNode.WithArgumentList(newArgumentsList);
        }
    }
}