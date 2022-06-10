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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Infrastructure.Styles.Analyzer
{
  public class RMSYN1011BracesAfterUsingStatementCodeFixProvider : CodeFixProvider
  {
    public override FixAllProvider? GetFixAllProvider ()
    {
      return WellKnownFixAllProviders.BatchFixer;
    }

    public override Task RegisterCodeFixesAsync (CodeFixContext context)
    {
      context.RegisterCodeFix(
        CodeAction.Create(
          "Fix braces",
          c => CodeFix(c, context),
          RMSYN1101BracesAfterUsingStatementAnalyzer.DiagnosticId),
        context.Diagnostics.First());

      return Task.CompletedTask;
    }

    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
      ImmutableArray.Create(RMSYN1101BracesAfterUsingStatementAnalyzer.DiagnosticId);


    private static async Task<Document> CodeFix (CancellationToken cancellationToken, CodeFixContext context)
    {
      var diagnostic = context.Diagnostics.First();
      var diagnosticSpan = diagnostic.Location.SourceSpan;

      var syntaxRoot = await context.Document.GetSyntaxRootAsync(cancellationToken);
      if (syntaxRoot == null) return context.Document;

      var statement = syntaxRoot.FindNode(diagnosticSpan).FirstAncestorOrSelf<StatementSyntax>();
      if (statement == null) return context.Document;

      var newRoot = syntaxRoot.ReplaceNode(statement, SyntaxFactory.Block(statement));
      return context.Document.WithSyntaxRoot(newRoot);
    }
  }
}