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
  public class IfAndElseIfStatementSyntaxWrapper
  {
    public SyntaxNode IfOrElseIfStatement { get; private set; }
    public ElseClauseSyntax? ElseClause { get; }
    public bool HasElseClause { get; }

    public IfAndElseIfStatementSyntaxWrapper (SyntaxNode ifOrElseIfStatement, SyntaxNode? elseClause)
    {
      if (ifOrElseIfStatement is not IfStatementSyntax and not ElseClauseSyntax)
        throw new ArgumentException("SyntaxNode must be of kind IfStatementSyntax or ElseClauseSyntax");
      IfOrElseIfStatement = ifOrElseIfStatement;
      if (elseClause != null && elseClause.IsKind(SyntaxKind.ElseClause))
      {
        ElseClause = (ElseClauseSyntax) elseClause;
        HasElseClause = true;
      }
      else
      {
        HasElseClause = false;
      }
    }

    public void RemoveElseClause ()
    {
      if (IsElseClause())
      {
        var tempElse = IfOrElseIfStatement as ElseClauseSyntax;
        var tempIf = tempElse!.Statement as IfStatementSyntax;
        tempIf = tempIf!.WithElse(null);
        tempElse = tempElse.WithStatement(tempIf);
        IfOrElseIfStatement = tempElse;
      }
      else if (IsIfStatement())
      {
        var tempIf = IfOrElseIfStatement as IfStatementSyntax;
        IfOrElseIfStatement = tempIf!.WithElse(null);
      }
    }

    public IfAndElseIfStatementSyntaxWrapper Copy ()
    {
      return new IfAndElseIfStatementSyntaxWrapper(IfOrElseIfStatement, ElseClause);
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

    public void WithElse (ElseClauseSyntax elseClause)
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

    private bool IsIfStatement ()
    {
      return IfOrElseIfStatement.IsKind(SyntaxKind.IfStatement);
    }

    private bool IsElseClause ()
    {
      return IfOrElseIfStatement.IsKind(SyntaxKind.ElseClause);
    }

    public bool ElseClauseHasIfStatement ()
    {
      return ElseClause != null && ElseClause.Statement.IsKind(SyntaxKind.IfStatement);
    }

    public void ReintroduceParentElseClause ()
    {
      if (IfOrElseIfStatement.IsKind(SyntaxKind.ElseClause))
        return;

      if (!HasElseClause)
        return;

      var elseClause = ElseClause;
      if (IfOrElseIfStatement.IsKind(SyntaxKind.IfStatement))
      {
        elseClause = elseClause!.WithElseKeyword(elseClause.ElseKeyword.WithTrailingTrivia(SyntaxTriviaList.Create(SyntaxFactory.Space)));
        IfOrElseIfStatement = IfOrElseIfStatement.WithLeadingTrivia(SyntaxTriviaList.Empty);
      }
      elseClause = elseClause!.WithStatement((StatementSyntax) IfOrElseIfStatement);
      IfOrElseIfStatement = elseClause;
    }
  }
}