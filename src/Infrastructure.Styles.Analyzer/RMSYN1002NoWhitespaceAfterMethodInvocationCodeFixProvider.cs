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

using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Infrastructure.Styles.Analyzer
{
  [ExportCodeFixProvider(LanguageNames.CSharp)]
  public class RMSYN1002NoWhitespaceAfterMethodInvocationCodeFixProvider : CodeFixProvider
  {
    public override FixAllProvider? GetFixAllProvider()
    {
      return WellKnownFixAllProviders.BatchFixer;
    }

    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
      context.RegisterCodeFix(
        CodeAction.Create(
          "Fix whitespaces",
          c => CodeFix(c, context),
          RMSYN1002NoWhitespaceAfterMethodInvocationAnalyzer.DiagnosticId),
        context.Diagnostics.First());
      return Task.CompletedTask;
    }

    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
      ImmutableArray.Create(RMSYN1002NoWhitespaceAfterMethodInvocationAnalyzer.DiagnosticId);

    private static async Task<Document> CodeFix(CancellationToken cancellationToken, CodeFixContext context)
    {
      var diagnostic = context.Diagnostics.First();
      var diagnosticSpan = diagnostic.Location.SourceSpan;

      var syntaxRoot = await context.Document.GetSyntaxRootAsync(cancellationToken);
      if (syntaxRoot is null)
        return context.Document;

      var node = syntaxRoot.FindToken(diagnosticSpan.Start).Parent!.AncestorsAndSelf()
        .First(s => s.IsKind(SyntaxKind.InvocationExpression)
                    || s.IsKind(SyntaxKind.ObjectCreationExpression)
                    || s.IsKind(SyntaxKind.ImplicitObjectCreationExpression))!;

      SyntaxToken left;
      SyntaxToken right;

      if (node is InvocationExpressionSyntax invocationExpression)
      {
        left = invocationExpression.Expression.GetLastToken();
        right = invocationExpression.ArgumentList.OpenParenToken;
      }
      else if (node is ObjectCreationExpressionSyntax objectCreationExpression)
      {
        left = objectCreationExpression.Type.GetLastToken();
        if (objectCreationExpression.ArgumentList is null)
          return context.Document;

        right = objectCreationExpression.ArgumentList.OpenParenToken;
      }
      else if (node is ImplicitObjectCreationExpressionSyntax implicitObjectCreationExpression)
      {
        left = implicitObjectCreationExpression.NewKeyword;
        right = implicitObjectCreationExpression.ArgumentList.OpenParenToken;
      }
      else
      {
        return context.Document;
      }

      var newNode = node.ReplaceTokens(new[] {left, right}, (token, _) =>
      {
        return left == token
          ? left.WithTrailingTrivia(SyntaxTriviaList.Empty)
          : right.WithLeadingTrivia(SyntaxTriviaList.Empty);
      });

      var newSyntaxRoot = syntaxRoot.ReplaceNode(node, newNode);
      return context.Document.WithSyntaxRoot(newSyntaxRoot);
    }
  }
}