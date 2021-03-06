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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using StyleAnalyzer = Infrastructure.Styles.Analyzer.RMSYN1007NoWhitespaceInCastAnalyzer;

namespace Infrastructure.Styles.Analyzer
{
  [ExportCodeFixProvider(LanguageNames.CSharp)]
  public class RMSYN1007NoWhitespaceInCastCodeFixProvider : CodeFixProvider
  {
    public override FixAllProvider GetFixAllProvider ()
    {
      return WellKnownFixAllProviders.BatchFixer;
    }

    public override Task RegisterCodeFixesAsync (CodeFixContext context)
    {
      context.RegisterCodeFix(
          CodeAction.Create(
              "Fix whitespaces",
              async cancellationToken =>
                  await SimpleWhitespaceCodeFixer.RemoveWhitespacesBetweenTwoTokens(context, cancellationToken),
              StyleAnalyzer.DiagnosticId),
          context.Diagnostics.First());
      return Task.CompletedTask;
    }

    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
      ImmutableArray.Create(StyleAnalyzer.DiagnosticId);
  }
}