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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Infrastructure.Styles.Analyzer
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class RMSYN1101BracesAfterUsingStatementAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "RMSYN1101";

    internal const string Title = "Using statements must use braces around the codeblock";
    internal const string Message = "There must be braces around the codeblock of a using statement";
    internal const string Description = "Using statements must use braces around the codeblock";
    internal const string Category = "Readability";

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

      context.RegisterSyntaxNodeAction(AnalyzeUsingStatement, SyntaxKind.UsingStatement);
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    private static void AnalyzeUsingStatement (SyntaxNodeAnalysisContext context)
    {
      var node = (UsingStatementSyntax) context.Node;

      if (node.Statement.Kind() != SyntaxKind.Block && node.Statement.Kind() != SyntaxKind.UsingStatement)
      {
        var span = TextSpan.FromBounds(node.Statement.SpanStart, node.Statement.Span.End);
        var location = Location.Create(node.SyntaxTree, span);
        var diagnostic = Diagnostic.Create(Rule, location);

        context.ReportDiagnostic(diagnostic);
      }
    }
  }
}