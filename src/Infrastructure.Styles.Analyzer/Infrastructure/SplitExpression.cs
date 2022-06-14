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
using System.Data;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Infrastructure.Styles.Analyzer.Infrastructure
{
  public class SplitExpression
  {
      public SyntaxNode Left { get; private set; }
      public SyntaxNode Right { get; private set; }

      public SplitExpression (SyntaxNode left, SyntaxNode right)
      {
        Left = left;
        Right = right;
      }

      public SplitExpression (SyntaxNode expression)
      {
        if (expression.IsKind(SyntaxKind.LogicalOrExpression))
        {
          var orNode = (expression as BinaryExpressionSyntax)!;
          Left = RemoveTrailingTrivia(orNode.Left);
          Right = RemoveLeadingTrivia(orNode.Right);
          return;
        }

        if (expression.IsKind(SyntaxKind.OrPattern))
        {
          var orPattern = (expression as BinaryPatternSyntax)!;
          var isParentPattern = GetParentIsPattern(expression);
          if (isParentPattern == null)
            throw new InvalidOperationException($"Could not split expression '{expression}'");
          Left = isParentPattern.WithPattern((PatternSyntax)RemoveTrailingTrivia(orPattern.Left));
          Right = isParentPattern.WithPattern((PatternSyntax)RemoveTrailingTrivia(orPattern.Right));
          return;
        }

        throw new InvalidOperationException($"Could not split expression '{expression}'");
      }

      private IsPatternExpressionSyntax GetParentIsPattern (SyntaxNode node)
      {
        while (true)
        {
          var parentPattern = node.Parent as IsPatternExpressionSyntax;
          if (parentPattern != null)
            return parentPattern;
          if (node.Parent.IsKind(SyntaxKind.IfStatement))
            throw new InvalidOperationException($"Could not find IsPatternParent of node '{node}'");
          node = node.Parent!;
        }
      } 
      
      private static SyntaxNode RemoveLeadingTrivia (SyntaxNode expression)
      {
        var tokenWithRemoved = expression.FindToken(expression.SpanStart).WithLeadingTrivia(SyntaxTriviaList.Empty);
        return expression.ReplaceToken(expression.FindToken(expression.SpanStart), tokenWithRemoved);
      }

      private static SyntaxNode RemoveTrailingTrivia (SyntaxNode expression)
      {
        if (expression.IsKind(SyntaxKind.ConstantPattern))
        {
          return expression.WithTrailingTrivia(SyntaxTriviaList.Empty);
        }
        var tokenWithRemoved = expression.FindToken(expression.Span.End - 1).WithTrailingTrivia(SyntaxTriviaList.Empty);
        return expression.ReplaceToken(expression.FindToken(expression.Span.End - 1), tokenWithRemoved);
      }
  }
}