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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

namespace Infrastructure.Styles.Analyzer
{
  [ExportCodeFixProvider(LanguageNames.CSharp)]
  public class RMSYN1004EmptyLinesShouldNotContainWhitespacesCodeFixProvider : CodeFixProvider
  {
    /// <inheritdoc />
    public override FixAllProvider? GetFixAllProvider()
    {
      return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc />
    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
      var codeAction = CodeAction.Create(
        "Remove whitespaces",
        c => CodeFix(c, context),
        string.Empty);
      context.RegisterCodeFix(codeAction, context.Diagnostics.First());

      return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
      ImmutableArray.Create(RMSYN1004EmptyLinesShouldNotContainWhitespacesAnalyzer.DiagnosticId);

    private static async Task<Document> CodeFix(CancellationToken cancellationToken, CodeFixContext context)
    {
      var diagnostic = context.Diagnostics.First();
      var diagnosticSpan = diagnostic.Location.SourceSpan;
      var syntaxRoot = await context.Document.GetSyntaxRootAsync(cancellationToken);
      if (syntaxRoot is null)
        return context.Document;

      var syntaxToken = syntaxRoot.FindToken(diagnosticSpan.Start);
      var newLeadingTrivia = RemoveWhitespacesFromEmptyLines(
        syntaxToken.LeadingTrivia,
        syntaxToken.IsKind(SyntaxKind.EndOfFileToken));
      var newTrailingTrivia = RemoveWhitespacesFromEmptyLines(
        syntaxToken.TrailingTrivia,
        syntaxToken.FullSpan.End == syntaxRoot.FullSpan.Length);
      var newSyntaxToken = syntaxToken
        .WithLeadingTrivia(newLeadingTrivia)
        .WithTrailingTrivia(newTrailingTrivia);

      var newSyntaxRoot = syntaxRoot.ReplaceToken(syntaxToken, newSyntaxToken);
      return context.Document.WithSyntaxRoot(newSyntaxRoot);
    }

    private static SyntaxTriviaList RemoveWhitespacesFromEmptyLines(
      SyntaxTriviaList previousSyntaxTrivia,
      bool isEofToken)
    {
      var triviaCount = previousSyntaxTrivia.Count;
      var newSyntaxTrivia = new List<SyntaxTrivia>(triviaCount);
      var position = 0;
      while (position < triviaCount)
      {
        // Check if we have 'WHITESPACE+ EOL' situation in which we do not add the whitespace
        var i = position;
        while (i < triviaCount && previousSyntaxTrivia[i].IsKind(SyntaxKind.WhitespaceTrivia))
          i++;

        if (i < triviaCount && previousSyntaxTrivia[i].IsKind(SyntaxKind.EndOfLineTrivia))
        {
          newSyntaxTrivia.Add(previousSyntaxTrivia[i]);
        }
        else
        {
          var end = (i < triviaCount, isEofToken) switch
          {
            (true, _) => i + 1,
            (false, false) => triviaCount,
            (false, true) => position, // special case for EOF token where we want to remove final whitespaces 
          };
          for (var j = position; j < end; j++)
            newSyntaxTrivia.Add(previousSyntaxTrivia[j]);
        }

        position = i + 1;
      }
      
      return SyntaxFactory.TriviaList(newSyntaxTrivia);
    }
  }
}