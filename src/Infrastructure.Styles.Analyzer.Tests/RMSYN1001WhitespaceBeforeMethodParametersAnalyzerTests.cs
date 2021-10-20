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
using StyleAnalyzer = Infrastructure.Styles.Analyzer.RMSYN1001WhitespaceBeforeMethodParametersAnalyzer;

[assembly: MetadataReference(typeof(object))]

namespace Infrastructure.Styles.Analyzer.Tests
{
  public class RMSYN1001WhitespaceBeforeMethodParametersAnalyzerTests
  {
    private static readonly StyleAnalyzer Analyzer = new();

    [Test]
    public void Analyze_WithOneWhitespace_Valid()
    {
      var code = @"
namespace N
{
    class C
    {
      private void ibimsafunktion () {}
    }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_WithoutWhitespace_Invalid()
    {
      var code = @"
namespace N
{
    class C
    {
      private void ibimsafunktion↓() {}
    }
}";
      RoslynAssert.Diagnostics(Analyzer,
        ExpectedDiagnostic.Create(
          StyleAnalyzer.DiagnosticId,
          StyleAnalyzer.Message),
        code);
    }

    [Test]
    public void Analyze_WithTwoWhitespaces_Invalid()
    {
      var code = @"
namespace N
{
    class C
    {
      private void ibimsafunktion↓  () {}
    }
}";
      RoslynAssert.Diagnostics(Analyzer,
        ExpectedDiagnostic.Create(
          StyleAnalyzer.DiagnosticId,
          StyleAnalyzer.Message),
        code);
    }

    [Test]
    public void Analyze_WithNewline_Invalid()
    {
      var code = @"
namespace N
{
    class C
    {
      private void ibimsafunktion↓
      () {}
    }
}";
      RoslynAssert.Diagnostics(Analyzer,
        ExpectedDiagnostic.Create(
          StyleAnalyzer.DiagnosticId,
          StyleAnalyzer.Message),
        code);
    }

    [Test]
    public void Analyze_WithWhitespaceAndNewline_Invalid()
    {
      var code = @"
namespace N
{
    class C
    {
      private void ibimsafunktion↓ 
      () {}
    }
}";
      RoslynAssert.Diagnostics(Analyzer,
        ExpectedDiagnostic.Create(
          StyleAnalyzer.DiagnosticId,
          StyleAnalyzer.Message),
        code);
    }

    [Test]
    public void Analyze_WithNoWhitespaceInGenericMethod_Invalid()
    {
      var code = @"
namespace N
{
    class C
    {
      private void ibimsafunktion<T>↓() {}
    }
}";
      RoslynAssert.Diagnostics(Analyzer,
        ExpectedDiagnostic.Create(
          StyleAnalyzer.DiagnosticId,
          StyleAnalyzer.Message),
        code);
    }

    [Test]
    public void Analyze_WithOneWhitespaceInGenericMethod_Valid()
    {
      var code = @"
namespace N
{
    class C
    {
      private void ibimsafunktion<T> () {}
    }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_WithNoWhitespaceInConstructor_Invalid()
    {
      var code = @"
namespace N
{
    class C
    {
      private C↓() {}
    }
}";
      RoslynAssert.Diagnostics(Analyzer,
        ExpectedDiagnostic.Create(
          StyleAnalyzer.DiagnosticId,
          StyleAnalyzer.Message),
        code);
    }

    [Test]
    public void Analyze_WithOneWhitespaceInConstructor_Valid()
    {
      var code = @"
namespace N
{
    class C
    {
      private C () {}
    }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_WithOneWhitespaceInLocalFunctionDeclaration_Valid()
    {
      var code = @"
namespace N
{
    class C
    {
      private void ibimsafunktion ()
      {
        int L ()
        {
          return 1;
        }
      }
    }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_WithNoWhitespaceInLocalFunctionDeclaration_Invalid()
    {
      var code = @"
namespace N
{
    class C
    {
      private void ibimsafunktion ()
      {
        int L↓()
        {
          return 1;
        }
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
    public void Analyze_WithOneWhitespaceInGenericLocalFunctionDeclaration_Valid()
    {
      var code = @"
namespace N
{
    class C
    {
      private void ibimsafunktion ()
      {
        int L<T> ()
        {
          return 1;
        }
      }
    }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_WithNoWhitespaceInGenericLocalFunctionDeclaration_Invalid()
    {
      var code = @"
namespace N
{
    class C
    {
      private void ibimsafunktion ()
      {
        int L<T>↓()
        {
          return 1;
        }
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
    public void Analyze_WithOneWhitespaceInDelegateDeclaration_Valid()
    {
      var code = @"
namespace N
{
    class C
    {
      delegate void Del (string str);
    }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_WithNoWhitespaceInDelegateDeclaration_Invalid()
    {
      var code = @"
namespace N
{
    class C
    {
      delegate void Del↓(string str);
    }
}";
      RoslynAssert.Diagnostics(Analyzer,
        ExpectedDiagnostic.Create(
          StyleAnalyzer.DiagnosticId,
          StyleAnalyzer.Message),
        code);
    }

    [Test]
    public void Analyze_WithOneWhitespaceInGenericDelegateDeclaration_Valid()
    {
      var code = @"
namespace N
{
    class C
    {
      delegate void Del<T> (T anyParameter);
    }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_WithNoWhitespaceInGenericDelegateDeclaration_Invalid()
    {
      var code = @"
namespace N
{
    class C
    {
      delegate void Del<T>↓(T anyParameter);
    }
}";
      RoslynAssert.Diagnostics(Analyzer,
        ExpectedDiagnostic.Create(
          StyleAnalyzer.DiagnosticId,
          StyleAnalyzer.Message),
        code);
    }

    [Test]
    public void Analyze_WithOneWhitespaceInAnonymousMethodExpression_Valid()
    {
      var code = @"
namespace N
{
    class C
    {
      public delegate void Print (int value);
      
      private void ibimsafunktion ()
      {
        Print print = delegate (int val) 
        {
           var a = val;
        };
      }
    }
}";
      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_WithNoWhitespaceInAnonymousMethodExpression_Invalid()
    {
      var code = @"
namespace N
{
    class C
    {
      public delegate void Print (int value);
      
      private void ibimsafunktion ()
      {
        Print print = delegate↓(int val) 
        {
           var a = val;
        };
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
    public void Analyze_WithNoParameterListInAnonymousMethodExpression_Valid()
    {
      var code = @"
namespace N
{
    class C
    {
      public delegate void Print (int value);
      
      private void ibimsafunktion ()
      {
        Print print = delegate
        {
        };
      }
    }
}";
      RoslynAssert.Valid(Analyzer, code);
    }
  }
}