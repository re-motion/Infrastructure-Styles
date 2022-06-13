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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Infrastructure.Styles.Analyzer
{
  public class SimpleIfStatementAnalyzer
  {
    private readonly IReadOnlyCollection<SyntaxKind> _invalidStatements;

    public SimpleIfStatementAnalyzer (IReadOnlyCollection<SyntaxKind> invalidStatements)
    {
      _invalidStatements = invalidStatements;
    }
    
    public static bool IsInsideIfStatement (SyntaxNode node, out IfStatementSyntax? ifParent)
    {
      if (node.Parent == null)
      {
        ifParent = null;
        return false;
      }

      var parent = node.Parent;
      ifParent = parent as IfStatementSyntax;
      if (ifParent != null)
        return true;

      if (parent.IsKind(SyntaxKind.ParenthesizedExpression))
      {
        ifParent = parent.Parent as IfStatementSyntax;
        if (ifParent != null)
          return true;
      }
      return false;
    }

    public static bool IsIllegalExpression (SyntaxNode expression, out ExpressionSyntax? illegalExpression)
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

    public bool IsLegalIfStatement (IfStatementSyntax ifNode)
    {
      if (ifNode.Statement.IsKind(SyntaxKind.Block))
      {
        var blockNode = (BlockSyntax) ifNode.Statement;
        if (IsInvalidStatement(blockNode.Statements.First()))
          return false;
      }
      return !IsInvalidStatement(ifNode.Statement);
    }

    private bool IsInvalidStatement (StatementSyntax statement)
    {
      return _invalidStatements.Any(statement.IsKind);
    }
  }
}