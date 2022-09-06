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
using StyleAnalyzer = Infrastructure.Styles.Analyzer.RMSYN1104OperatorChainingWithNewlineAnalyzer;

namespace Infrastructure.Styles.Analyzer.Tests
{
  [TestFixture]
  public class RMSYN1104OperatorChainingWithNewlineAnalyzerTests
  {
    private StyleAnalyzer Analyzer = new StyleAnalyzer();

    [Test]
    [TestCase("||")]
    [TestCase("&&")]
    [TestCase("^")]
    [TestCase("&")]
    [TestCase("|")]
    public void Analyze_WithChainingInSameLine_Valid (string addedOperator)
    {
      var code = @"
namespace N
{
  class C
  {
    void Main()
    {
      var bool1 = false;
      var bool2 = true;

      var testBoolean = bool1 " + addedOperator + @" bool2;
    }
  }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    [TestCase("||")]
    [TestCase("&&")]
    [TestCase("^")]
    [TestCase("&")]
    [TestCase("|")]
    public void Analyze_WithBooleanOperatorChaining_Invalid (string addedOperator)
    {
      var code = @"
namespace N
{
  class C
  {
    void Main()
    {
      var bool1 = false;
      var bool2 = true;

      var testBoolean = bool1 ↓" + addedOperator + @"
          bool2;
    }
  }
}";
      RoslynAssert.Diagnostics(Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              StyleAnalyzer.Message),
          code);
    }

    [Test]
    [TestCase("+")]
    [TestCase("-")]
    [TestCase("*")]
    [TestCase("/")]
    [TestCase("%")]
    public void Analyze_WithMathOperatorChaining_Invalid (string addedOperator)
    {
      var code = @"
namespace N
{
  class C
  {
    void Main()
    {
      var value1 = 5;
      var value2 = 12;

      var test = value1 ↓" + addedOperator + @"
          value2;
    }
  }
}";
      RoslynAssert.Diagnostics(Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              StyleAnalyzer.Message),
          code);
    }

