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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Infrastructure.Styles.Analyzer
{
  public class RMSYN1102SeparateExitConditionsCodeFixProvider : CodeFixProvider
  {
    internal struct SplitExpressions
    {
      public ExpressionSyntax left { get; private set; }
      public ExpressionSyntax right { get; private set; }

      public SplitExpressions (ExpressionSyntax left, ExpressionSyntax right)
      {
        this.left = left;
        this.right = right;
      }
    }

    public override FixAllProvider? GetFixAllProvider ()
    {
      return WellKnownFixAllProviders.BatchFixer;
    }

    public override Task RegisterCodeFixesAsync (CodeFixContext context)
    {
      context.RegisterCodeFix(
        CodeAction.Create(
          "Split IF statement",
          c => CodeFix(c, context),
          RMSYN1102SeparateExitConditionsAnalyzer.DiagnosticId),
        context.Diagnostics.First());
      return Task.CompletedTask;
    }

    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
      ImmutableArray.Create(RMSYN1102SeparateExitConditionsAnalyzer.DiagnosticId);

    private static async Task<Document> CodeFix (CancellationToken cancellationToken, CodeFixContext context)
    {
      var diagnostic = context.Diagnostics.First();
      var diagnosticSpan = diagnostic.Location.SourceSpan;

      var syntaxRoot = await context.Document.GetSyntaxRootAsync(cancellationToken);
      if (syntaxRoot == null) return context.Document;

      return context.Document.WithSyntaxRoot(ReplaceIfStatement(syntaxRoot, diagnosticSpan));
    }

    private static SyntaxNode ReplaceIfStatement (SyntaxNode syntaxRoot, TextSpan diagnosticSpan)
    {
      var illegalExpression = syntaxRoot.FindNode(diagnosticSpan);
      

      RMSYN1102SeparateExitConditionsAnalyzer.IsImmediateParentOrParentsParentIfStatement(illegalExpression,
        out var ifStatement);

      if (ifStatement == null)
        return syntaxRoot;

      var syntaxNodes = new List<SyntaxNode>();
      FillSyntaxNodeListWithProperIfStatementsFromOrExpression(syntaxNodes, ifStatement, (ExpressionSyntax) illegalExpression);

      var newRoot = syntaxRoot.ReplaceNode(ifStatement, syntaxNodes);

      return newRoot;
    }

    private static SplitExpressions SplitOrSyntax (ExpressionSyntax expression)
    {
      if (expression.IsKind(SyntaxKind.LogicalOrExpression))
      {
        var orNode = (expression as BinaryExpressionSyntax)!;
        return new SplitExpressions(orNode.Left, orNode.Right);
      }

      if (expression.IsKind(SyntaxKind.OrPattern))
      {
        throw new NotImplementedException();
      }

      throw new InvalidOperationException($"Could not split expression '{expression}'");
    }


    private static bool IsIllegalExpression (SyntaxNode expression, out ExpressionSyntax? illegalExpression)
    {
      if (expression.IsKind(SyntaxKind.ParenthesizedExpression))
      {
        var parenthesizedExpression = expression as ParenthesizedExpressionSyntax;
        return IsIllegalExpression(parenthesizedExpression!.Expression, out illegalExpression);
      }
      if (expression.IsKind(SyntaxKind.LogicalOrExpression) || expression.IsKind(SyntaxKind.OrPattern))
      {
        illegalExpression = expression as ExpressionSyntax;
        return true;
      }

      illegalExpression = null;
      return false;
    }

    private static void FillSyntaxNodeListWithProperIfStatementsFromOrExpression (List<SyntaxNode> syntaxNodes, IfStatementSyntax ifBlueprint, ExpressionSyntax expression)
    {
      while (true)
      {
        if (IsIllegalExpression(expression, out var illegalExpression))
        {
          var splitExpression = SplitOrSyntax(illegalExpression!);
          splitExpression = RemoveRedundantTrivia(splitExpression);
          FillSyntaxNodeListWithProperIfStatementsFromOrExpression(syntaxNodes, ifBlueprint, splitExpression.left);
          expression = splitExpression.right;
          continue;
        }

        var nextIfStatement = ifBlueprint.WithCondition(expression);
        syntaxNodes.Add(nextIfStatement);
        break;
      }
    }

    private static SplitExpressions RemoveRedundantTrivia (SplitExpressions splitExpression)
    {
      return  new SplitExpressions(RemoveTrailingTrivia(splitExpression.left),
        RemoveLeadingTrivia(splitExpression.right));
    }

    private static ExpressionSyntax RemoveLeadingTrivia (ExpressionSyntax expression)
    {
      var tokenWithRemoved = expression.FindToken(expression.SpanStart).WithLeadingTrivia(SyntaxTriviaList.Empty);
      return expression.ReplaceToken(expression.FindToken(expression.SpanStart), tokenWithRemoved);
    }

    private static ExpressionSyntax RemoveTrailingTrivia (ExpressionSyntax expression)
    {
      var tokenWithRemoved = expression.FindToken(expression.Span.End).WithTrailingTrivia(SyntaxTriviaList.Empty);
      return expression.ReplaceToken(expression.FindToken(expression.Span.End), tokenWithRemoved);
    }
  }
  
}