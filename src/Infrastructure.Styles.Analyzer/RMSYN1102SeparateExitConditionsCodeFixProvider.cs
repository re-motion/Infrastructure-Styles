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
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Styles.Analyzer.Infrastructure;
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

      return context.Document.WithSyntaxRoot(ReplaceIllegalStatement(syntaxRoot, diagnosticSpan));
    }

    private static SyntaxNode ReplaceIllegalStatement (SyntaxNode syntaxRoot, TextSpan diagnosticSpan)
    {
      var illegalExpression = syntaxRoot.FindNode(diagnosticSpan);

      illegalExpression = FindIllegalParentStatement(illegalExpression);
      
      if (!SimpleIfStatementAnalyzer.IsInsideIfStatement(illegalExpression, out var ifStatement))
        return syntaxRoot;

      if (ifStatement == null)
        return syntaxRoot;

      var syntaxWrapper = CreateSyntaxWrapper(ifStatement);

      var syntaxNodes = new List<IfAndElseIfStatementSyntaxWrapper>();
      
      FillSyntaxNodeListWithProperStatementsFromOrExpression(syntaxNodes, syntaxWrapper.Copy(),
        illegalExpression);

      return BuildNewRoot(syntaxRoot, syntaxWrapper, syntaxNodes);
    }

    private static IfAndElseIfStatementSyntaxWrapper CreateSyntaxWrapper (IfStatementSyntax ifStatement)
    {
      IfAndElseIfStatementSyntaxWrapper blueprint;
      if (ifStatement.Parent.IsKind(SyntaxKind.ElseClause))
        blueprint = new IfAndElseIfStatementSyntaxWrapper(ifStatement.Parent);
      else
        blueprint = new IfAndElseIfStatementSyntaxWrapper(ifStatement);

      return blueprint;
    }

    private static SyntaxNode BuildNewRoot (SyntaxNode oldRoot,
      IfAndElseIfStatementSyntaxWrapper statementSyntaxWrapper, List<IfAndElseIfStatementSyntaxWrapper> syntaxNodes)
    {
      if (statementSyntaxWrapper.IsIfStatement())
        return oldRoot.ReplaceNode(statementSyntaxWrapper.IfOrElseIfStatement,
          syntaxNodes.Select(s => s.IfOrElseIfStatement));
      
      if (statementSyntaxWrapper.IsElseClause())
        return BuildNewElseClauseRoot(oldRoot, syntaxNodes, statementSyntaxWrapper);

      throw new InvalidOperationException(
        $"StatementSyntaxWrapper did not hold an else clause or if statement, type of item held was '{statementSyntaxWrapper.IfOrElseIfStatement.GetType()}'");
    }

    private static SyntaxNode BuildNewElseClauseRoot (SyntaxNode oldRoot,
      List<IfAndElseIfStatementSyntaxWrapper> syntaxNodes, IfAndElseIfStatementSyntaxWrapper statementSyntaxWrapper)
    {
      var tempElseClause = syntaxNodes.Last();
      while (syntaxNodes.Count > 1)
      {
        syntaxNodes.RemoveAt(syntaxNodes.Count - 1);
        var tempParentElseClause = syntaxNodes.Last();

        tempParentElseClause.WithElseClause((ElseClauseSyntax) tempElseClause.IfOrElseIfStatement);
        tempElseClause = tempParentElseClause;
      }

      return oldRoot.ReplaceNode(statementSyntaxWrapper.IfOrElseIfStatement, tempElseClause.IfOrElseIfStatement);
    }


    private static void FillSyntaxNodeListWithProperStatementsFromOrExpression (
      List<IfAndElseIfStatementSyntaxWrapper> syntaxNodes, IfAndElseIfStatementSyntaxWrapper blueprint,
      SyntaxNode expression)
    {
      if (SimpleIfStatementAnalyzer.IsIllegalExpression(expression, out var illegalExpression))
      {
        var splitExpression = new SplitExpression(illegalExpression!);
        FillSyntaxNodeListWithProperStatementsFromOrExpression(syntaxNodes, blueprint, splitExpression.Left);
        FillSyntaxNodeListWithProperStatementsFromOrExpression(syntaxNodes, blueprint, splitExpression.Right);
        return;
      }

      blueprint.WithCondition(expression);
      syntaxNodes.Add(blueprint.Copy());
    }

    private static SyntaxNode FindIllegalParentStatement (SyntaxNode node)
    {
      if(node.Parent.IsKind(SyntaxKind.IfStatement))
      {
        return node;
      }
      if (node.Parent.IsKind(SyntaxKind.IsPatternExpression))
      {
        return node;
      }

      if (node.Parent == null)
        return node;
      
      return FindIllegalParentStatement(node.Parent);
    }
  }
}