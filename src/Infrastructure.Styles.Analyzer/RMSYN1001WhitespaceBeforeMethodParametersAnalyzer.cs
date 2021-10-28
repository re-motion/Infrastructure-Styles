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
  public class RMSYN1001WhitespaceBeforeMethodParametersAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "RMSYN1001";

    internal const string Title = "Invalid number of whitespaces before parameters in method declaration";
    internal const string Message = "There must be exactly ONE whitespace before parameters in method declaration";
    internal const string Description = "Method declaration must have exactly ONE space before its parameters";
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

      context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
      context.RegisterSyntaxNodeAction(AnalyzeConstructor, SyntaxKind.ConstructorDeclaration);
      context.RegisterSyntaxNodeAction(AnalyzeLocalFunction, SyntaxKind.LocalFunctionStatement);
      context.RegisterSyntaxNodeAction(AnalyzeDelegateDeclaration, SyntaxKind.DelegateDeclaration);
      context.RegisterSyntaxNodeAction(AnalyzeAnonymousMethodExpression, SyntaxKind.AnonymousMethodExpression);
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    private static void AnalyzeAnonymousMethodExpression(SyntaxNodeAnalysisContext context)
    {
      var constructorDeclaration = (AnonymousMethodExpressionSyntax) context.Node;
      var left = constructorDeclaration.DelegateKeyword;
      var right = constructorDeclaration.ParameterList?.OpenParenToken;

      if (right is null)
        return;

      AnalyzeWhitespace(context, left, (SyntaxToken) right);
    }

    private static void AnalyzeDelegateDeclaration(SyntaxNodeAnalysisContext context)
    {
      var delegateDeclaration = (DelegateDeclarationSyntax) context.Node;
      var left = delegateDeclaration.TypeParameterList is not { } typeParameters
        ? delegateDeclaration.Identifier
        : typeParameters.GreaterThanToken;
      var right = delegateDeclaration.ParameterList.OpenParenToken;

      AnalyzeWhitespace(context, left, right);
    }

    private static void AnalyzeLocalFunction(SyntaxNodeAnalysisContext context)
    {
      var localFunctionStatement = (LocalFunctionStatementSyntax) context.Node;
      var left = localFunctionStatement.TypeParameterList is not { } typeParameters
        ? localFunctionStatement.Identifier
        : typeParameters.GreaterThanToken;
      var right = localFunctionStatement.ParameterList.OpenParenToken;

      AnalyzeWhitespace(context, left, right);
    }

    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
      var methodDeclaration = (MethodDeclarationSyntax) context.Node;
      var left = methodDeclaration.TypeParameterList is not { } typeParameters
        ? methodDeclaration.Identifier
        : typeParameters.GreaterThanToken;
      var right = methodDeclaration.ParameterList.OpenParenToken;

      AnalyzeWhitespace(context, left, right);
    }

    private static void AnalyzeConstructor(SyntaxNodeAnalysisContext context)
    {
      var constructorDeclaration = (ConstructorDeclarationSyntax) context.Node;
      var left = constructorDeclaration.Identifier;
      var right = constructorDeclaration.ParameterList.OpenParenToken;

      AnalyzeWhitespace(context, left, right);
    }

    private static void AnalyzeWhitespace(SyntaxNodeAnalysisContext context, SyntaxToken left, SyntaxToken right)
    {
      var trivia = left.TrailingTrivia;
      if (trivia.Count == 0)
      {
        var diagnostic = Diagnostic.Create(Rule, right.GetLocation());

        context.ReportDiagnostic(diagnostic);
      }
      else if (trivia.Count != 1 || !SyntaxFactory.Space.IsEquivalentTo(trivia.First()))
      {
        var newSpan = TextSpan.FromBounds(left.Span.End, right.SpanStart);
        var newLocation = Location.Create(right.Parent!.SyntaxTree, newSpan);
        var diagnostic = Diagnostic.Create(Rule, newLocation);

        context.ReportDiagnostic(diagnostic);
      }
    }
  }
}