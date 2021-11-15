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

using Gu.Roslyn.Asserts;
using NUnit.Framework;
using StyleAnalyzer = Infrastructure.Styles.Analyzer.RMSYN1008NoWhitespaceInAttributeAnalyzer;

[assembly: MetadataReference(typeof(object))]

namespace Infrastructure.Styles.Analyzer.Tests
{
  public class RMSYN1008NoWhitespaceInAttributeAnalyzerTests
  {
    private static readonly StyleAnalyzer Analyzer = new();

    [Test]
    public void Analyze_WithNoParenthesis_Valid ()
    {
      var code = @"
namespace N
{
    [System.Serializable]
    class C
    {
    }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_WithNoWhitespace_Valid ()
    {
      var code = @"
namespace N
{
    [System.Serializable()]
    class C
    {
    }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_WithoutWhitespace_Invalid ()
    {
      var code = @"
namespace N
{
    [System.Serializable↓ ()]
    class C
    {
    }
}";
      RoslynAssert.Diagnostics(
          Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              StyleAnalyzer.Message),
          code);
    }

    [Test]
    public void Analyze_WithTwoWhitespaces_Invalid ()
    {
      var code = @"
namespace N
{
    [System.Serializable↓   ()]
    class C
    {
    }
}";
      RoslynAssert.Diagnostics(
          Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              StyleAnalyzer.Message),
          code);
    }

    [Test]
    public void Analyze_WithNewline_Invalid ()
    {
      var code = @"
namespace N
{
    [System.Serializable↓
         ()]
    class C
    {
    }
}";
      RoslynAssert.Diagnostics(
          Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              StyleAnalyzer.Message),
          code);
    }
  }
}