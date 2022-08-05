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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Infrastructure.Styles.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RMSYN1103ExtensionMethodsTestedAsNormalInvocationSyntaxAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "RMSYN1103";

        internal const string Title = "Extension methods are tested using normal method invocation syntax";
        internal const string Message = "Extension methods should be tested with normal method invocation syntax";

        internal const string Description =
            "Extension methods should be tested using normal method invocation instead of by extension";

        internal const string Category = "Readability";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            Message,
            Category,
            DiagnosticSeverity.Warning,
            true,
            Description);

        private static readonly List<string> TestAttributeList = new List<string> { "Test", "TestAttribute" };

        public override void Initialize (AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {

                if (compilationContext.Compilation.GetTypeByMetadataName("NUnit.Framework.NUnitAttribute") == null)
                    return;

                compilationContext.RegisterSyntaxNodeAction(AnalyzeDeclarationExpression,
                    SyntaxKind.MethodDeclaration);
            });
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Rule);

        private static void AnalyzeInvocationExpression (InvocationExpressionSyntax node,
            SyntaxNodeAnalysisContext context)
        {
            var symbolInfo = context.SemanticModel.GetSymbolInfo(node, context.CancellationToken);
            if (symbolInfo.Symbol is not IMethodSymbol { IsExtensionMethod: true } methodSymbol)
                return;

            if (methodSymbol.ReducedFrom == null)
                return;

            context.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation()));
        }

        private static void AnalyzeDeclarationExpression (SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            if (!IsTestMethod(methodDeclaration))
                return;

            var body = methodDeclaration.Body;
            if (body == null)
                return;

            var descendantNodes = body.DescendantNodes(node => !node.IsKind(SyntaxKind.InvocationExpression));
            var descendantInvocations = descendantNodes.Where(n => n.IsKind(SyntaxKind.InvocationExpression));
            foreach (var invocation in descendantInvocations)
                AnalyzeInvocationExpression((InvocationExpressionSyntax)invocation, context);
        }

        private static bool IsTestMethod (MethodDeclarationSyntax methodDeclaration)
        {
            if (methodDeclaration.AttributeLists.Count == 0)
                return false;

            foreach (var attributes in methodDeclaration.AttributeLists.SelectMany(attributeList =>
                attributeList.Attributes))
            {
                if (attributes.Name.IsKind(SyntaxKind.IdentifierToken) ||
                    attributes.Name.IsKind(SyntaxKind.IdentifierName))
                    if (IsContainedInTestAttributeList(attributes.Name.ToString()))
                        return true;

                if (attributes.Name is QualifiedNameSyntax name &&
                    IsContainedInTestAttributeList(name.Right.ToString()))
                    return true;
            }

            return false;
        }

        private static bool IsContainedInTestAttributeList (string identifier)
        {
            return TestAttributeList.Any(a => a.StartsWith(identifier));
        }
    }
}