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
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Infrastructure.Styles.Analyzer
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class RMSYN1003WhitespaceBeforeKeywordExpressionAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "RMSYN1003";

    internal const string Title = "Invalid formatting of a keyword expression";
    internal const string MessageFormat = "'{0}' must be immediately followed by '('.";
    internal const string Category = "Formatting";

    private static readonly DiagnosticDescriptor s_descriptor = new DiagnosticDescriptor(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        true);

    public override void Initialize (AnalysisContext context)
    {
      context.EnableConcurrentExecution();
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

      // typeof(...)
      context.RegisterSyntaxNodeAction(
          static analysisContext => AnalyzeKeywordExpression(
              analysisContext,
              static n => ((TypeOfExpressionSyntax) n).Keyword,
              static n => ((TypeOfExpressionSyntax) n).OpenParenToken),
          ImmutableArray.Create(SyntaxKind.TypeOfExpression));
      // sizeof(...)
      context.RegisterSyntaxNodeAction(
          static analysisContext => AnalyzeKeywordExpression(
              analysisContext,
              static n => ((SizeOfExpressionSyntax) n).Keyword,
              static n => ((SizeOfExpressionSyntax) n).OpenParenToken),
          ImmutableArray.Create(SyntaxKind.SizeOfExpression));
      // default(...)
      context.RegisterSyntaxNodeAction(
          static analysisContext => AnalyzeKeywordExpression(
              analysisContext,
              static n => ((DefaultExpressionSyntax) n).Keyword,
              static n => ((DefaultExpressionSyntax) n).OpenParenToken),
          ImmutableArray.Create(SyntaxKind.DefaultExpression));
      // checked(...), unchecked(...)
      context.RegisterSyntaxNodeAction(
          static analysisContext => AnalyzeKeywordExpression(
              analysisContext,
              static n => ((CheckedExpressionSyntax) n).Keyword,
              static n => ((CheckedExpressionSyntax) n).OpenParenToken),
          ImmutableArray.Create(SyntaxKind.CheckedExpression, SyntaxKind.UncheckedExpression));
      // : base(...), : this(...)
      context.RegisterSyntaxNodeAction(
          static analysisContext => AnalyzeKeywordExpression(
              analysisContext,
              static n => ((ConstructorInitializerSyntax) n).ThisOrBaseKeyword,
              static n => ((ConstructorInitializerSyntax) n).ArgumentList.OpenParenToken),
          ImmutableArray.Create(SyntaxKind.ThisConstructorInitializer, SyntaxKind.BaseConstructorInitializer));
      // new(...)
      context.RegisterSyntaxNodeAction(
          static analysisContext => AnalyzeKeywordExpression(
              analysisContext,
              static n => ((ImplicitObjectCreationExpressionSyntax) n).NewKeyword,
              static n => ((ImplicitObjectCreationExpressionSyntax) n).ArgumentList.OpenParenToken),
          ImmutableArray.Create(SyntaxKind.ImplicitObjectCreationExpression));
      // nameof(...)
      context.RegisterSyntaxNodeAction(
          static analysisContext =>
          {
              var invocationExpression = (InvocationExpressionSyntax) analysisContext.Node;
              if (invocationExpression.Expression is IdentifierNameSyntax identifierName
                  && identifierName.Identifier.IsKind(SyntaxKind.IdentifierToken)
                  && identifierName.Identifier.Text == "nameof")
              {
                  // We deliberately ignore the special case of a "nameof" methods existing as the effects are minimal and the chance of it occuring are near zero
                  AnalyzeKeywordExpression(
                          analysisContext,
                          static n => ((InvocationExpressionSyntax) n).Expression.GetLastToken(),
                          static n => ((InvocationExpressionSyntax) n).ArgumentList.OpenParenToken);
              }
          },
          ImmutableArray.Create(SyntaxKind.InvocationExpression));
    }

    private static void AnalyzeKeywordExpression (
        SyntaxNodeAnalysisContext ctx,
        Func<SyntaxNode, SyntaxToken> leftTokenSelector,
        Func<SyntaxNode, SyntaxToken> rightTokenSelector)
    {
      var leftToken = leftTokenSelector(ctx.Node);
      if (leftToken.TrailingTrivia.Count > 0)
      {
        var rightToken = rightTokenSelector(ctx.Node);

        if (rightToken.IsMissing)
          return;

        var start = leftToken.Span.End;
        var end = rightToken.SpanStart;
        var location = Location.Create(ctx.Node.SyntaxTree, TextSpan.FromBounds(start, end));
        ctx.ReportDiagnostic(Diagnostic.Create(s_descriptor, location, leftToken.Text));
      }
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
      ImmutableArray.Create(s_descriptor);
  }
}