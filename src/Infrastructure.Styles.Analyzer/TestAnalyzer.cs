using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Infrastructure.Styles.Analyzer
{
  [DiagnosticAnalyzer (LanguageNames.CSharp)]
  public class TestAnalyzer : DiagnosticAnalyzer
  {
    public override void Initialize (AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis (GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
      ImmutableArray<DiagnosticDescriptor>.Empty;
  }
}