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
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Infrastructure.Styles.Analyzer
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class RMSYN1004EmptyLinesShouldNotContainWhitespacesAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "RMSYN1004";

    private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
        DiagnosticId,
        "Lines should not contain trailing whitespaces",
        "Line contains trailing whitespaces",
        "Formatting",
        DiagnosticSeverity.Warning,
        true);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();

      context.RegisterSyntaxTreeAction(Analyze);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Descriptor);

    private static void Analyze(SyntaxTreeAnalysisContext analysisContext)
    {
      var possibleDiagnosticStart = 0;
      var previousTokenEnd = 0;
      var possibleDiagnosticCase = false;

      var root = analysisContext.Tree.GetRoot(analysisContext.CancellationToken);
      foreach (var trivia in root.DescendantTrivia())
      {
        var triviaKind = trivia.Kind();
        if (!possibleDiagnosticCase)
        {
          if (triviaKind != SyntaxKind.WhitespaceTrivia)
            continue;

          var span = trivia.Span;
          possibleDiagnosticStart = span.Start;
          previousTokenEnd = span.End;
          possibleDiagnosticCase = true;
          continue;
        }

        // Check if there is a token between this one and the last one since we are only checking trivia
        if (previousTokenEnd != trivia.SpanStart)
        {
          if (triviaKind == SyntaxKind.WhitespaceTrivia)
          {
            var span = trivia.Span;
            possibleDiagnosticStart = span.Start;
            previousTokenEnd = span.End;
          }
          else
          {
            possibleDiagnosticCase = false;
          }

          continue;
        }

        var triviaSpan = trivia.Span;
        if (triviaKind == SyntaxKind.EndOfLineTrivia)
        {
          ReportDiagnostic(ref analysisContext, possibleDiagnosticStart, triviaSpan.Start);
          possibleDiagnosticCase = false;
          continue;
        }

        if (triviaKind != SyntaxKind.WhitespaceTrivia)
        {
          possibleDiagnosticCase = false;
          continue;
        }

        previousTokenEnd = triviaSpan.End;
      }

      // Make sure we also catch problems in the last line
      if (possibleDiagnosticCase)
      {
        var treeLength = analysisContext.Tree.Length;
        if (treeLength == previousTokenEnd)
        {
          ReportDiagnostic(ref analysisContext, possibleDiagnosticStart, treeLength);
        }
      }
    }

    private static void ReportDiagnostic(ref SyntaxTreeAnalysisContext analysisContext, int start, int end)
    {
      var textSpan = TextSpan.FromBounds(start, end);
      if (textSpan.IsEmpty)
        return;

      var diagnostic = Diagnostic.Create(Descriptor, Location.Create(analysisContext.Tree, textSpan));
      analysisContext.ReportDiagnostic(diagnostic);
    }
  }
}