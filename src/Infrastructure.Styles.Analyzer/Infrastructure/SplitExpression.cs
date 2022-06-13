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

namespace Infrastructure.Styles.Analyzer.Infrastructure
{
  public class SplitExpression
  {
      public ExpressionSyntax Left { get; private set; }
      public ExpressionSyntax Right { get; private set; }

      public SplitExpression (ExpressionSyntax left, ExpressionSyntax right)
      {
        Left = left;
        Right = right;
      }

      public  SplitExpression (ExpressionSyntax expression)
      {
        if (expression.IsKind(SyntaxKind.LogicalOrExpression))
        {
          var orNode = (expression as BinaryExpressionSyntax)!;
          Left = orNode.Left;
          Right = orNode.Right;
          return;
        }

        if (expression.IsKind(SyntaxKind.OrPattern))
        {
          throw new NotImplementedException();
        }

        throw new InvalidOperationException($"Could not split expression '{expression}'");
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

      public  void RemoveRedundantTrivia ()
      {
        
        Left = RemoveTrailingTrivia(Left);
        Right = RemoveLeadingTrivia(Right);
      }
  }
}