    [Test]
    [TestCase("+")]
    [TestCase("-")]
    [TestCase("*")]
    [TestCase("/")]
    [TestCase("%")]
    public void Analyze_WithMathOperatorChaining_Valid (string addedOperator)
    {
      var code = @"
namespace N
{
  class C
  {
    void Main()
    {
      var value1 = 5;
      var value2 = 12;

      var test = value1 
          " + addedOperator + @" value2;
    }
  }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    [TestCase(">")]
    [TestCase(">=")]
    [TestCase("==")]
    [TestCase("!=")]
    [TestCase("<=")]
    [TestCase("<")]
    public void Analyze_WithComparisonOperators_Valid (string addedOperator)
    {
      var code = @"
namespace N
{
  class C
  {
    void Main()
    {
      var test = 5 " + addedOperator + @" 7;
    }
  }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    [TestCase(">")]
    [TestCase(">=")]
    [TestCase("==")]
    [TestCase("!=")]
    [TestCase("<=")]
    [TestCase("<")]
    public void Analyze_WithComparisonOperators_InValid (string addedOperator)
    {
      var code = @"
namespace N
{
  class C
  {
    void Main()
    {
      var test = 5 ↓" + addedOperator + @"
          7;
    }
  }
}";
      RoslynAssert.Diagnostics(Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              StyleAnalyzer.Message),
          code);
    }

    [Test]
    [TestCase("var testBoolean = boolean ? someInt : someOtherInt")]
    [TestCase(@"var testBoolean = boolean
                    ? someInt
                    : someOtherInt")]
    public void Analyze_WithTernaryOperator_Valid (string validExpression)
    {
      var code = @"
namespace N
{
  class C
  {
    void Main()
    {
      var boolean = false;
      var someInt = 0;
      var someOtherInt = 5;

      " + validExpression + @";
    }
  }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    [TestCase(@"var testBoolean = boolean ↓?
                    someInt 
                    : someOtherInt")]
    [TestCase(@"var testBoolean = boolean
                    ? someInt ↓: 
                    someOtherInt")]
    [TestCase(@"var testBoolean = boolean ↓?
                    someInt ↓: 
                    someOtherInt")]
    public void Analyze_WithTernaryOperator_InValid (string invalidExpression)
    {
      var code = @"
namespace N
{
  class C
  {
    void Main()
    {
      var boolean = false;
      var someInt = 0;
      var someOtherInt = 5;

      " + invalidExpression + @";
    }
  }
}";
      RoslynAssert.Diagnostics(Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              StyleAnalyzer.Message),
          code);
    }

    [Test]
    [TestCase("someInt.ToString()")]
    [TestCase(@"someInt
                  .ToString()
                  .Replace('A', 'b')")]
    public void Analyze_WithSimpleMemberAccessExpression_Valid (string invocationCall)
    {
      var code = @"
namespace N
{
  class C
  {
    void Main()
    {
      var someInt = 0;

      " + invocationCall + @";
    }
  }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    [TestCase(@"someInt↓.
                  ToString();")]
    [TestCase(@"someInt↓.
                  ToString()↓.
                  Replace('A', 'b');")]
    public void Analyze_WithSimpleMemberAccessExpression_InValid (string invocationCall)
    {
      var code = @"
namespace N
{
  class C
  {
    void Main()
    {
      var someInt = 0;

      " + invocationCall + @";
    }
  }
}";
      RoslynAssert.Diagnostics(Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              StyleAnalyzer.Message),
          code);
    }

    [Test]
    [TestCase("is 7")]
    [TestCase("is > 7 or < 7")]
    [TestCase("is > 7 and < 10")]
    public void Analyze_WithIsPatterns_Valid (string addedOperator)
    {
      var code = @"
namespace N
{
  class C
  {
    void Main()
    {
      var one = 1;

      var test = one " + addedOperator + @";
    }
  }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    [TestCase(@"↓is
                  7")]
    [TestCase(@"is 4 ↓or
                  7")]
    [TestCase(@"↓is
                  4 ↓or
                  7")]
    [TestCase(@"is > 4 ↓and
                  < 7")]
    [TestCase(@"↓is
                  > 4 ↓and
                  < 7")]
    public void Analyze_WithIsPatternAndOrPatterns_InValid (string isExpression)
    {
      var code = @"
namespace N
{
  class C
  {
    void Main()
    {
      var one = 1;

      var test = one " + isExpression + @";

    }
  }
}";
      RoslynAssert.Diagnostics(Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              StyleAnalyzer.Message),
          code);
    }

    [Test]
    [TestCase(@"one↓?
                 .ToString()")]
    [TestCase(@"one?↓.
                 ToString()")]
    public void Analyze_WithNullConditionalOperator_InValid (string testExpression)
    {
      var code = @"
namespace N
{
  class C
  {
    void Main()
    {

      var one = ""a"";

      var test = " + testExpression + @";

    }
  }
}";
      RoslynAssert.Diagnostics(Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              StyleAnalyzer.Message),
          code);
    }

    [Test]
    public void Analyze_WithNullConditionalOperator_Valid ()
    {
      var code = @"
namespace N
{
  class C
  {
    void Main()
    {

      var one = ""a"";

      var test = one?.ToString();

    }
  }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_WithNullCoalescingOperator_InValid ()
    {
      var code = @"
namespace N
{
  class C
  {
    void Main()
    {

      string? one = ""a"";

      var test = one ↓??
          ""b"";

    }
  }
}";
      RoslynAssert.Diagnostics(Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              StyleAnalyzer.Message),
          code);
    }

    [Test]
    public void Analyze_WithNullCoalescingOperator_Valid ()
    {
      var code = @"
namespace N
{
  class C
  {
    void Main()
    {

      string? one = ""a"";

      var test = one ?? ""b"";

    }
  }
}";
      RoslynAssert.Valid(Analyzer, code);
    }
  }
}