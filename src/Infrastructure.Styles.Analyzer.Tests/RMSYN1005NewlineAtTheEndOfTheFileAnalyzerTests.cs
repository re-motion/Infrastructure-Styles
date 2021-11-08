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
using StyleAnalyzer = Infrastructure.Styles.Analyzer.RMSYN1005NewlineAtTheEndOfTheFileAnalyzer;

namespace Infrastructure.Styles.Analyzer.Tests
{
  [TestFixture]
  public class RMSYN1005NewlineAtTheEndOfTheFileAnalyzerTests
  {
    private static readonly StyleAnalyzer Analyzer = new();

    [Test]
    public void Analyze_WithOneLinuxStyleNewlineAtTheEndOfTheFile_Valid ()
    {
      var code = @"
namespace N
{
    class C { }
}" + "\n";

      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_WithTriviaAtTheEnd_Valid ()
    {
      var code = @"
namespace N
{
    class C { }
}
// test" + "\r\n";

      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_WithPreprocessorDirective_Valid ()
    {
      var code = @"
#if DEBUG
namespace N
{
    class C { }
}
#endif
";

      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_WithOneWindowsStyleNewlineAtTheEndOfTheFile_Valid ()
    {
      var code = @"
namespace N
{
    class C { }
}" + "\r\n";

      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_WithSpacesAndNoNewlineAtTheEndOfTheFile_Invalid ()
    {
      var code = @"
namespace N
{
    class C { }
↓}  ";

      RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.Create(StyleAnalyzer.DiagnosticId, StyleAnalyzer.Message), code);
    }

    [Test]
    public void Analyze_WithNoNewlineAtTheEndOfTheFile_Invalid ()
    {
      var code = @"
namespace N
{
    class C { }
↓}";

      RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.Create(StyleAnalyzer.DiagnosticId, StyleAnalyzer.Message), code);
    }

    [Test]
    public void Analyze_WithTwoNewlinesAtTheEndOfTheFile_Invalid ()
    {
      var code = @"
namespace N
{
    class C { }
}
↓
";
      RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.Create(StyleAnalyzer.DiagnosticId, StyleAnalyzer.Message), code);
    }
  }
}