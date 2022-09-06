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

namespace Infrastructure.Styles.Analyzer
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class RMSYN1104OperatorChainingWithNewlineAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "RMSYN1104";

    internal const string Title = "Operator chaining with newline";
    internal const string Message = "Operator chaining with newline should have the operator at the beginning of a new line";

    internal const string Description =
        "Operator chaining with newline should have the operator at the beginning of a new line for better readability.";

    internal const string Category = "Readability";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        Title,
        Message,
        Category,
        DiagnosticSeverity.Warning,
        true,
        Description);

    public override void Initialize (AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

      context.EnableConcurrentExecution();

      RegisterAnalysisAction<BinaryExpressionSyntax>(context, f => f.OperatorToken,
          SyntaxKind.AddExpression,
          SyntaxKind.SubtractExpression,
          SyntaxKind.MultiplyExpression,
          SyntaxKind.DivideExpression,
          SyntaxKind.ModuloExpression,
          SyntaxKind.LessThanExpression,
          SyntaxKind.LessThanOrEqualExpression,
          SyntaxKind.GreaterThanOrEqualExpression,
          SyntaxKind.GreaterThanExpression,
          SyntaxKind.EqualsExpression,
          SyntaxKind.NotEqualsExpression,
          SyntaxKind.LogicalOrExpression,
          SyntaxKind.LogicalAndExpression,
          SyntaxKind.BitwiseOrExpression,
          SyntaxKind.BitwiseAndExpression,
          SyntaxKind.ExclusiveOrExpression,
          SyntaxKind.IsExpression);

      // is 5
      RegisterAnalysisAction<IsPatternExpressionSyntax>(context, f => f.IsKeyword,
          SyntaxKind.IsPatternExpression);

      // is < 5 and > 7 or 1
      RegisterAnalysisAction<BinaryPatternSyntax>(context, f => f.OperatorToken,
          SyntaxKind.OrPattern,
          SyntaxKind.AndPattern);

      // ? part of ?. operator
      RegisterAnalysisAction<ConditionalAccessExpressionSyntax>(context, f => f.OperatorToken,
          SyntaxKind.ConditionalAccessExpression);
      // . part of ?. operator
      RegisterAnalysisAction<MemberBindingExpressionSyntax>(context, f => f.OperatorToken,
          SyntaxKind.MemberBindingExpression);

      // ??
      RegisterAnalysisAction<BinaryExpressionSyntax>(context, f => f.OperatorToken,
          SyntaxKind.CoalesceExpression);

      // .ToString().Whatever().UndSoWeiter()
      RegisterAnalysisAction<MemberAccessExpressionSyntax>(context, f => f.OperatorToken,
          SyntaxKind.SimpleMemberAccessExpression);

      //ternary operator, aka: something ? yes : no
      context.RegisterSyntaxNodeAction(AnalyzeConditionalExpression, SyntaxKind.ConditionalExpression);
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    private static void RegisterAnalysisAction<T> (AnalysisContext context, Func<T, SyntaxToken> syntaxTokenSelector, params SyntaxKind[] kind) where T : ExpressionOrPatternSyntax
    {
      context.RegisterSyntaxNodeAction(AnalyzeOperatorTokenExpression(syntaxTokenSelector), kind);
    }

    private static Action<SyntaxNodeAnalysisContext> AnalyzeOperatorTokenExpression<T> (Func<T, SyntaxToken> syntaxTokenSelector) where T : ExpressionOrPatternSyntax
    {
      return context =>
      {
        var expression = (T)context.Node;
        ReportDiagnosticIfContainsTrailingEndLine(syntaxTokenSelector(expression), context);
      };
    }

    private static void AnalyzeConditionalExpression (SyntaxNodeAnalysisContext context)
    {
      var expression = (ConditionalExpressionSyntax)context.Node;
      ReportDiagnosticIfContainsTrailingEndLine(expression.ColonToken, context);
      ReportDiagnosticIfContainsTrailingEndLine(expression.QuestionToken, context);
    }

    private static void ReportDiagnosticIfContainsTrailingEndLine (SyntaxToken token, SyntaxNodeAnalysisContext context)
    {
      foreach (var trivia in token.TrailingTrivia)
        if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
          context.ReportDiagnostic(Diagnostic.Create(Rule, token.GetLocation()));
    }
  }
}