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
  public class RMSYN1003WhitespaceBeforeKeywordExpressionCodeFixProviderTests
  {
    private static readonly RMSYN1003WhitespaceBeforeKeywordExpressionAnalyzer Analyzer = new();
    private static readonly RMSYN1003WhitespaceBeforeKeywordExpressionCodeFixProvider CodeFixProvider = new();

    [Test]
    public void CodeFix_TypeOfWithOneWhitespace ()
    {
      var before = @"
namespace N
{
    class C
    {
      private System.Type StringType () => typeof↓ (string);
    }
}";

      var after = @"
namespace N
{
    class C
    {
      private System.Type StringType () => typeof(string);
    }
}";

      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }

    [Test]
    public void CodeFix_TypeOfWithMultipleWhitespaces ()
    {
      var before = @"
namespace N
{
    class C
    {
      private System.Type StringType () => typeof↓    (string);
    }
}";

      var after = @"
namespace N
{
    class C
    {
      private System.Type StringType () => typeof(string);
    }
}";

      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }

    [Test]
    public void CodeFix_TypeOfWithNewlineAndWhitespaces ()
    {
      var before = @"
namespace N
{
    class C
    {
      private System.Type StringType () => typeof↓    
          (string);
    }
}";

      var after = @"
namespace N
{
    class C
    {
      private System.Type StringType () => typeof(string);
    }
}";

      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }

    [Test]
    public void CodeFix_BaseWithOneWhitespace ()
    {
      var before = @"
namespace N
{
    class C
    {
      public C(): base↓ () {}
    }
}";

      var after = @"
namespace N
{
    class C
    {
      public C(): base() {}
    }
}";

      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }

    [Test]
    public void CodeFix_BaseWithMultipleWhitespaces ()
    {
      var before = @"
namespace N
{
    class C
    {
      public C(): base↓    () {}
    }
}";

      var after = @"
namespace N
{
    class C
    {
      public C(): base() {}
    }
}";

      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }

    [Test]
    public void CodeFix_BaseWithNewlineAndWhitespaces ()
    {
      var before = @"
namespace N
{
    class C
    {
      public C(): base↓
                    () {}
    }
}";

      var after = @"
namespace N
{
    class C
    {
      public C(): base() {}
    }
}";

      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }

    [Test]
    public void CodeFix_ThisWithOneWhitespace ()
    {
      var before = @"
namespace N
{
    class C
    {
      public C(): this↓ (3) {}
      public C(int a) {}
    }
}";

      var after = @"
namespace N
{
    class C
    {
      public C(): this(3) {}
      public C(int a) {}
    }
}";

      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }

    [Test]
    public void CodeFix_ThisWithMultipleWhitespaces ()
    {
      var before = @"
namespace N
{
    class C
    {
      public C(): this↓    (3) {}
      public C(int a) {}
    }
}";

      var after = @"
namespace N
{
    class C
    {
      public C(): this(3) {}
      public C(int a) {}
    }
}";

      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }

    [Test]
    public void CodeFix_ThisWithNewlineAndWhitespaces ()
    {
      var before = @"
namespace N
{
    class C
    {
      public C(): this↓
                    (3) {}
      public C(int a) {}
    }
}";

      var after = @"
namespace N
{
    class C
    {
      public C(): this(3) {}
      public C(int a) {}
    }
}";

      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }
  }
}