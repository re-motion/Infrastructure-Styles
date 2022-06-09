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

namespace Infrastructure.Styles.Analyzer
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class RMSYN1007NoWhitespaceInCastAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "RMSYN1007";

    internal const string Title = "Invalid number of whitespaces in cast expression";
    internal const string Message = "There must be no whitespace in a cast expression";
    internal const string Description = "Cast expression must no contain a space";
    internal const string Category = "Formatting";

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

      context.RegisterSyntaxNodeAction(AnalyzeCastExpression, SyntaxKind.CastExpression);
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    private static void AnalyzeCastExpression (SyntaxNodeAnalysisContext context)
    {
      var constructorDeclaration = (CastExpressionSyntax) context.Node;
      var left = constructorDeclaration.CloseParenToken;
      var right = constructorDeclaration.Expression.GetFirstToken();

      SimpleWhitespaceAnalyzer.AnalyzeNoWhitespaceBetweenTokens(
          ref context,
          Rule,
          in left,
          in right);
    }
  }
}