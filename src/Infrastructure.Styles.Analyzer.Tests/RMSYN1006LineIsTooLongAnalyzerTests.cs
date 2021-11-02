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
using Infrastructure.Styles.Analyzer.Infrastructure;
using Microsoft.CodeAnalysis.Diagnostics;
using Moq;
using NUnit.Framework;

namespace Infrastructure.Styles.Analyzer.Tests
{
  public class RMSYN1006LineIsTooLongAnalyzerTests
  {
    private static readonly RMSYN1006LineIsTooLongAnalyzer Analyzer = new();

    [Test]
    public void ReportDiagnostic ()
    {
      var code = @"
namespace N
{
    class C
    {
      // this very long text should not have a diagnostic because it is a little bit shorter than the line below it and exactly 180 characters long on the exact dot................
↓     // this is a very long line that should trigger the analyzer in this test because it is so verrrrrry loooooong why is it not enougth that this line keeeeeps on giiiiiiiiiivingg
    }
}";

      RoslynAssert.Diagnostics(
        Analyzer,
        code);
    }


    [Test]
    public void ReportDiagnostic_WithConfiguredMaxLineLength ()
    {
      var maxLineLengthOptionMock = new Mock<IAnalyzerOptionValueProvider<int>>();
      maxLineLengthOptionMock.Setup(e => e.GetOptionValue(It.IsAny<SyntaxTreeAnalysisContext>()))
        .Returns(120);

      var code = @"
namespace N
{
    class C
    {
      // this very long text should not have a diagnostic because it is a little bit shorter than the line below it.....
↓     // this is a very long line that should trigger the analyzer in this test because it is so verrrrry loong that it is
    }
}";

      RoslynAssert.Diagnostics(
        new RMSYN1006LineIsTooLongAnalyzer(maxLineLengthOptionMock.Object),
        code);
    }

    [Test]
    public void DoNotReportDiagnostic_OnLongDocComment ()
    {
      var code = @"
namespace N
{
    /// <summary>
    /// This is a long doc comment that stretches along multiple lines and each line is
    /// relatively long but no line is longer than the maximum line length which should
    /// not cause a diagnostic to be reported but it did happen once because of an error
    /// </summary>
    class C
    {
    }
}";

      RoslynAssert.Valid(
        Analyzer,
        code);
    }
  }
}