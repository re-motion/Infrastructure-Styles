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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Infrastructure.Styles.Analyzer
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class RMSYN1102SeparateExitConditionsAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "RMSYN1102";

    internal const string Title = "Exit conditions must use separate IF statements instead of OR'ed statements";
    internal const string Message = "There must not be OR'ed exit conditions";
    internal const string Description = "Exit conditions must use separate IF statements instead of OR'ed statements";
    internal const string Category = "Readability";

    internal static readonly IReadOnlyCollection<SyntaxKind> InvalidStatements = new Collection<SyntaxKind>()
    {
      SyntaxKind.ReturnStatement
    };

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
      DiagnosticId,
      Title,
      Message,
      Category,
      DiagnosticSeverity.Warning,
      true,
      Description);

    public override void Initialize (AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

      context.EnableConcurrentExecution();
      context.RegisterSyntaxNodeAction(AnalyzeLogicalOrExpression, SyntaxKind.LogicalOrExpression);
      context.RegisterSyntaxNodeAction(AnalyzeOrPatternExpression, SyntaxKind.OrPattern);
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    private static void AnalyzeLogicalOrExpression (SyntaxNodeAnalysisContext context)
    {
      var orNode = context.Node;
      if (orNode.Parent == null)
        return;
      
      if (!IsInsideIfStatement(orNode, out var ifNode))
        return;

      AnalyzeIfStatement(context, ifNode!, orNode.GetLocation());
    }

    private static void AnalyzeIfStatement (SyntaxNodeAnalysisContext context, IfStatementSyntax ifNode,
      Location orPosition)
    {
      if (ifNode.Statement.IsKind(SyntaxKind.Block))
      {
        var blockNode = (BlockSyntax) ifNode.Statement;
        if (IsInvalidStatement(blockNode.Statements.First())) 
          CreateDiagnostic(orPosition, context);
      }
      else if (IsInvalidStatement(ifNode.Statement))
      {
        CreateDiagnostic(orPosition, context);
      }
    }

    private static void AnalyzeOrPatternExpression (SyntaxNodeAnalysisContext context)
    {
      var orNode = context.Node;
      if (orNode.Parent == null)
        return;
      var patternNode = orNode.Parent;
      
      if(patternNode.Parent == null)
        return;
      if (!IsInsideIfStatement(patternNode, out var ifNode)) return;
      AnalyzeIfStatement(context, ifNode!, orNode.GetLocation());
    }

    private static bool IsInvalidStatement (StatementSyntax statement)
    {
      return InvalidStatements.Any(statement.IsKind);
    }

    private static void CreateDiagnostic (Location location, SyntaxNodeAnalysisContext context)
    {
      var diagnostic = Diagnostic.Create(Rule, location);
      context.ReportDiagnostic(diagnostic);
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
  }
}