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
using StyleAnalyzer = Infrastructure.Styles.Analyzer.RMSYN1002NoWhitespaceAfterMethodInvocationAnalyzer;

namespace Infrastructure.Styles.Analyzer.Tests
{
  [TestFixture]
  public class RMSYN1002NoWhitespaceAfterMethodInvocationAnalyzerTests
  {
    private static readonly StyleAnalyzer Analyzer = new();

    [Test]
    public void Analyze_WithNoWhitespace_Valid()
    {
      var code = @"
namespace N
{
    class C
    {
      private void ibimsafunktion() {}

      private void Main()
      {
        ibimsafunktion();
      }
    }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_WithOneWhitespace_Invalid()
    {
      var code = @"
namespace N
{
    class C
    {
      private void ibimsafunktion () {}

      private void Main()
      {
        ibimsafunktion↓ ();
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
    public void Analyze_WithNoWhitespaceNameof_Valid()
    {
      var code = @"
namespace N
{
    class C
    {
      private void ibimsafunktion (string a) {}

      private void Main()
      {
        var a = nameof(Main);
        ibimsafunktion(a);
      }
    }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_WithOneWhitespaceNameof_Invalid()
    {
      var code = @"
namespace N
{
    class C
    {
      private void ibimsafunktion (string a) {}

      private void Main()
      {
        var a = nameof↓ (Main);
        ibimsafunktion(a);
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
    public void Analyze_WithNoWhitespaceGenericMethodInvocation_Valid()
    {
      var code = @"
namespace N
{
    class C
    {
      private void ibimsafunktion<T>() {}

      private void Main()
      {
        ibimsafunktion<int>();
      }
    }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_WithManyWhitespacesGenericMethodInvocation_Invalid()
    {
      var code = @"
namespace N
{
    class C
    {
      private void ibimsafunktion<T>() {}

      private void Main()
      {
        ibimsafunktion<int>↓     ();
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
    public void Analyze_WithNoWhitespacesObjectCreationExpression_Valid()
    {
      var code = @"
namespace N
{
    class C
    {
      private void Main()
      {
        var a = new C();
      }
    }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_WithOneWhitespacesObjectCreationExpression_Invalid()
    {
      var code = @"
namespace N
{
    class C
    {
      private void Main()
      {
        var a = new C↓ ();
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
    public void Analyze_WithNewLineWhitespacesObjectCreationExpression_Invalid()
    {
      var code = @"
namespace N
{
    class C
    {
      private void Main()
      {
        var a = new C↓
        ();
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
    public void Analyze_WithNoWhitespaceObjectCreationExpressionWithInitializerAndNoArgumentList_Valid()
    {
      var code = @"
namespace N
{
    class C
    {
      private void Main()
      {
        var a = new C
        {};
      }
    }
}";
      RoslynAssert.Valid(Analyzer, code);
    }
    
    [Test]
    public void Analyze_WithOneWhitespaceObjectCreationExpressionWithInitializerAndNoArgumentList_Valid()
    {
      var code = @"
namespace N
{
    class C
    {
      private void Main()
      {
        var a = new C 
        {};
      }
    }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_WithNoWhitespacesImplicitObjectCreationExpression_Valid()
    {
      var code = @"
namespace N
{
    class C
    {
      private void Main()
      {
        C a = new();
      }
    }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_WithOneWhitespacesImplicitObjectCreationExpression_Invalid()
    {
      var code = @"
namespace N
{
    class C
    {
      private void Main()
      {
        C a = new↓ ();
      }
    }
}";
      RoslynAssert.Diagnostics(Analyzer,
        ExpectedDiagnostic.Create(
          StyleAnalyzer.DiagnosticId,
          StyleAnalyzer.Message),
        code);
    }
  }
}