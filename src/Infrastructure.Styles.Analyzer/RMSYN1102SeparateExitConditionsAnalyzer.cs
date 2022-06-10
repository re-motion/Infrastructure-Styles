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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Infrastructure.Styles.Analyzer
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class RMSYN1102SeparateExitConditionsAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "RMSYN1102";

    internal const string Title = "Exit conditions must use separate IF statements instead of OR'ed statements";
    internal const string Message = "There must not be OR'ed exit conditions";

    internal const string Description =
        "Exit conditions must use separate IF statements instead of OR'ed statements";

    internal const string Category = "Readability";

    internal static readonly IReadOnlyCollection<SyntaxKind> InvalidStatements = new Collection<SyntaxKind>()
    {
        SyntaxKind.ReturnStatement
    };

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        id: DiagnosticId,
        title: Title,
        messageFormat: Message,
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override void Initialize (AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

      context.EnableConcurrentExecution();
      context.RegisterSyntaxNodeAction(AnalyzeLogicalExpression, SyntaxKind.LogicalOrExpression);
      context.RegisterSyntaxNodeAction(AnalyzeLogicalExpression, SyntaxKind.OrPattern);
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
      ImmutableArray.Create(Rule);

    private static void AnalyzeLogicalExpression (SyntaxNodeAnalysisContext context)
    {
      var baseNode = context.Node;
      SyntaxNode orNode;
      if (baseNode.IsKind(SyntaxKind.LogicalOrExpression))
        orNode = baseNode;
      else if (baseNode.IsKind(SyntaxKind.OrPattern))
        orNode = baseNode.Parent!;
      else
        return;

      if (orNode.Parent == null)
        return;

      var parentIf = GetParentIfStatementOrNull(orNode);
      if (parentIf is null)
        return;

      if (!IsLegalIfNode(parentIf))
        CreateDiagnostic(context, parentIf.GetLocation());
    }

    private static bool IsLegalIfNode (IfStatementSyntax? ifNode)
    {
      return IsLegalIfStatement(ifNode!);
    }

    private static void CreateDiagnostic (SyntaxNodeAnalysisContext context, Location location)
    {
      var diagnostic = Diagnostic.Create(Rule, location);
      context.ReportDiagnostic(diagnostic);
    }


    private static IfStatementSyntax? GetParentIfStatementOrNull (SyntaxNode node)
    {
      var ancestors = node.Ancestors();
      ancestors = ancestors.SkipWhile(
          n => n.IsKind(SyntaxKind.OrPattern) 
               || n.IsKind(SyntaxKind.IsPatternExpression) 
               || n.IsKind(SyntaxKind.ParenthesizedExpression));
      
      var ifParent = ancestors.First() as IfStatementSyntax;
      return ifParent;
    }

    private static bool IsLegalIfStatement (IfStatementSyntax ifNode)
    {
      if (ifNode.Statement.IsKind(SyntaxKind.Block))
      {
        var blockNode = (BlockSyntax)ifNode.Statement;
        return !IsInvalidStatement(blockNode.Statements.First()) || ContainsSoleLastElseStatement(ifNode);
      }
      else
      {
        return !IsInvalidStatement(ifNode.Statement) || ContainsSoleLastElseStatement(ifNode);
      }
    }

    private static bool ContainsSoleLastElseStatement (IfStatementSyntax ifNode)
    {
      var tempIfNode = ifNode;
      while (tempIfNode.Else != null)
      {
        tempIfNode = tempIfNode.Else.Statement as IfStatementSyntax;
        if (tempIfNode is null)
          return true;
      }

      return false;
    }

    private static bool IsInvalidStatement (StatementSyntax statement)
    {
      return InvalidStatements.Any(statement.IsKind);
    }
  }
}