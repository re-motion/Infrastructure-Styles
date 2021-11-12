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
  public class RMSYN1007NoWhitespaceInCastCodeFixProviderTests
  {
    private static readonly RMSYN1007NoWhitespaceInCastAnalyzer Analyzer = new();
    private static readonly RMSYN1007NoWhitespaceInCastCodeFixProvider CodeFixProvider = new();

    [Test]
    public void CodeFix_WithNoWhitespace()
    {
      var before = @"
namespace N
{
    class C
    {
      private void ibimsafunktion() { var a = (int)↓ 3;}
    }
}";

      var after = @"
namespace N
{
    class C
    {
      private void ibimsafunktion() { var a = (int)3;}
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
      private void ibimsafunktion() { var a = (int)↓   3;}
    }
}";

      var after = @"
namespace N
{
    class C
    {
      private void ibimsafunktion() { var a = (int)3;}
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
      private void ibimsafunktion() { var a = (int)↓
           3;}
    }
}";

      var after = @"
namespace N
{
    class C
    {
      private void ibimsafunktion() { var a = (int)3;}
    }
}";
      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }
  }
}