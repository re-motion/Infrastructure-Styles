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

      context.RegisterSyntaxNodeAction(AnalyzeFile, ImmutableArray.Create(SyntaxKind.CompilationUnit));
    }

    private void AnalyzeFile (SyntaxNodeAnalysisContext context)
    {
      var compilationUnitSyntax = (CompilationUnitSyntax) context.Node;
      var lastFileToken = compilationUnitSyntax.GetLastToken();
      var EOFToken = compilationUnitSyntax.EndOfFileToken;

      if (EOFToken.HasLeadingTrivia
          || lastFileToken.TrailingTrivia.Count != 1
          || !lastFileToken.TrailingTrivia.First().IsKind(SyntaxKind.EndOfLineTrivia))
      {
        var start = lastFileToken.Span.End;
        var end = EOFToken.Span.Start;
        var location = Location.Create(context.Node.SyntaxTree, TextSpan.FromBounds(start, end));
        context.ReportDiagnostic(Diagnostic.Create(s_descriptor, location));
      }
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
      ImmutableArray.Create(s_descriptor);
  }
}