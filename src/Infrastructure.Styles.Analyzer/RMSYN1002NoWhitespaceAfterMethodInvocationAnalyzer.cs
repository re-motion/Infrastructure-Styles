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
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Infrastructure.Styles.Analyzer
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class RMSYN1002NoWhitespaceAfterMethodInvocationAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "RMSYN1002";

    internal const string Title = "Method invocation must not have a whitespace before its arguments";
    internal const string Message = "There must not be any whitespaces in a method invocation before its arguments";
    internal const string Description = "Method invocation must not have a whitespace before its arguments";
    internal const string Category = "Formatting";

    private static readonly DiagnosticDescriptor Rule = new(
      DiagnosticId,
      Title,
      Message,
      Category,
      DiagnosticSeverity.Warning,
      true,
      Description);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

      context.EnableConcurrentExecution();

      context.RegisterSyntaxNodeAction(AnalyzeInvocationExpression, SyntaxKind.InvocationExpression);
      context.RegisterSyntaxNodeAction(AnalyzeObjectCreationExpression, SyntaxKind.ObjectCreationExpression);
      context.RegisterSyntaxNodeAction(AnalyzeImplicitObjectCreationExpression,
        SyntaxKind.ImplicitObjectCreationExpression);
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    private static void AnalyzeImplicitObjectCreationExpression(SyntaxNodeAnalysisContext context)
    {
      var node = (ImplicitObjectCreationExpressionSyntax) context.Node;
      var left = node.NewKeyword;
      var right = node.ArgumentList.OpenParenToken;

      AnalyzeWhitespace(context, left, right);
    }

    private static void AnalyzeObjectCreationExpression(SyntaxNodeAnalysisContext context)
    {
      var node = (ObjectCreationExpressionSyntax) context.Node;
      var left = node.Type.GetLastToken();
      if (node.ArgumentList is null)
        return;

      var right = node.ArgumentList.OpenParenToken;

      AnalyzeWhitespace(context, left, right);
    }

    private static void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
    {
      var node = (InvocationExpressionSyntax) context.Node;
      var left = node.Expression.GetLastToken();
      var right = node.ArgumentList.OpenParenToken;

      AnalyzeWhitespace(context, left, right);
    }

    private static void AnalyzeWhitespace(SyntaxNodeAnalysisContext context, SyntaxToken left, SyntaxToken right)
    {
      if (left.TrailingTrivia.Count == 0 && right.LeadingTrivia.Count == 0)
        return;

      var newSpan = TextSpan.FromBounds(left.Span.End, right.SpanStart);
      var newLocation = Location.Create(context.Node.SyntaxTree, newSpan);
      var diagnostic = Diagnostic.Create(Rule, newLocation);

      context.ReportDiagnostic(diagnostic);
    }
  }
}