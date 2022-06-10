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
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Infrastructure.Styles.Analyzer
{
  public class RMSYN1102SeparateExitConditionsCodeFixProvider : CodeFixProvider
  {
    internal struct DoubleExpressions
    {
      public ExpressionSyntax left { get; private set; }
      public ExpressionSyntax right { get; private set; }

      public DoubleExpressions (ExpressionSyntax left, ExpressionSyntax right)
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

      DoubleExpressions orExpressions;
      IfStatementSyntax? ifStatement;
      var orPattern = syntaxRoot.FindNode(diagnosticSpan).FirstAncestorOrSelf<PatternSyntax>();
      var logicalOrExpression = syntaxRoot.FindNode(diagnosticSpan).FirstAncestorOrSelf<BinaryExpressionSyntax>();
      if (orPattern != null)
      {
        orExpressions = SplitOrPattern(orPattern);
        ifStatement = orPattern.Parent!.Parent as IfStatementSyntax;
      }
      else if (logicalOrExpression != null)
      {
        orExpressions = SplitLogicalOrExpression(logicalOrExpression);
        ifStatement = logicalOrExpression.Parent as IfStatementSyntax;
      }
      else
      {
        return context.Document;
      }

      if (ifStatement == null)
        return context.Document;
      var ifChildStatement = ifStatement.Statement;
      
      return context.Document;
    }

    private static DoubleExpressions SplitLogicalOrExpression (BinaryExpressionSyntax orNode)
    {
      return new DoubleExpressions(orNode.Left, orNode.Right);
    }

    private static DoubleExpressions SplitOrPattern (PatternSyntax orPattern)
    {
      throw new NotImplementedException();
    }
  }
}