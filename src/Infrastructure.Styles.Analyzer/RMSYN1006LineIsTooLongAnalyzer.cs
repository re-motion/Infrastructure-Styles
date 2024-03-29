﻿// This file is part of the re-motion Framework (www.re-motion.org)
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
using Infrastructure.Styles.Analyzer.Infrastructure;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Infrastructure.Styles.Analyzer
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class RMSYN1006LineIsTooLongAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "RMSYN1006";

    private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
        DiagnosticId,
        "Line should not be too long",
        "Line is too long",
        "Formatting",
        DiagnosticSeverity.Warning,
        true);

    private readonly IAnalyzerOptionValueProvider<int> _maxLengthSetting;

    public RMSYN1006LineIsTooLongAnalyzer ()
      : this(AnalyzerOption.MaxLineLength)
    {
    }

    internal RMSYN1006LineIsTooLongAnalyzer (IAnalyzerOptionValueProvider<int> maxLengthSetting)
    {
      _maxLengthSetting = maxLengthSetting;
    }

    /// <inheritdoc />
    public override void Initialize (AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();

      context.RegisterSyntaxTreeAction(Analyze);
    }

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Descriptor);

    private void Analyze (SyntaxTreeAnalysisContext analysisContext)
    {
      var maxLineLength = _maxLengthSetting.GetOptionValue(analysisContext);

      var syntaxTree = analysisContext.Tree;
      var sourceText = syntaxTree.GetText();
      foreach (var line in sourceText.Lines)
      {
        var lineSpan = line.Span;
        var length = lineSpan.Length;
        if (length <= maxLineLength)
          continue;

        var diagnostic = Diagnostic.Create(Descriptor, Location.Create(syntaxTree, lineSpan));
        analysisContext.ReportDiagnostic(diagnostic);
      }
    }
  }
}