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
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Infrastructure.Styles.Analyzer
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class RMSYN1005NewlineAtTheEndOfTheFileAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "RMSYN1005";

    internal const string Title = "The file must end with a single newline";
    internal const string Message = "The file must end with a single newline.";
    internal const string Category = "Formatting";

    private static readonly DiagnosticDescriptor s_descriptor = new(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Warning, true);

    public override void Initialize (AnalysisContext context)
    {
      context.EnableConcurrentExecution();
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

      context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
    }

    private void AnalyzeSyntaxTree (SyntaxTreeAnalysisContext context)
    {
      var syntaxTree = context.Tree;
      var sourceText = syntaxTree.GetText();
      var lines = sourceText.Lines;
      
      if (lines.Count == 0)
        return;

      var lastLine = lines[lines.Count - 1];
      var isEmpty = lastLine.Start == lastLine.End;
      if (!isEmpty && lastLine.End == lastLine.EndIncludingLineBreak)
      {
        var diagnostic = Diagnostic.Create(s_descriptor, Location.Create(syntaxTree, lastLine.Span));
        context.ReportDiagnostic(diagnostic);
      }
      
      // Ensure that there are no empty lines preceding the last lines -> there should only be one newline at the end
      var start = lines.Count - 2;
      var i = start;
      while (i >= 0 && lines[i].Text != null && string.IsNullOrWhiteSpace(sourceText.ToString(lines[i].Span)))
        i--;

      if (i != start)
      {
        var textSpan = TextSpan.FromBounds(lines[i + 1].Start, lastLine.End);
        var location = Location.Create(syntaxTree, textSpan);
        var diagnostic = Diagnostic.Create(s_descriptor, location);
        context.ReportDiagnostic(diagnostic);
      }
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
      ImmutableArray.Create(s_descriptor);
  }
}