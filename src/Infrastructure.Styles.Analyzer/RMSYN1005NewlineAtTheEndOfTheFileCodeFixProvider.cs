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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StyleAnalyzer = Infrastructure.Styles.Analyzer.RMSYN1005NewlineAtTheEndOfTheFileAnalyzer;

namespace Infrastructure.Styles.Analyzer
{
  [ExportCodeFixProvider(LanguageNames.CSharp)]
  public class RMSYN1005NewlineAtTheEndOfTheFileCodeFixProvider : CodeFixProvider
  {
    public override FixAllProvider GetFixAllProvider ()
    {
      return WellKnownFixAllProviders.BatchFixer;
    }

    public override Task RegisterCodeFixesAsync (CodeFixContext context)
    {
      context.RegisterCodeFix(
          CodeAction.Create(
              "Ensure single newline at the end of the file",
              async token => await CodeFix(token, context),
              StyleAnalyzer.DiagnosticId),
          context.Diagnostics.First());
      return Task.CompletedTask;
    }

    private async Task<Document> CodeFix (CancellationToken cancellationToken, CodeFixContext context)
    {
      var diagnosticLocation = context.Diagnostics.First().Location;
      var syntaxRoot = await context.Document.GetSyntaxRootAsync(cancellationToken);

      if (syntaxRoot is null)
        return context.Document;

      var lastToken = syntaxRoot.GetLastToken();
      var endOfFileToken = syntaxRoot.GetLastToken(includeZeroWidth: true);
      Debug.Assert(endOfFileToken.IsKind(SyntaxKind.EndOfFileToken));

      var updatedRoot = syntaxRoot.ReplaceTokens(
          new[] { lastToken, endOfFileToken },
          (token, _) => token.IsKind(lastToken.Kind())
              ? ReplaceLastToken(in lastToken)
              : ReplaceEofToken(in endOfFileToken));

      return context.Document.WithSyntaxRoot(updatedRoot);
    }

    private static SyntaxToken ReplaceLastToken (in SyntaxToken lastToken)
    {
      if (lastToken.HasTrailingTrivia && lastToken.TrailingTrivia.Last().IsKind(SyntaxKind.EndOfLineTrivia))
        return lastToken;

      var newTrailingTrivia = lastToken.TrailingTrivia.Add(SyntaxFactory.LineFeed);
      return lastToken.WithTrailingTrivia(newTrailingTrivia);
    }

    private static SyntaxToken ReplaceEofToken (in SyntaxToken endOfFileToken)
    {
      if (!endOfFileToken.HasLeadingTrivia)
        return endOfFileToken;

      var leadingTrivia = endOfFileToken.LeadingTrivia;

      // Check if there are trailing newlines and how many
      var i = leadingTrivia.Count;
      while (i > 0 && leadingTrivia[i - 1].IsKind(SyntaxKind.EndOfLineTrivia))
        i--;

      var hasImplicitNewline = false;
      if (i > 0 && leadingTrivia[i - 1].HasStructure)
      {
        var structure = leadingTrivia[i - 1].GetStructure()!;
        var lastStructureToken = structure.GetLastToken(includeZeroWidth: true);
        hasImplicitNewline = lastStructureToken.HasTrailingTrivia && lastStructureToken.TrailingTrivia.Last().IsKind(SyntaxKind.EndOfLineTrivia);
      }

      // None found
      if (i == leadingTrivia.Count)
        return endOfFileToken;

      // If all are newlines we can remove them
      if (i == 0)
        return endOfFileToken.WithLeadingTrivia(SyntaxTriviaList.Empty);

      // but otherwise we have to remove one less as there is trivia in that line
      var endIndex = hasImplicitNewline ? i : i + 1;
      var newLeadingTriva = SyntaxFactory.TriviaList(leadingTrivia.Take(endIndex));
      return endOfFileToken.WithLeadingTrivia(newLeadingTriva);
    }

    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(StyleAnalyzer.DiagnosticId);
  }
}