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
  public class IllegalExpression
  {
    public SyntaxNode Node { get; }

    public IfStatementSyntax IfStatement { get; }

    public IllegalExpression (SyntaxNode illegalExpression)
    {
      if (illegalExpression.IsKind(SyntaxKind.OrPattern))
        illegalExpression = FindTopMostParentExpression(illegalExpression);
      Node = illegalExpression;
      SimpleIfStatementAnalyzer.IsInsideIfStatement(Node, out var ifStatement);
      IfStatement = ifStatement ?? throw new ArgumentException(
        "Could not create IllegalExpression due to given node not being inside IfStatement");
    }

    private SyntaxNode FindTopMostParentExpression (SyntaxNode node)
    {
      if (node.Parent.IsKind(SyntaxKind.IfStatement)) return node;
      if (node.Parent.IsKind(SyntaxKind.IsPatternExpression)) return node;

      if (node.Parent == null)
        return node;

      return FindTopMostParentExpression(node.Parent);
    }
  }
}