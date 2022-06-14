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
  public class RMSYN1102SeparateExitConditionsCodeFixProviderTests
  {
    private static readonly RMSYN1102SeparateExitConditionsAnalyzer Analyzer = new();
    private static readonly RMSYN1102SeparateExitConditionsCodeFixProvider CodeFixProvider = new();

    [Test]
    public void CodeFix_SimpleConditionWithReturn_CreatesProperIFStatements ()
    {
      var before = @"
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
      var after = @"
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
      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }
    
    [Test]
    public void CodeFix_ComplexConditionWithReturnStatement_CreatesProperIFStatements ()
    {
        var before = @"
namespace N
{
    class C
    {

        private void Main()
        {
            if(↓(1 > 2 && 3 > 4) || (2 > 1 || 4 > 5))
                return;
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
            if((1 > 2 && 3 > 4))
                return;
            if(2 > 1)
                return;
            if(4 > 5)
                return;
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }
    
    [Test]
    public void CodeFix_ComplexConditionElseIfWithReturnStatement_CreatesProperIFStatements ()
    {
        var before = @"
namespace N
{
    class C
    {

        private void Main()
        {
            if(1 > 2 && 3 > 4)
                return;
            else if(↓2 > 1 || 4 > 5)
                return;
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
            if(1 > 2 && 3 > 4)
                return;
            else if(2 > 1)
                return;
            else if(4 > 5)
                return;
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }
    
    [Test]
    public void CodeFix_SimpleOrPatterConditionWithReturnStatement_CreatesProperIFStatements ()
    {
        var before = @"
namespace N
{
    class C
    {

        private void Main()
        {
            int a = 0;
            if(a is ↓1 or 3)
                return;
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
            int a = 0;
            if(a is 1)
                return;
            if(a is 3)
                return;
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }
    
    [Test]
    public void CodeFix_ComplexOrPatterConditionWithReturnStatement_CreatesProperIFStatements ()
    {
        var before = @"
namespace N
{
    class C
    {

        private void Main()
        {
            int a = 0;
            if(a is ↓1 or > 3 and > 4 or 2)
                return;
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
            int a = 0;
            if(a is 1)
                return;
            if(a is > 3 and > 4)
                return;
            if(a is 2)
                return;
        }
    }
}";
        RoslynAssert.FixAll(Analyzer, CodeFixProvider, before, after);
    }
  }
}