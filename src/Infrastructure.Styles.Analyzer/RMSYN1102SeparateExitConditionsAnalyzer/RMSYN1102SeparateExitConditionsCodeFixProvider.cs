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
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Infrastructure.Styles.Analyzer.RMSYN1102SeparateExitConditionsAnalyzer
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
      IllegalExpression illegalExpression;
      try
      {
        illegalExpression = new IllegalExpression(syntaxRoot.FindNode(diagnosticSpan));
      }
      catch
      {
        return syntaxRoot;
      }

      var syntaxWrapper = CreateSyntaxWrapper(illegalExpression.IfStatement);
      
      //need to remove else otherwise there are else clauses after every if and that's not desirable
      var syntaxWrapperWithoutElse = syntaxWrapper.Copy();
      syntaxWrapperWithoutElse.RemoveElseClause();

      var syntaxNodes = new List<IfAndElseIfStatementSyntaxWrapper>();

      FillSyntaxNodeListWithStatements(syntaxNodes, syntaxWrapperWithoutElse,
        illegalExpression.Node);

      //time to handle the previously ignored else clauses
      if (syntaxWrapper.HasElseClause)
        HandleElseClauses(syntaxWrapper, syntaxNodes);

      return BuildRoot(syntaxRoot, syntaxNodes, syntaxWrapper);
    }

    private static void HandleElseClauses (IfAndElseIfStatementSyntaxWrapper syntaxWrapper,
      List<IfAndElseIfStatementSyntaxWrapper> syntaxNodes)
    {
      if (syntaxWrapper.ElseClauseHasIfStatement())
      {
        var heldIfStatement = syntaxWrapper.ElseClause!.Statement as IfStatementSyntax;
        var elseClauseSyntaxWrapper = CreateSyntaxWrapper(heldIfStatement!);
        FillSyntaxNodeListWithStatements(syntaxNodes, elseClauseSyntaxWrapper, heldIfStatement!.Condition);
      }
      else
      {
        syntaxNodes.Add(new IfAndElseIfStatementSyntaxWrapper(syntaxWrapper.ElseClause!, null));
      }

      ReintroduceParentElseClauses(syntaxNodes);
    }

    private static IfAndElseIfStatementSyntaxWrapper CreateSyntaxWrapper (IfStatementSyntax ifStatement)
    {
      IfAndElseIfStatementSyntaxWrapper blueprint;
      if (ifStatement.Parent.IsKind(SyntaxKind.ElseClause))
        blueprint = new IfAndElseIfStatementSyntaxWrapper(ifStatement.Parent, ifStatement.Else);
      else
        blueprint = new IfAndElseIfStatementSyntaxWrapper(ifStatement, ifStatement.Else);

      return blueprint;
    }

    private static SyntaxNode BuildRoot (SyntaxNode oldRoot,
      List<IfAndElseIfStatementSyntaxWrapper> syntaxNodes, IfAndElseIfStatementSyntaxWrapper oldNode)
    {
      BuildProperStatementList(syntaxNodes);

      //ReplaceNode() can only replace an ifNode by a list if the list does not include else statements.
      //Else if statements can only be in the list if it's count is 1
      if (syntaxNodes.Count == 1)
        return oldRoot.ReplaceNode(oldNode.IfOrElseIfStatement, syntaxNodes.First().IfOrElseIfStatement);
      return oldRoot.ReplaceNode(oldNode.IfOrElseIfStatement,
        syntaxNodes.Select(s => s.IfOrElseIfStatement));
    }

    private static void BuildProperStatementList (List<IfAndElseIfStatementSyntaxWrapper> syntaxNodes)
    {
      var tempElseClause = syntaxNodes.Last();
      syntaxNodes.RemoveAt(syntaxNodes.Count - 1);

      while (syntaxNodes.Count > 0)
      {
        if (tempElseClause.IfOrElseIfStatement.IsKind(SyntaxKind.IfStatement)) break;

        var tempParentElseClause = syntaxNodes.Last();

        tempParentElseClause.WithElse((ElseClauseSyntax) tempElseClause.IfOrElseIfStatement);
        tempElseClause = tempParentElseClause;
        syntaxNodes.RemoveAt(syntaxNodes.Count - 1);
      }

      syntaxNodes.Add(tempElseClause);
    }


    private static void FillSyntaxNodeListWithStatements (
      List<IfAndElseIfStatementSyntaxWrapper> syntaxNodes, IfAndElseIfStatementSyntaxWrapper blueprint,
      SyntaxNode expression)
    {
      if (SimpleIfStatementAnalyzer.IsIllegalExpression(expression, out var illegalExpression))
      {
        var splitExpression = new SplitExpression(illegalExpression!);
        FillSyntaxNodeListWithStatements(syntaxNodes, blueprint, splitExpression.Left);
        FillSyntaxNodeListWithStatements(syntaxNodes, blueprint, splitExpression.Right);
        return;
      }

      blueprint.WithCondition(expression);
      syntaxNodes.Add(blueprint.Copy());
    }

    private static void ReintroduceParentElseClauses (List<IfAndElseIfStatementSyntaxWrapper> syntaxNodes)
    {
      foreach (var (syntaxNode, index) in syntaxNodes.Select((syntaxNode, i) => (syntaxNode, i)))
      {
        if (index == 0)
          continue;
        syntaxNode.ReintroduceParentElseClause();
      }
    }
  }
}