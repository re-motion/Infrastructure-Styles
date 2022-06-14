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

      
      if (!SimpleIfStatementAnalyzer.IsInsideIfStatement(orNode, out var ifNode))
        return;

      if(!IsLegalIfNode(ifNode, context, orNode))
        CreateDiagnostic(context, orNode.GetLocation());
    }

    private static void AnalyzeOrPatternExpression (SyntaxNodeAnalysisContext context)
    {
      var orNode = context.Node;
      var patternNode = orNode.Parent;
      
      if(patternNode?.Parent == null)
        return;
      
      if (!SimpleIfStatementAnalyzer.IsInsideIfStatement(patternNode, out var ifNode)) 
        return;
      
      if(!IsLegalIfNode(ifNode, context, orNode))
        CreateDiagnostic(context, orNode.GetLocation());
    }

    private static bool IsLegalIfNode (IfStatementSyntax? ifNode, SyntaxNodeAnalysisContext context, SyntaxNode orNode)
    {
      var ifStatementAnalyzer = new SimpleIfStatementAnalyzer(InvalidStatements);
      return ifStatementAnalyzer.IsLegalIfStatement(ifNode!);
    }

    private static void CreateDiagnostic (SyntaxNodeAnalysisContext context, Location location)
    {
      var diagnostic = Diagnostic.Create(Rule, location);
      context.ReportDiagnostic(diagnostic);
    }
  }
}