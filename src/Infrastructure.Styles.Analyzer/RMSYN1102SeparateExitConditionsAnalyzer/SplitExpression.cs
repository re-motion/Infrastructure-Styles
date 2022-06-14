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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Infrastructure.Styles.Analyzer.RMSYN1102SeparateExitConditionsAnalyzer
{
  public class SplitExpression
  {
    public SyntaxNode Left { get; }
    public SyntaxNode Right { get; }

    public SplitExpression (SyntaxNode node)
    {
      if (node.IsKind(SyntaxKind.LogicalOrExpression))
      {
        var orNode = (node as BinaryExpressionSyntax)!;
        Left = RemoveTrailingTrivia(orNode.Left);
        Right = RemoveLeadingTrivia(orNode.Right);
        return;
      }

      if (node.IsKind(SyntaxKind.OrPattern))
      {
        var orPattern = (node as BinaryPatternSyntax)!;
        var isParentPattern = GetIsPatternExpressionParent(node);
        if (isParentPattern == null)
          throw new InvalidOperationException($"Could not split expression '{node}'");
        Left = isParentPattern.WithPattern((PatternSyntax) RemoveTrailingTrivia(orPattern.Left));
        Right = isParentPattern.WithPattern((PatternSyntax) RemoveTrailingTrivia(orPattern.Right));
        return;
      }

      throw new InvalidOperationException($"Could not split expression '{node}'");
    }

    private static IsPatternExpressionSyntax GetIsPatternExpressionParent (SyntaxNode node)
    {
      while (true)
      {
        if (node.Parent is IsPatternExpressionSyntax parentPattern)
          return parentPattern;
        if (node.Parent.IsKind(SyntaxKind.IfStatement))
          throw new InvalidOperationException($"Could not find IsPatternParent of node '{node}'");
        node = node.Parent!;
      }
    }

    private static SyntaxNode RemoveLeadingTrivia (SyntaxNode node)
    {
      if (node.IsKind(SyntaxKind.ConstantPattern)) return node.WithLeadingTrivia(SyntaxTriviaList.Empty);
      var tokenWithRemoved = node.FindToken(node.SpanStart).WithLeadingTrivia(SyntaxTriviaList.Empty);
      return node.ReplaceToken(node.FindToken(node.SpanStart), tokenWithRemoved);
    }

    private static SyntaxNode RemoveTrailingTrivia (SyntaxNode node)
    {
      if (node.IsKind(SyntaxKind.ConstantPattern)) return node.WithTrailingTrivia(SyntaxTriviaList.Empty);
      var tokenWithRemoved = node.FindToken(node.Span.End - 1).WithTrailingTrivia(SyntaxTriviaList.Empty);
      return node.ReplaceToken(node.FindToken(node.Span.End - 1), tokenWithRemoved);
    }
  }
}