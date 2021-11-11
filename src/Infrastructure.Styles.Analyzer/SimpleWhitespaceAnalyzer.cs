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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Infrastructure.Styles.Analyzer
{
  public static class SimpleWhitespaceAnalyzer
  {
    /// <summary>
    /// Ensures that there is no whitespace between <paramref name="left"/> and <paramref name="right"/>,
    /// reporting a diagnostic using <paramref name="descriptor"/> if this is not the case.
    /// </summary>
    public static void AnalyzeNoWhitespaceBetweenTokens (
        ref SyntaxNodeAnalysisContext context,
        DiagnosticDescriptor descriptor,
        in SyntaxToken left,
        in SyntaxToken right)
    {
      if (left.TrailingTrivia.Count == 0 && right.LeadingTrivia.Count == 0)
        return;

      var newSpan = TextSpan.FromBounds(left.Span.End, right.SpanStart);
      var newLocation = Location.Create(right.Parent!.SyntaxTree, newSpan);
      var diagnostic = Diagnostic.Create(descriptor, newLocation);

      context.ReportDiagnostic(diagnostic);
    }

    /// <summary>
    /// Ensures that there is exactly ONE whitespace between <paramref name="left"/> and <paramref name="right"/>,
    /// reporting a diagnostic using <paramref name="descriptor"/> if this is not the case.
    /// </summary>
    public static void AnalyzeExactlyOneWhitespaceBetweenTokens (
        ref SyntaxNodeAnalysisContext context,
        DiagnosticDescriptor descriptor,
        in SyntaxToken left,
        in SyntaxToken right)
    {
      var trivia = left.TrailingTrivia;
      if (trivia.Count == 0)
      {
        var diagnostic = Diagnostic.Create(descriptor, right.GetLocation());

        context.ReportDiagnostic(diagnostic);
      }
      else if (trivia.Count != 1 || !SyntaxFactory.Space.IsEquivalentTo(trivia.First()))
      {
        var newSpan = TextSpan.FromBounds(left.Span.End, right.SpanStart);
        var newLocation = Location.Create(right.Parent!.SyntaxTree, newSpan);
        var diagnostic = Diagnostic.Create(descriptor, newLocation);

        context.ReportDiagnostic(diagnostic);
      }
    }
  }
}