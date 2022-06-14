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
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Infrastructure.Styles.Analyzer
{
  public class IfAndElseIfStatementSyntaxWrapper
  {
    public SyntaxNode IfOrElseIfStatement { get; private set; }

    public IfAndElseIfStatementSyntaxWrapper (SyntaxNode ifOrElseIfStatement)
    {
      if (ifOrElseIfStatement is not IfStatementSyntax and not ElseClauseSyntax)
        throw new ArgumentException("Syntaxnode must be of kind IfStatementSyntax or ElseClauseSyntax");
      IfOrElseIfStatement = ifOrElseIfStatement;
    }

    public IfAndElseIfStatementSyntaxWrapper Copy ()
    {
      return new IfAndElseIfStatementSyntaxWrapper(IfOrElseIfStatement);
    }
    
    public void WithCondition (SyntaxNode node)
    {
      var expression = (node as ExpressionSyntax)!;
      if (IsIfStatement())
      {
        var temp = IfOrElseIfStatement as IfStatementSyntax;
        IfOrElseIfStatement = temp!.WithCondition(expression);
      }
      else
      {
        var tempElseClause = IfOrElseIfStatement as ElseClauseSyntax;
        var tempIfStatement = tempElseClause!.Statement as IfStatementSyntax;
        tempIfStatement = tempIfStatement!.WithCondition(expression);
        IfOrElseIfStatement = tempElseClause.WithStatement(tempIfStatement);
      }
    }

    public void WithElseClause (ElseClauseSyntax elseClause)
    {
      if (IsIfStatement())
      {
        var temp = IfOrElseIfStatement as IfStatementSyntax;
        IfOrElseIfStatement = temp!.WithElse(elseClause);
      }
      else
      {
        var tempElseClause = IfOrElseIfStatement as ElseClauseSyntax;
        var tempIfStatement = tempElseClause!.Statement as IfStatementSyntax;
        tempIfStatement = tempIfStatement!.WithElse(elseClause);
        IfOrElseIfStatement = tempElseClause.WithStatement(tempIfStatement);
      }
    }

    public bool IsIfStatement ()
    {
      return IfOrElseIfStatement.IsKind(SyntaxKind.IfStatement);
    }

    public bool IsElseClause ()
    {
      return IfOrElseIfStatement.IsKind(SyntaxKind.ElseClause);
    }
  }
}