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
  public class RMSYN1002NoWhitespaceAfterMethodInvocationCodeFixProviderTests
  {
    private static readonly RMSYN1002NoWhitespaceAfterMethodInvocationAnalyzer Analyzer = new();
    private static readonly RMSYN1002NoWhitespaceAfterMethodInvocationCodeFixProvider CodeFixProvider = new();

    [Test]
    public void CodeFix_WithOneWhitespaceInMethodInvocation()
    {
      var before = @"
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

      var after = @"
namespace N
{
    class C
    {
      private void ibimsafunktion () {}

      private void Main()
      {
        ibimsafunktion();
      }
    }
}";
      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }

    [Test]
    public void CodeFix_WithOneWhitespaceInNameof()
    {
      var before = @"
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

      var after = @"
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
      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }

    [Test]
    public void CodeFix_WithOneWhitespaceInGenericMethodInvocation()
    {
      var before = @"
namespace N
{
    class C
    {
      private void ibimsafunktion<T> () {}

      private void Main()
      {
        ibimsafunktion<int>↓ ();
      }
    }
}";

      var after = @"
namespace N
{
    class C
    {
      private void ibimsafunktion<T> () {}

      private void Main()
      {
        ibimsafunktion<int>();
      }
    }
}";
      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }

    [Test]
    public void CodeFix_WithNewLineInMethodInvocation()
    {
      var before = @"
namespace N
{
    class C
    {
      private void ibimsafunktion () {}

      private void Main()
      {
        ibimsafunktion↓
        ();
      }
    }
}";

      var after = @"
namespace N
{
    class C
    {
      private void ibimsafunktion () {}

      private void Main()
      {
        ibimsafunktion();
      }
    }
}";
      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }

    [Test]
    public void CodeFix_WithOneWhitespaceInObjectCreationExpression()
    {
      var before = @"
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

      var after = @"
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
      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }

    [Test]
    public void CodeFix_WithOneWhitespaceInImplicitObjectCreationExpression()
    {
      var before = @"
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

      var after = @"
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
      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }
  }
}