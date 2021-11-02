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

namespace Infrastructure.Styles.Analyzer.Tests
{
  public class RMSYN1004EmptyLinesNoSpacesAnalyzerTests
  {
    private static readonly RMSYN1004EmptyLinesShouldNotContainWhitespacesAnalyzer Analyzer = new();

    [Test]
    public void ReportDiagnostic()
    {
      var code = @"
namespace N
{
    class C
    {
↓    
    }
}";

      RoslynAssert.Diagnostics(
        Analyzer,
        code);
    }

    [Test]
    public void ReportDiagnostic_InFirstLine()
    {
      var code = @"↓    
namespace N
{
    class C
    {
    }
}";

      RoslynAssert.Diagnostics(
        Analyzer,
        code);
    }

    [Test]
    public void ReportDiagnostic_InLastLine()
    {
      var code = @"
namespace N
{
    class C
    {
    }
}
↓    ";

      RoslynAssert.Diagnostics(
        Analyzer,
        code);
    }

    [Test]
    public void DoNotReportDiagnostic_IfOtherTriviaIsInSameLine()
    {
      var code = @"
namespace N
{
    class C
    {
    /**/
    }
}";

      RoslynAssert.Valid(
        Analyzer,
        code);
    }

    [Test]
    public void ReportDiagnostic_IfOtherTriviaIsInOtherLine()
    {
      var code = @"
namespace N
{
    class C
    {
↓    
    /**/
    }
}";

      RoslynAssert.Diagnostics(
        Analyzer,
        code);
    }

    [Test]
    public void ReportDiagnostics_ForTrailingSpaces()
    {
      var code = @"
namespace N↓ 
{
    class C↓  
    {↓ 
↓    
↓    
    }
}↓ ";

      RoslynAssert.Diagnostics(
        Analyzer,
        code);
    }

    [Test]
    public void ReportDiagnostics_ForMultipleFollowingLines()
    {
      var code = @"
namespace N
{
    class C
    {
↓    
↓    
    }
}";

      RoslynAssert.Diagnostics(
        Analyzer,
        code);
    }

    [Test]
    public void DoNotReportDiagnostics_IgnoreOtherWhitespaceIssues()
    {
      var code = @"
namespace   N
{
    class    C
    {
      // test
      private
         int     a   ;

      /* xD */
      static   void   T ( )    {
        }
    }
}";

      RoslynAssert.Valid(
        Analyzer,
        code);
    }
  }
}