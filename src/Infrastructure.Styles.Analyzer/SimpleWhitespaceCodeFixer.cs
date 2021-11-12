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

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

namespace Infrastructure.Styles.Analyzer
{
  public static class SimpleWhitespaceCodeFixer
  {
    /// <summary>
    /// Removes any whitespaces between a left and right token that are determined by the
    /// start and end of the reported diagnostic.
    /// </summary>
    public static async Task<Document> RemoveWhitespacesBetweenTwoTokens (
        CodeFixContext context,
        CancellationToken cancellationToken)
    {
      var diagnosticLocation = context.Diagnostics.First().Location;
      var syntaxRoot = await context.Document.GetSyntaxRootAsync(cancellationToken);

      if (syntaxRoot is null)
        return context.Document;

      var leftToken = syntaxRoot.FindToken(diagnosticLocation.SourceSpan.Start);
      var rightToken = syntaxRoot.FindToken(diagnosticLocation.SourceSpan.End);

      var updatedRoot = syntaxRoot.ReplaceTokens(
          new[] {leftToken, rightToken},
          (token, _) => token.IsKind(leftToken.Kind())
              ? token.WithTrailingTrivia(SyntaxTriviaList.Empty)
              : token.WithLeadingTrivia(SyntaxTriviaList.Empty));

      return context.Document.WithSyntaxRoot(updatedRoot);
    }
  }
}