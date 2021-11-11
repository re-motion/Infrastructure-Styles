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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using StyleAnalyzer = Infrastructure.Styles.Analyzer.RMSYN1003WhitespaceBeforeKeywordExpressionAnalyzer;

namespace Infrastructure.Styles.Analyzer.Tests
{
  public class RMSYN1003WhitespaceBeforeKeywordExpressionAnalyzerTests
  {
    private static readonly StyleAnalyzer Analyzer = new();

    private static readonly CSharpCompilationOptions s_unsafeDllCompilationOptions = new(
        OutputKind.DynamicallyLinkedLibrary,
        allowUnsafe: true);

    [Test]
    public void Analyze_TypeOfWithNoWhitespace_Valid ()
    {
      var code = @"
namespace N
{
    class C
    {
      private System.Type StringType () => typeof(string);
    }
}";

      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_TypeOfWithOneWhitespace_Invalid ()
    {
      var code = @"
namespace N
{
    class C
    {
      private System.Type StringType () => typeof↓ (string);
    }
}";

      RoslynAssert.Diagnostics(
          Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              string.Format(StyleAnalyzer.MessageFormat, "typeof")),
          code);
    }

    [Test]
    public void Analyze_TypeOfWithMoreThanOneWhitespace_Invalid ()
    {
      var code = @"
namespace N
{
    class C
    {
      private System.Type StringType () => typeof↓   (string);
    }
}";

      RoslynAssert.Diagnostics(
          Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              string.Format(StyleAnalyzer.MessageFormat, "typeof")),
          code);
    }

