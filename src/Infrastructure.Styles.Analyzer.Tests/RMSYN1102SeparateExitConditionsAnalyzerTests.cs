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
  [TestFixture]
  public class RMSYN1102SeparateExitConditionsAnalyzerTests
  {
    private static readonly RMSYN1102SeparateExitConditionsAnalyzer.RMSYN1102SeparateExitConditionsAnalyzer Analyzer =
      new();

    [Test]
    public void Analyze_WithExitConditionsInSeparateIFStatements_Valid ()
    {
      var code = @"
namespace N
{
    class C
    {

        private void Main()
        {
            if(1 > 2)
                return;
            if(2 > 1)
                return;
        }
    }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_WithReturnExitConditionsORedInIFStatement_InValid ()
    {
      var code = @"
namespace N
{
    class C
    {

        private void Main()
        {
            if(↓1 > 2 || 2 > 1)
            {
                return;
            }
        }
    }
}";
      RoslynAssert.Diagnostics(Analyzer,
        ExpectedDiagnostic.Create(
          RMSYN1102SeparateExitConditionsAnalyzer.RMSYN1102SeparateExitConditionsAnalyzer.DiagnosticId,
          RMSYN1102SeparateExitConditionsAnalyzer.RMSYN1102SeparateExitConditionsAnalyzer.Message),
        code);
    }

    [Test]
    public void Analyze_WithNonExitConditionsORedInIFStatement_Valid ()
    {
      var code = @"
namespace N
{
    class C
    {

        private void Main()
        {
            if(1 > 2 || 2 > 1)
            {
                int a = 12;
                return;
            }
        }
    }
}";
      RoslynAssert.Valid(Analyzer, code);
    }


    [Test]
    public void Analyze_ORedExitConditionWithReturnStatement_InValid ()
    {
      var code = @"
namespace N
{
    class C
    {

        private void Main()
        {
            if(↓1 > 2 || 2 > 1)
                return;
        }
    }
}";
      RoslynAssert.Diagnostics(Analyzer,
        ExpectedDiagnostic.Create(
          RMSYN1102SeparateExitConditionsAnalyzer.RMSYN1102SeparateExitConditionsAnalyzer.DiagnosticId,
          RMSYN1102SeparateExitConditionsAnalyzer.RMSYN1102SeparateExitConditionsAnalyzer.Message),
        code);
    }

    [Test]
    public void Analyze_ORedExitConditionWithReturnStatementInParenthesis_InValid ()
    {
      var code = @"
namespace N
{
    class C
    {

        private void Main()
        {
            if((↓1 > 2 || 2 > 1))
                return;
        }
    }
}";
      RoslynAssert.Diagnostics(Analyzer,
        ExpectedDiagnostic.Create(
          RMSYN1102SeparateExitConditionsAnalyzer.RMSYN1102SeparateExitConditionsAnalyzer.DiagnosticId,
          RMSYN1102SeparateExitConditionsAnalyzer.RMSYN1102SeparateExitConditionsAnalyzer.Message),
        code);
    }

    [Test]
    public void Analyze_ANDandORExitConditionWithReturnStatement_InValid ()
    {
      var code = @"
namespace N
{
    class C
    {

        private void Main()
        {
            if(↓(2 > 1 && true) || (2 > 1 && false))
                return;
        }
    }
}";
      RoslynAssert.Diagnostics(Analyzer,
        ExpectedDiagnostic.Create(
          RMSYN1102SeparateExitConditionsAnalyzer.RMSYN1102SeparateExitConditionsAnalyzer.DiagnosticId,
          RMSYN1102SeparateExitConditionsAnalyzer.RMSYN1102SeparateExitConditionsAnalyzer.Message),
        code);
    }

    [Test]
    public void Analyze_ORandANDExitConditionWithReturnStatement_Valid ()
    {
      var code = @"
namespace N
{
    class C
    {

        private void Main()
        {
            if((1 > 2 || true) && (2 > 1 || false))
                return;
        }
    }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_WithSimpleMethodCallAfterORedIF_Valid ()
    {
      var code = @"
namespace N
{
    class C
    {
        private void M () {}

        private void Main()
        {
            if(1 > 2 || 2 > 1)
                M();
        }
    }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_ElseIfWithORExitCondition_InValid ()
    {
      var code = @"
namespace N
{
    class C
    {

        private void Main()
        {
            int a = 0;
            if(a == 0)
                return;
            else if(↓true || false)
                return; 
        }
    }
}";
      RoslynAssert.Diagnostics(Analyzer,
        ExpectedDiagnostic.Create(
          RMSYN1102SeparateExitConditionsAnalyzer.RMSYN1102SeparateExitConditionsAnalyzer.DiagnosticId,
          RMSYN1102SeparateExitConditionsAnalyzer.RMSYN1102SeparateExitConditionsAnalyzer.Message),
        code);
    }

    [Test]
    public void Analyze_IfWithoutORPatternExitCondition_Valid ()
    {
      var code = @"
namespace N
{
    class C
    {

        private void Main()
        {
            int a = 0;
            if(a is 1)
                return;
            if(a is 2)
                return; 
        }
    }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_IfWithORPatternExitCondition_InValid ()
    {
      var code = @"
namespace N
{
    class C
    {

        private void Main()
        {
            int a = 0;
            if(a is ↓1 or 2)
                return; 
        }
    }
}";
      RoslynAssert.Diagnostics(Analyzer,
        ExpectedDiagnostic.Create(
          RMSYN1102SeparateExitConditionsAnalyzer.RMSYN1102SeparateExitConditionsAnalyzer.DiagnosticId,
          RMSYN1102SeparateExitConditionsAnalyzer.RMSYN1102SeparateExitConditionsAnalyzer.Message),
        code);
    }

    [Test]
    public void Analyze_ElseIfWithORPatternExitCondition_InValid ()
    {
      var code = @"
namespace N
{
    class C
    {

        private void Main()
        {
            int a = 0;
            if(true)
                return;
            else if(a is ↓1 or 2)
                return;
        }
    }
}";
      RoslynAssert.Diagnostics(Analyzer,
        ExpectedDiagnostic.Create(
          RMSYN1102SeparateExitConditionsAnalyzer.RMSYN1102SeparateExitConditionsAnalyzer.DiagnosticId,
          RMSYN1102SeparateExitConditionsAnalyzer.RMSYN1102SeparateExitConditionsAnalyzer.Message),
        code);
    }
  }
}