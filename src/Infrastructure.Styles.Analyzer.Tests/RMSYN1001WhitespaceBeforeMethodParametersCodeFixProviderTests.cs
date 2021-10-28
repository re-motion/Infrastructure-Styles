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

[assembly: MetadataReference(typeof(object))]

namespace Infrastructure.Styles.Analyzer.Tests
{
  public class RMSYN1001WhitespaceBeforeMethodParametersCodeFixProviderTests
  {
    private static readonly RMSYN1001WhitespaceBeforeMethodParametersAnalyzer Analyzer = new();
    private static readonly RMSYN1001WhitespaceBeforeMethodParametersCodeFixProvider CodeFixProvider = new();

    [Test]
    public void CodeFix_WithNoWhitespace()
    {
      var before = @"
namespace N
{
    class C
    {
      private void ibimsafunktion↓() {}
    }
}";

      var after = @"
namespace N
{
    class C
    {
      private void ibimsafunktion () {}
    }
}";
      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }

    [Test]
    public void CodeFix_WithTwoWhitespaces()
    {
      var before = @"
namespace N
{
    class C
    {
      private void ibimsafunktion↓  () {}
    }
}";

      var after = @"
namespace N
{
    class C
    {
      private void ibimsafunktion () {}
    }
}";
      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }

    [Test]
    public void CodeFix_WithNewLine()
    {
      var before = @"
namespace N
{
    class C
    {
      private void ibimsafunktion↓
      () {}
    }
}";

      var after = @"
namespace N
{
    class C
    {
      private void ibimsafunktion () {}
    }
}";
      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }

    [Test]
    public void CodeFix_WithWhitespaceAndNewline()
    {
      var before = @"
namespace N
{
    class C
    {
      private void ibimsafunktion↓ 
      () {}
    }
}";

      var after = @"
namespace N
{
    class C
    {
      private void ibimsafunktion () {}
    }
}";
      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }

    [Test]
    public void CodeFix_WithMultipleCodeFixes()
    {
      var before = @"
namespace N
{
    class C
    {
      private void ibimsafunktion↓ 
      () {}

      private void ibimsazweitefunktion↓() {}
    }
}";

      var after = @"
namespace N
{
    class C
    {
      private void ibimsafunktion () {}

      private void ibimsazweitefunktion () {}
    }
}";
      RoslynAssert.FixAll(Analyzer, CodeFixProvider, before, after);
    }

    [Test]
    public void CodeFix_WithWhitespaceInConstructor()
    {
      var before = @"
namespace N
{
    class C
    {
      public C↓() {}
    }
}";

      var after = @"
namespace N
{
    class C
    {
      public C () {}
    }
}";
      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }

    [Test]
    public void CodeFix_WithNoWhitespaceAfterTypeParameterList()
    {
      var before = @"
namespace N
{
    class C
    {
      private void ibimsafunktion<T>↓() {}
    }
}";

      var after = @"
namespace N
{
    class C
    {
      private void ibimsafunktion<T> () {}
    }
}";
      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }

    [Test]
    public void CodeFix_WithNoWhitespaceInLocalFunctionDeclaration()
    {
      var before = @"
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

      var after = @"
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
      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }


    [Test]
    public void CodeFix_WithTwoWhitespacesInLocalFunctionDeclaration()
    {
      var before = @"
namespace N
{
    class C
    {
      private void ibimsafunktion ()
      {
        System.String L↓  ()
        {
          return ""1"";
        }
      }
    }
}";

      var after = @"
namespace N
{
    class C
    {
      private void ibimsafunktion ()
      {
        System.String L ()
        {
          return ""1"";
        }
      }
    }
}";
      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }

    [Test]
    public void CodeFix_WithNoWhitespaceInGenericLocalFunctionDeclaration()
    {
      var before = @"
namespace N
{
    class C
    {
      private void ibimsafunktion ()
      {
        System.String L<T>↓()
        {
          return ""1"";
        }
      }
    }
}";

      var after = @"
namespace N
{
    class C
    {
      private void ibimsafunktion ()
      {
        System.String L<T> ()
        {
          return ""1"";
        }
      }
    }
}";
      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }

    [Test]
    public void CodeFix_WithNoWhitespaceInDelegateDeclaration()
    {
      var before = @"
namespace N
{
    class C
    {
      delegate void Del↓(string str);
    }
}";

      var after = @"
namespace N
{
    class C
    {
      delegate void Del (string str);
    }
}";
      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }

    [Test]
    public void CodeFix_WithNoWhitespaceInGenericDelegateDeclaration()
    {
      var before = @"
namespace N
{
    class C
    {
      delegate void Del<T>↓(T str);
    }
}";

      var after = @"
namespace N
{
    class C
    {
      delegate void Del<T> (T str);
    }
}";
      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }

    [Test]
    public void CodeFix_WithNoWhitespaceInAnonymousMethodExpression()
    {
      var before = @"
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

      var after = @"
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
      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }
  }
}