    [Test]
    public void Analyze_TypeOfWithMoreThanOneWhitespaceAndNewLine_Invalid ()
    {
      var code = @"
namespace N
{
    class C
    {
      private System.Type StringType () => typeof↓   
          (string);
    }
}";

      RoslynAssert.Diagnostics(
          Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              string.Format(StyleAnalyzer.MessageFormat, "typeof")),
          code);
    }

    [Test]
    public void Analyze_SizeOfWithNoWhitespace_Valid ()
    {
      var code = @"
namespace N
{
    class C
    {
      private void SizeOf ()
      {
        unsafe { System.Diagnostics.Debug.WriteLine(sizeof(byte)); }
      }
    }
}";

      RoslynAssert.Valid(Analyzer, code, compilationOptions: s_unsafeDllCompilationOptions);
    }

    [Test]
    public void Analyze_SizeOfWithOneWhitespace_Invalid ()
    {
      var code = @"
namespace N
{
    class C
    {
      private void SizeOf ()
      {
        unsafe { System.Diagnostics.Debug.WriteLine(sizeof↓ (byte)); }
      }
    }
}";

      RoslynAssert.Diagnostics(
          Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              string.Format(StyleAnalyzer.MessageFormat, "sizeof")),
          code,
          compilationOptions: s_unsafeDllCompilationOptions);
    }

    [Test]
    public void Analyze_SizeOfWithMoreThanOneWhitespace_Invalid ()
    {
      var code = @"
namespace N
{
    class C
    {
      private void SizeOf ()
      {
        unsafe { System.Diagnostics.Debug.WriteLine(sizeof↓     (byte)); }
      }
    }
}";

      RoslynAssert.Diagnostics(
          Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              string.Format(StyleAnalyzer.MessageFormat, "sizeof")),
          code,
          compilationOptions: s_unsafeDllCompilationOptions);
    }

    [Test]
    public void Analyze_SizeOfWithMoreThanOneWhitespaceAndNewLine_Invalid ()
    {
      var code = @"
namespace N
{
    class C
    {
      private void SizeOf ()
      {
        unsafe
        {
          System.Diagnostics.Debug.WriteLine(sizeof↓  
              (byte));
        }
      }
    }
}";

      RoslynAssert.Diagnostics(
          Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              string.Format(StyleAnalyzer.MessageFormat, "sizeof")),
          code,
          compilationOptions: s_unsafeDllCompilationOptions);
    }

    [Test]
    public void Analyze_DefaultWithNoWhitespace_Valid ()
    {
      var code = @"
namespace N
{
    class C
    {
      private object Default () => default(string);
    }
}";

      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_DefaultWithoutArgument_Valid ()
    {
      var code = @"
namespace N
{
    class C
    {
      private object Default () => default  ;
    }
}";

      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_DefaultWithOneWhitespace_Invalid ()
    {
      var code = @"
namespace N
{
    class C
    {
      private object Default () => default↓ (string);
    }
}";

      RoslynAssert.Diagnostics(
          Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              string.Format(StyleAnalyzer.MessageFormat, "default")),
          code);
    }

    [Test]
    public void Analyze_DefaultWithMoreThanOneWhitespace_Invalid ()
    {
      var code = @"
namespace N
{
    class C
    {
      private object Default () => default↓    (string);
    }
}";

      RoslynAssert.Diagnostics(
          Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              string.Format(StyleAnalyzer.MessageFormat, "default")),
          code);
    }

    [Test]
    public void Analyze_DefaultWithMoreThanOneWhitespaceAndNewLine_Invalid ()
    {
      var code = @"
namespace N
{
    class C
    {
      private object Default () => default↓   
          (string);
    }
}";

      RoslynAssert.Diagnostics(
          Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              string.Format(StyleAnalyzer.MessageFormat, "default")),
          code);
    }

    [Test]
    public void Analyze_CheckedWithNoWhitespace_Valid ()
    {
      var code = @"
namespace N
{
    class C
    {
      private int Checked () => checked(1 + 1);
    }
}";

      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_CheckedBlockWithWeirdFormatting_Valid ()
    {
      var code = @"
namespace N
{
    class C
    {
      private int Checked ()
      {
        checked    {
          return 1 + 1;
        };
      }
    }
}";

      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_UncheckedBlockWithWeirdFormatting_Valid ()
    {
      var code = @"
namespace N
{
    class C
    {
      private int Unchecked ()
      {
        unchecked    {
          return 1 + 1;
        };
      }
    }
}";

      RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public void Analyze_CheckedWithOneWhitespace_Invalid ()
    {
      var code = @"
namespace N
{
    class C
    {
      private int Checked () => checked↓ (1 + 1);
    }
}";

      RoslynAssert.Diagnostics(
          Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              string.Format(StyleAnalyzer.MessageFormat, "checked")),
          code);
    }

    [Test]
    public void Analyze_CheckedWithMoreThanOneWhitespace_Invalid ()
    {
      var code = @"
namespace N
{
    class C
    {
      private int Checked () => checked↓    (1 + 1);
    }
}";

      RoslynAssert.Diagnostics(
          Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              string.Format(StyleAnalyzer.MessageFormat, "checked")),
          code);
    }

    [Test]
    public void Analyze_CheckedWithMoreThanOneWhitespaceAndNewLine_Invalid ()
    {
      var code = @"
namespace N
{
    class C
    {
      private int Checked () => checked↓    
          (1 + 1);
    }
}";

      RoslynAssert.Diagnostics(
          Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              string.Format(StyleAnalyzer.MessageFormat, "checked")),
          code);
    }

    [Test]
    public void Analyze_BaseConstructorInitializerWithOneWhitespace_Invalid ()
    {
      var code = @"
namespace N
{
    class C
    {
      public C(): base↓ () {}
    }
}";

      RoslynAssert.Diagnostics(
          Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              string.Format(StyleAnalyzer.MessageFormat, "base")),
          code);
    }

    [Test]
    public void Analyze_BaseConstructorInitializeWithMoreThanOneWhitespace_Invalid ()
    {
      var code = @"
namespace N
{
    class C
    {
      public C(): base↓    () {}
    }
}";

      RoslynAssert.Diagnostics(
          Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              string.Format(StyleAnalyzer.MessageFormat, "base")),
          code);
    }

    [Test]
    public void Analyze_BaseConstructorInitializerWithMoreThanOneWhitespaceAndNewLine_Invalid ()
    {
      var code = @"
namespace N
{
    class C
    {
      public C(): base↓
                    () {}
    }
}";

      RoslynAssert.Diagnostics(
          Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              string.Format(StyleAnalyzer.MessageFormat, "base")),
          code);
    }

    [Test]
    public void Analyze_ThisConstructorInitializerWithOneWhitespace_Invalid ()
    {
      var code = @"
namespace N
{
    class C
    {
      public C(): this↓ (3) {}
      public C(int a) {}
    }
}";

      RoslynAssert.Diagnostics(
          Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              string.Format(StyleAnalyzer.MessageFormat, "this")),
          code);
    }

    [Test]
    public void Analyze_ThisConstructorInitializeWithMoreThanOneWhitespace_Invalid ()
    {
      var code = @"
namespace N
{
    class C
    {
      public C(): this↓    (3) {}
      public C(int a) {}
    }
}";

      RoslynAssert.Diagnostics(
          Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              string.Format(StyleAnalyzer.MessageFormat, "this")),
          code);
    }

    [Test]
    public void Analyze_ThisConstructorInitializerWithMoreThanOneWhitespaceAndNewLine_Invalid ()
    {
      var code = @"
namespace N
{
    class C
    {
      public C(): this↓
                    (3) {}
      public C(int a) {}
    }
}";

      RoslynAssert.Diagnostics(
          Analyzer,
          ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              string.Format(StyleAnalyzer.MessageFormat, "this")),
          code);
    }
  }
}